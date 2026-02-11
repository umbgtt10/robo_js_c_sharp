using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboticControl.Application.DTOs;
using RoboticControl.Application.Services;
using RoboticControl.Domain.Exceptions;

namespace RoboticControl.API.Controllers;

/// <summary>
/// Controller for robot control operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RobotController : ControllerBase
{
    private readonly ILogger<RobotController> _logger;
    private readonly RobotControlService _robotService;

    public RobotController(ILogger<RobotController> logger, RobotControlService robotService)
    {
        _logger = logger;
        _robotService = robotService;
    }

    /// <summary>
    /// Get current robot position
    /// </summary>
    [HttpGet("position")]
    [ProducesResponseType(typeof(RobotPositionDto), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<RobotPositionDto>> GetPosition(CancellationToken cancellationToken)
    {
        try
        {
            var position = await _robotService.GetCurrentPositionAsync(cancellationToken);
            return Ok(position);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Hardware connection error");
            return StatusCode(500, new { error = "Hardware connection error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get current robot status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(RobotStatusDto), 200)]
    public async Task<ActionResult<RobotStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _robotService.GetSystemStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Move robot to absolute position
    /// </summary>
    [HttpPost("move")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Move([FromBody] MoveCommandDto command, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _robotService.MoveToPositionAsync(command, cancellationToken);

            if (success)
            {
                return Ok(new { message = "Move command executed successfully" });
            }

            return StatusCode(500, new { error = "Move command failed" });
        }
        catch (CommandValidationException ex)
        {
            _logger.LogWarning(ex, "Move command validation failed");
            return BadRequest(new { error = "Validation error", message = ex.Message });
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Hardware connection error during move");
            return StatusCode(500, new { error = "Hardware connection error", message = ex.Message });
        }
    }

    /// <summary>
    /// Jog robot (relative movement)
    /// </summary>
    [Authorize]
    [HttpPost("jog")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Jog([FromBody] JogCommandDto command, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _robotService.JogAsync(command, cancellationToken);

            if (success)
            {
                return Ok(new { message = "Jog command executed successfully" });
            }

            return StatusCode(500, new { error = "Jog command failed" });
        }
        catch (CommandValidationException ex)
        {
            _logger.LogWarning(ex, "Jog command validation failed");
            return BadRequest(new { error = "Validation error", message = ex.Message });
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Hardware connection error during jog");
            return StatusCode(500, new { error = "Hardware connection error", message = ex.Message });
        }
    }

    /// <summary>
    /// Execute emergency stop
    /// </summary>
    [Authorize]
    [HttpPost("emergency-stop")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> EmergencyStop()
    {
        var success = await _robotService.ExecuteEmergencyStopAsync();

        if (success)
        {
            return Ok(new { message = "Emergency stop executed" });
        }

        return StatusCode(500, new { error = "Emergency stop failed" });
    }

    /// <summary>
    /// Execute homing sequence
    /// </summary>
    [HttpPost("home")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Home(CancellationToken cancellationToken)
    {
        try
        {
            var success = await _robotService.HomeAsync(cancellationToken);

            if (success)
            {
                return Ok(new { message = "Homing sequence completed" });
            }

            return StatusCode(500, new { error = "Homing sequence failed" });
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Hardware connection error during homing");
            return StatusCode(500, new { error = "Hardware connection error", message = ex.Message });
        }
    }

    /// <summary>
    /// Reset error state
    /// </summary>
    [Authorize]
    [HttpPost("reset-error")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ResetError()
    {
        var success = await _robotService.ResetErrorAsync();

        if (success)
        {
            return Ok(new { message = "Error state reset" });
        }

        return StatusCode(500, new { error = "Failed to reset error" });
    }
}
