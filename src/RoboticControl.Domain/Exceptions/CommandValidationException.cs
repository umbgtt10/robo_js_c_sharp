namespace RoboticControl.Domain.Exceptions;

/// <summary>
/// Exception thrown when command validation fails
/// </summary>
public class CommandValidationException : Exception
{
    public CommandValidationException() : base("Command validation failed")
    {
    }

    public CommandValidationException(string message) : base(message)
    {
    }

    public CommandValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
