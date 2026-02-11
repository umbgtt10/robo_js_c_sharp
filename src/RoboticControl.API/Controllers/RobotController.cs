using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboticControl.API.Models;
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
    [ProducesResponseType(typeof(ApiResponse<RobotPositionDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetPosition(CancellationToken cancellationToken)
    {
        var position = await _robotService.GetCurrentPositionAsync(cancellationToken);
        return Ok(ApiResponse<RobotPositionDto>.SuccessResponse(position, "Position retrieved successfully"));
    }

    /// <summary>
    /// Get current robot status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<RobotStatusDto>), 200)]
    public async Task<ActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var status = await _robotService.GetSystemStatusAsync(cancellationToken);
        return Ok(ApiResponse<RobotStatusDto>.SuccessResponse(status, "Status retrieved successfully"));
    }

    /// <summary>
    /// Move robot to absolute position
    /// </summary>
    [Authorize(Policy = "CanOperate")]
    [HttpPost("move")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Move([FromBody] MoveCommandDto command, CancellationToken cancellationToken)
    {
        var success = await _robotService.MoveToPositionAsync(command, cancellationToken);

        if (success)
        {
            return Ok(ApiResponse.SuccessResponse("Move command executed successfully"));
        }

        return StatusCode(500, ApiResponse.ErrorResponse("Move command failed"));
    }

    /// <summary>
    /// Jog robot (relative movement)
    /// </summary>
    [Authorize(Policy = "CanOperate")]
    [HttpPost("jog")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Jog([FromBody] JogCommandDto command, CancellationToken cancellationToken)
    {
        var success = await _robotService.JogAsync(command, cancellationToken);

        if (success)
        {
            return Ok(ApiResponse.SuccessResponse("Jog command executed successfully"));
        }

        return StatusCode(500, ApiResponse.ErrorResponse("Jog command failed"));
    }

    /// <summary>
    /// Execute emergency stop
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("emergency-stop")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> EmergencyStop()
    {
        var success = await _robotService.ExecuteEmergencyStopAsync();

        if (success)
        {
            return Ok(ApiResponse.SuccessResponse("Emergency stop executed"));
        }

        return StatusCode(500, ApiResponse.ErrorResponse("Emergency stop failed"));
    }

    /// <summary>
    /// Execute homing sequence
    /// </summary>
    [Authorize(Policy = "CanOperate")]
    [HttpPost("home")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Home(CancellationToken cancellationToken)
    {
        var success = await _robotService.HomeAsync(cancellationToken);

        if (success)
        {
            return Ok(ApiResponse.SuccessResponse("Homing sequence completed"));
        }

        return StatusCode(500, ApiResponse.ErrorResponse("Homing sequence failed"));
    }

    /// <summary>
    /// Reset error state
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("reset-error")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ResetError()
    {
        var success = await _robotService.ResetErrorAsync();

        if (success)
        {
            return Ok(ApiResponse.SuccessResponse("Error state reset"));
        }

        return StatusCode(500, ApiResponse.ErrorResponse("Failed to reset error"));
    }
}
