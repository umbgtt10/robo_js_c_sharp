namespace RoboticControl.Application.DTOs;

/// <summary>
/// Data transfer object for move commands
/// </summary>
public class MoveCommandDto
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double? RotationX { get; set; }
    public double? RotationY { get; set; }
    public double? RotationZ { get; set; }
}
