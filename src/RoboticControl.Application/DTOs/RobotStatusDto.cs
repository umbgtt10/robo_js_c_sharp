namespace RoboticControl.Application.DTOs;

/// <summary>
/// Data transfer object for robot status
/// </summary>
public class RobotStatusDto
{
    public bool IsConnected { get; set; }
    public string State { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public double LoadPercentage { get; set; }
    public DateTime Timestamp { get; set; }
}
