using Microsoft.AspNetCore.SignalR;
using RoboticControl.Domain.Entities;
using RoboticControl.Domain.Interfaces;
using AutoMapper;
using RoboticControl.Application.DTOs;

namespace RoboticControl.API.BackgroundServices;

/// <summary>
/// Background service that polls robot position and broadcasts updates via SignalR
/// </summary>
public class PositionPollingService : BackgroundService
{
    private readonly ILogger<PositionPollingService> _logger;
    private readonly IHubContext<Hubs.RobotHub> _hubContext;
    private readonly IRobotHardwareService _hardwareService;
    private readonly IMapper _mapper;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMilliseconds(100);

    public PositionPollingService(
        ILogger<PositionPollingService> logger,
        IHubContext<Hubs.RobotHub> hubContext,
        IRobotHardwareService hardwareService,
        IMapper mapper)
    {
        _logger = logger;
        _hubContext = hubContext;
        _hardwareService = hardwareService;
        _mapper = mapper;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Position polling service starting");

        // Wait for initial connection
        while (!_hardwareService.IsConnected && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        // Subscribe to hardware events
        _hardwareService.PositionChanged += OnPositionChanged;
        _hardwareService.StatusChanged += OnStatusChanged;

        _logger.LogInformation("Position polling service started");

        // Keep service alive
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Normal shutdown
        }

        _logger.LogInformation("Position polling service stopped");
    }

    private async void OnPositionChanged(object? sender, RobotPosition position)
    {
        try
        {
            var positionDto = _mapper.Map<RobotPositionDto>(position);
            await _hubContext.Clients.All.SendAsync("PositionUpdate", positionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting position update");
        }
    }

    private async void OnStatusChanged(object? sender, RobotStatus status)
    {
        try
        {
            var statusDto = _mapper.Map<RobotStatusDto>(status);
            await _hubContext.Clients.All.SendAsync("StatusUpdate", statusDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting status update");
        }
    }
}
