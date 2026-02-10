namespace RoboticControl.Domain.Enums;

/// <summary>
/// Status of the connection to the hardware
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// Not connected to hardware
    /// </summary>
    Disconnected,

    /// <summary>
    /// Attempting to establish connection
    /// </summary>
    Connecting,

    /// <summary>
    /// Successfully connected to hardware
    /// </summary>
    Connected,

    /// <summary>
    /// Connection was lost, attempting to reconnect
    /// </summary>
    Reconnecting,

    /// <summary>
    /// Connection failed
    /// </summary>
    Failed
}
