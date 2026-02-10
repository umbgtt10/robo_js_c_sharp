using RoboticControl.Domain.Entities;

namespace RoboticControl.Domain.Interfaces;

/// <summary>
/// Interface for hardware communication with the robot
/// </summary>
public interface IRobotHardwareService
{
    /// <summary>
    /// Event raised when robot position changes
    /// </summary>
    event EventHandler<RobotPosition>? PositionChanged;

    /// <summary>
    /// Event raised when robot status changes
    /// </summary>
    event EventHandler<RobotStatus>? StatusChanged;

    /// <summary>
    /// Connect to the robot hardware
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the robot hardware
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Get current position from the robot
    /// </summary>
    Task<RobotPosition> GetCurrentPositionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current status from the robot
    /// </summary>
    Task<RobotStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Move robot to absolute position
    /// </summary>
    Task<bool> MoveToPositionAsync(RobotPosition targetPosition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move robot relative to current position
    /// </summary>
    Task<bool> MoveRelativeAsync(double deltaX, double deltaY, double deltaZ, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute emergency stop
    /// </summary>
    Task<bool> EmergencyStopAsync();

    /// <summary>
    /// Execute homing sequence
    /// </summary>
    Task<bool> HomeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset error state
    /// </summary>
    Task<bool> ResetErrorAsync();

    /// <summary>
    /// Check if connected to hardware
    /// </summary>
    bool IsConnected { get; }
}
