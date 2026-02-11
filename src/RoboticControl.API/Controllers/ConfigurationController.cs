using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RoboticControl.Application.DTOs;
using RoboticControl.Application.Services;

namespace RoboticControl.API.Controllers;

/// <summary>
/// Controller for system configuration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly RobotControlService _robotService;

    public ConfigurationController(ILogger<ConfigurationController> logger, RobotControlService robotService)
    {
        _logger = logger;
        _robotService = robotService;
    }

    /// <summary>
    /// Get work envelope configuration
    /// </summary>
    [HttpGet("work-envelope")]
    [EnableRateLimiting("config")]
    [ProducesResponseType(typeof(WorkEnvelopeDto), 200)]
    public ActionResult<WorkEnvelopeDto> GetWorkEnvelope()
    {
        var workEnvelope = _robotService.GetWorkEnvelope();
        return Ok(workEnvelope);
    }

    /// <summary>
    /// Get connection settings (placeholder)
    /// </summary>
    [HttpGet("connection")]
    [EnableRateLimiting("config")]
    [ProducesResponseType(200)]
    public ActionResult GetConnectionSettings()
    {
        // Placeholder - would return actual connection settings
        return Ok(new
        {
            host = "localhost",
            port = 5000,
            connectionStatus = "connected"
        });
    }
}
