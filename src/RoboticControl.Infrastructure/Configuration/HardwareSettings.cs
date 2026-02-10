namespace RoboticControl.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for hardware connection
/// </summary>
public class HardwareSettings
{
    public const string SectionName = "Hardware";

    /// <summary>
    /// IP address or hostname of the robot controller
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// TCP port for robot communication
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectionTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Command timeout in milliseconds
    /// </summary>
    public int CommandTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Position polling interval in milliseconds
    /// </summary>
    public int PollingIntervalMs { get; set; } = 100;

    /// <summary>
    /// Maximum reconnection attempts (-1 for infinite)
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = -1;

    /// <summary>
    /// Base reconnection delay in milliseconds (will use exponential backoff)
    /// </summary>
    public int ReconnectDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum reconnection delay in milliseconds
    /// </summary>
    public int MaxReconnectDelayMs { get; set; } = 16000;
}
