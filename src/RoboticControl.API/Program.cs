using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
