using FluentValidation;
using RoboticControl.Application.Mappings;
using RoboticControl.Application.Services;
using RoboticControl.Application.Validators;
using RoboticControl.Domain.Interfaces;
using RoboticControl.Infrastructure.Configuration;
using RoboticControl.Infrastructure.Hardware;
using Serilog;

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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Not using authentication/authorization in this simple example, but if we were, we would add app.UseAuthentication() here before app.UseAuthorization()
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
