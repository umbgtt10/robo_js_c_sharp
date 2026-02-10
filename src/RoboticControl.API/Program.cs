using FluentValidation;
using RoboticControl.Application.Mappings;
using RoboticControl.Application.Services;
using RoboticControl.Application.Validators;
using RoboticControl.Domain.Interfaces;
using RoboticControl.Infrastructure.Configuration;
using RoboticControl.Infrastructure.Hardware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/robotcontrol-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
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

// Configure CORS
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
builder.Services.AddValidatorsFromAssemblyContaining<MoveCommandValidator>();

// Register hardware services
builder.Services.AddSingleton<TcpRobotClient>();
builder.Services.AddSingleton<IRobotHardwareService, RobotHardwareService>();
builder.Services.AddScoped<RobotControlService>();

// Add background services (order matters: connection first, then polling)
builder.Services.AddHostedService<RoboticControl.API.BackgroundServices.HardwareConnectionService>();
builder.Services.AddHostedService<RoboticControl.API.BackgroundServices.PositionPollingService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

app.MapControllers();
app.MapHub<RoboticControl.API.Hubs.RobotHub>("/hubs/robot");

// Health check endpoint
app.MapGet("/health", () => new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
});

app.Run();
