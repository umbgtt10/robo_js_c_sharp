using FluentValidation;
using FluentValidation.AspNetCore;
using RoboticControl.API.Middleware;
using RoboticControl.Application.Mappings;
using RoboticControl.Application.Services;
using RoboticControl.Application.Validators;
using RoboticControl.Domain.Interfaces;
using RoboticControl.Infrastructure.Configuration;
using RoboticControl.Infrastructure.Hardware;

namespace RoboticControl.API.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add application services, validators, and infrastructure dependencies
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add services to the container (scan all the assemblies looking for ControllerBase and subscribe/inject them in the DI container)
        services.AddControllers();

        // Configure FluentValidation to validate automatically before controller actions
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Robotic Control API",
                Version = "v1",
                Description = "Web API for controlling robotic systems via TCP/IP"
            });
        });

        // Configure CORS (Cross-Origin Resource Sharing) => Accept request from frontend (e.g. http://localhost:5173)
        var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173"];

        services.AddCors(options =>
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
        services.AddSignalR();

        // Configure Hardware settings
        services.Configure<HardwareSettings>(
            configuration.GetSection(HardwareSettings.SectionName));

        // Register application services
        services.AddAutoMapper(typeof(MappingProfile));

        // This will scan the assembly for any class that inherits from AbstractValidator<T> and register it in the DI container
        services.AddValidatorsFromAssemblyContaining<MoveCommandValidator>();

        // Created once and then shared across the application (singleton) - manages the TCP connection to the robot hardware
        services.AddSingleton<TcpRobotClient>();

        // Created once and then shared across the application (singleton) - provides higher-level operations and event handling on top of the TCP client
        services.AddSingleton<IRobotHardwareService, RobotHardwareService>();

        // Created on demand for the API controllers to use the hardware service and perform operations like move, stop, home, etc.
        services.AddScoped<RobotControlService>();

        // Add background services (order matters: connection first, then event broadcast): This run all the time and independently of the API controllers - it will try to connect to the hardware on startup and then subscribe to events and broadcast them via SignalR
        services.AddHostedService<BackgroundServices.HardwareConnectionService>();
        services.AddHostedService<BackgroundServices.HardwareEventBroadcastService>();

        // Configure global exception handler for consistent error responses
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
