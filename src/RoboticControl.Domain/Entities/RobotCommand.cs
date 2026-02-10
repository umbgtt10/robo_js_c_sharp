using RoboticControl.Domain.Enums;

namespace RoboticControl.Domain.Entities;

/// <summary>
/// Represents a command to be executed by the robot
/// </summary>
public class RobotCommand
{
    /// <summary>
    /// Unique identifier for the command
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of command
    /// </summary>
    public CommandType CommandType { get; set; }

    /// <summary>
    /// Target position for movement commands
    /// </summary>
    public RobotPosition? TargetPosition { get; set; }

    /// <summary>
    /// Command parameters as key-value pairs
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }

    /// <summary>
    /// Current status of command execution
    /// </summary>
    public CommandStatus Status { get; set; }

    /// <summary>
    /// When the command was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the command execution started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// When the command execution completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if command failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    public RobotCommand()
    {
        Id = Guid.NewGuid();
        Parameters = new Dictionary<string, object>();
        Status = CommandStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Status of command execution
/// </summary>
public enum CommandStatus
{
    Pending,
    Executing,
    Completed,
    Failed,
    Cancelled
}
