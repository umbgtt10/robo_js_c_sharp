namespace RoboticControl.Application.DTOs;

/// <summary>
/// Data transfer object for relative movement (jog)
/// </summary>
public class JogCommandDto
{
    public double DeltaX { get; set; }
    public double DeltaY { get; set; }
    public double DeltaZ { get; set; }
}
