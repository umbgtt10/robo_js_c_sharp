using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using RoboticControl.API.Middleware;
using RoboticControl.Application.Mappings;
using RoboticControl.Application.Services;
using RoboticControl.Application.Validators;
using RoboticControl.Domain.Interfaces;
using RoboticControl.Infrastructure.Configuration;
using RoboticControl.Infrastructure.Hardware;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging with console and file sinks
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/robotcontrol-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container (scan all the assemblies looking for ControllerBase and subscribe/inject them in the DI container)
builder.Services.AddControllers();

// Configure FluentValidation to validate automatically before controller actions
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Robotic Control API",
        Version = "v1",
        Description = "Web API for controlling robotic systems via TCP/IP"
    });
});

// Configure CORS (Cross-Origin Resource Sharing) => Accept request from frontend (e.g. http://localhost:5173)
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});

// Configure JWT Authentication
// In production, JWT secret MUST be provided via environment variable JWT_SECRET_KEY
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException(
        "JWT Secret Key not configured. " +
        "Set JWT_SECRET_KEY environment variable or JwtSettings:SecretKey in appsettings.json");

// Validate key length for security
if (secretKey.Length < 32)
{
    throw new InvalidOperationException(
        $"JWT Secret Key must be at least 32 characters. Current length: {secretKey.Length}");
}

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "RoboticControlAPI";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "RoboticControlClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    // Policy: Both Admin and Operator can operate the robot
    options.AddPolicy("CanOperate", policy =>
        policy.RequireRole("Admin", "Operator"));

    // Policy: Only Admin for critical/dangerous operations
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Configure Rate Limiting to protect hardware and prevent abuse
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter for all requests (per IP address)
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetTokenBucketLimiter(ipAddress, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 200,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 200,
            AutoReplenishment = true
        });
    });

    // Auth endpoints (login) - Strict limit to prevent brute force attacks
    // 5 attempts per minute per IP address
    options.AddPolicy("auth", context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 2
        });
    });

    // Robot command endpoints (move, stop, home) - Protect hardware from command flooding
    // 60 commands per minute per authenticated user
    options.AddPolicy("commands", context =>
    {
        var username = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(username, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10
        });
    });

    // Robot query endpoints (position, status) - More lenient for monitoring
    // 100 queries per minute per authenticated user, sliding window for smoother distribution
    options.AddPolicy("queries", context =>
    {
        var username = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetSlidingWindowLimiter(username, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6, // 10-second segments
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 20
        });
    });

    // Configuration endpoint - Very strict
    options.AddPolicy("config", context =>
    {
        var username = context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(username, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    // Customize rejection response
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? retryAfterValue.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Too many requests. Please slow down.",
            retryAfter = $"{retryAfter} seconds",
            timestamp = DateTime.UtcNow
        }, cancellationToken: cancellationToken);
    };
});

// Configure SignalR
builder.Services.AddSignalR();

// Configure Hardware settings
builder.Services.Configure<HardwareSettings>(
    builder.Configuration.GetSection(HardwareSettings.SectionName));

// Register application services
builder.Services.AddAutoMapper(typeof(MappingProfile));

// This will scan the assembly for any class that inherits from AbstractValidator<T> and register it in the DI container
builder.Services.AddValidatorsFromAssemblyContaining<MoveCommandValidator>();

// Created once and then shared across the application (singleton) - manages the TCP connection to the robot hardware
builder.Services.AddSingleton<TcpRobotClient>();

// Created once and then shared across the application (singleton) - provides higher-level operations and event handling on top of the TCP client
builder.Services.AddSingleton<IRobotHardwareService, RobotHardwareService>();

// Created on demand for the API controllers to use the hardware service and perform operations like move, stop, home, etc.
builder.Services.AddScoped<RobotControlService>();

// Add background services (order matters: connection first, then event broadcast): This run all the time and independently of the API controllers - it will try to connect to the hardware on startup and then subscribe to events and broadcast them via SignalR
builder.Services.AddHostedService<RoboticControl.API.BackgroundServices.HardwareConnectionService>();
builder.Services.AddHostedService<RoboticControl.API.BackgroundServices.HardwareEventBroadcastService>();

// Configure global exception handler for consistent error responses
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline

// Exception handler must be first to catch all exceptions
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Rate limiting (after CORS, before Authentication)
app.UseRateLimiter();

// Authentication and Authorization (order matters: Authentication before Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Create SignalR hub endpoint for real-time updates to the frontend (e.g. position and status updates) - this is where the frontend will connect to receive real-time notifications from the server about hardware events. The HardwareEventBroadcastService will push updates to this hub whenever the robot's position or status changes.
app.MapHub<RoboticControl.API.Hubs.RobotHub>("/hubs/robot");

// Minimal health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
});

app.Run();
