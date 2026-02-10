namespace RoboticControl.Domain.Exceptions;

/// <summary>
/// Exception thrown when hardware connection fails or is lost
/// </summary>
public class HardwareConnectionException : Exception
{
    public HardwareConnectionException() : base("Hardware connection error")
    {
    }

    public HardwareConnectionException(string message) : base(message)
    {
    }

    public HardwareConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
