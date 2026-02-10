namespace RoboticControl.Application.DTOs;

/// <summary>
/// Data transfer object for work envelope configuration
/// </summary>
public class WorkEnvelopeDto
{
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double ZMin { get; set; }
    public double ZMax { get; set; }
}
