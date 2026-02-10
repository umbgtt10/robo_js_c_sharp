using Microsoft.AspNetCore.SignalR;
using RoboticControl.Domain.Entities;
using RoboticControl.Domain.Interfaces;
using AutoMapper;
using RoboticControl.Application.DTOs;

namespace RoboticControl.API.BackgroundServices;

/// <summary>
/// Background service that subscribes to hardware events and broadcasts real-time updates via SignalR.
/// Uses event-driven architecture rather than polling - responds to PositionChanged and StatusChanged events
/// from the hardware service and relays them to all connected SignalR clients.
/// </summary>
public class HardwareEventBroadcastService : BackgroundService
{
    private readonly ILogger<HardwareEventBroadcastService> _logger;
    private readonly IHubContext<Hubs.RobotHub> _hubContext;
    private readonly IRobotHardwareService _hardwareService;
    private readonly IMapper _mapper;

    public HardwareEventBroadcastService(
        ILogger<HardwareEventBroadcastService> logger,
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
        _logger.LogInformation("Hardware event broadcast service starting");

        // Wait for initial connection
        while (!_hardwareService.IsConnected && !stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        // Subscribe to hardware events (event-driven, not polling)
        _hardwareService.PositionChanged += OnPositionChanged;
        _hardwareService.StatusChanged += OnStatusChanged;

        _logger.LogInformation("Hardware event broadcast service started");

        // Keep service alive
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Normal shutdown
        }

        _logger.LogInformation("Hardware event broadcast service stopped");
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
