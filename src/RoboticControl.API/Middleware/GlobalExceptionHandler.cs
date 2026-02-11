using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RoboticControl.API.Models;
using RoboticControl.Domain.Exceptions;

namespace RoboticControl.API.Middleware;

/// <summary>
/// Global exception handler for consistent error responses
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var (statusCode, message, errors) = exception switch
        {
            CommandValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation error",
                new List<string> { ex.Message }
            ),

            HardwareConnectionException ex => (
                StatusCodes.Status503ServiceUnavailable,
                "Hardware connection error",
                new List<string> { ex.Message }
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Access denied",
                new List<string> { "You don't have permission to access this resource" }
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                new List<string> {
                    httpContext.RequestServices
                        .GetRequiredService<IWebHostEnvironment>()
                        .IsDevelopment()
                        ? exception.Message
                        : "Please contact support"
                }
            )
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse.ErrorResponse(message, errors);
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true; // Exception handled
    }
}
