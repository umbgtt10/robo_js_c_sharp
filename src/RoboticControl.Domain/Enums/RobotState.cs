namespace RoboticControl.Domain.Enums;

/// <summary>
/// Represents the operational state of the robot
/// </summary>
public enum RobotState
{
    /// <summary>
    /// Robot is disconnected from the control system
    /// </summary>
    Disconnected,

    /// <summary>
    /// Robot is idle and ready to receive commands
    /// </summary>
    Idle,

    /// <summary>
    /// Robot is currently executing a movement command
    /// </summary>
    Moving,

    /// <summary>
    /// Robot is in emergency stop state
    /// </summary>
    EmergencyStopped,

    /// <summary>
    /// Robot is in error state requiring intervention
    /// </summary>
    Error,

    /// <summary>
    /// Robot is executing homing sequence
    /// </summary>
    Homing
}
