using RoboticControl.Domain.Enums;

namespace RoboticControl.Domain.Entities;

/// <summary>
/// Represents the current status of the robot system
/// </summary>
public class RobotStatus
{
    /// <summary>
    /// Whether the robot is connected to the control system
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// Current operational state of the robot
    /// </summary>
    public RobotState State { get; set; }

    /// <summary>
    /// Current robot temperature in Celsius
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Current error code (None if no error)
    /// </summary>
    public ErrorCode ErrorCode { get; set; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when status was collected
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Current load percentage (0-100)
    /// </summary>
    public double LoadPercentage { get; set; }

    public RobotStatus()
    {
        Timestamp = DateTime.UtcNow;
        State = RobotState.Disconnected;
        ErrorCode = ErrorCode.None;
    }
}
