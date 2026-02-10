namespace RoboticControl.Domain.Enums;

/// <summary>
/// Error codes that can be reported by the robot system
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// No error
    /// </summary>
    None = 0,

    /// <summary>
    /// Communication timeout with hardware
    /// </summary>
    CommunicationTimeout = 1,

    /// <summary>
    /// Movement exceeds work envelope boundaries
    /// </summary>
    WorkEnvelopeViolation = 2,

    /// <summary>
    /// Invalid command parameters
    /// </summary>
    InvalidParameters = 3,

    /// <summary>
    /// Robot is in invalid state for requested operation
    /// </summary>
    InvalidState = 4,

    /// <summary>
    /// Hardware connection lost
    /// </summary>
    ConnectionLost = 5,

    /// <summary>
    /// Temperature limit exceeded
    /// </summary>
    TemperatureExceeded = 6,

    /// <summary>
    /// Unknown error occurred
    /// </summary>
    UnknownError = 99
}
