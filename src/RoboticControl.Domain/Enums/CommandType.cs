namespace RoboticControl.Domain.Enums;

/// <summary>
/// Types of commands that can be sent to the robot
/// </summary>
public enum CommandType
{
    /// <summary>
    /// Move to absolute position
    /// </summary>
    MoveAbsolute,

    /// <summary>
    /// Move relative to current position (jog)
    /// </summary>
    MoveRelative,

    /// <summary>
    /// Execute homing sequence
    /// </summary>
    Home,

    /// <summary>
    /// Emergency stop - halt all operations
    /// </summary>
    EmergencyStop,

    /// <summary>
    /// Reset error state
    /// </summary>
    ResetError,

    /// <summary>
    /// Get current position
    /// </summary>
    GetPosition,

    /// <summary>
    /// Get system status
    /// </summary>
    GetStatus
}
