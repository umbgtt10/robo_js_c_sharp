namespace RoboticControl.Domain.Entities;

/// <summary>
/// Defines the work envelope (safe operating boundaries) for the robot
/// </summary>
public class WorkEnvelope
{
    /// <summary>
    /// Minimum X coordinate in millimeters
    /// </summary>
    public double XMin { get; set; }

    /// <summary>
    /// Maximum X coordinate in millimeters
    /// </summary>
    public double XMax { get; set; }

    /// <summary>
    /// Minimum Y coordinate in millimeters
    /// </summary>
    public double YMin { get; set; }

    /// <summary>
    /// Maximum Y coordinate in millimeters
    /// </summary>
    public double YMax { get; set; }

    /// <summary>
    /// Minimum Z coordinate in millimeters
    /// </summary>
    public double ZMin { get; set; }

    /// <summary>
    /// Maximum Z coordinate in millimeters
    /// </summary>
    public double ZMax { get; set; }

    /// <summary>
    /// Check if a position is within the work envelope
    /// </summary>
    public bool IsWithinBounds(RobotPosition position)
    {
        return position.X >= XMin && position.X <= XMax &&
               position.Y >= YMin && position.Y <= YMax &&
               position.Z >= ZMin && position.Z <= ZMax;
    }

    /// <summary>
    /// Default work envelope (1000mm cube centered at origin)
    /// </summary>
    public static WorkEnvelope Default => new()
    {
        XMin = -500,
        XMax = 500,
        YMin = -500,
        YMax = 500,
        ZMin = 0,
        ZMax = 1000
    };
}
