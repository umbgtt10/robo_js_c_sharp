namespace RoboticControl.Domain.Entities;

/// <summary>
/// Represents the 3D position and orientation of the robot
/// </summary>
public class RobotPosition
{
    /// <summary>
    /// X-axis coordinate in millimeters
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y-axis coordinate in millimeters
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Z-axis coordinate in millimeters
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// Rotation around X-axis in degrees
    /// </summary>
    public double RotationX { get; set; }

    /// <summary>
    /// Rotation around Y-axis in degrees
    /// </summary>
    public double RotationY { get; set; }

    /// <summary>
    /// Rotation around Z-axis in degrees
    /// </summary>
    public double RotationZ { get; set; }

    /// <summary>
    /// Timestamp when position was recorded
    /// </summary>
    public DateTime Timestamp { get; set; }

    public RobotPosition()
    {
        Timestamp = DateTime.UtcNow;
    }

    public RobotPosition(double x, double y, double z, double rotX = 0, double rotY = 0, double rotZ = 0)
    {
        X = x;
        Y = y;
        Z = z;
        RotationX = rotX;
        RotationY = rotY;
        RotationZ = rotZ;
        Timestamp = DateTime.UtcNow;
    }
}
