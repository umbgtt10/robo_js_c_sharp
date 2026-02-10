using Microsoft.AspNetCore.SignalR;
using RoboticControl.Application.DTOs;

namespace RoboticControl.API.Hubs;

/// <summary>
/// SignalR hub for real-time robot data streaming
/// </summary>
public class RobotHub : Hub
{
    private readonly ILogger<RobotHub> _logger;

    public RobotHub(ILogger<RobotHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcast position update to all connected clients
    /// </summary>
    public async Task BroadcastPosition(RobotPositionDto position)
    {
        await Clients.All.SendAsync("PositionUpdate", position);
    }

    /// <summary>
    /// Broadcast status update to all connected clients
    /// </summary>
    public async Task BroadcastStatus(RobotStatusDto status)
    {
        await Clients.All.SendAsync("StatusUpdate", status);
    }
}
