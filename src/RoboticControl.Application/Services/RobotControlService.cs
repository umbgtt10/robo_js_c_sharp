using AutoMapper;
using Microsoft.Extensions.Logging;
using RoboticControl.Application.DTOs;
using RoboticControl.Domain.Entities;
using RoboticControl.Domain.Enums;
using RoboticControl.Domain.Exceptions;
using RoboticControl.Domain.Interfaces;

namespace RoboticControl.Application.Services;

/// <summary>
/// Application service for robot control operations
/// </summary>
public class RobotControlService
{
    private readonly ILogger<RobotControlService> _logger;
    private readonly IRobotHardwareService _hardwareService;
    private readonly IMapper _mapper;
    private readonly WorkEnvelope _workEnvelope;
    private RobotPosition? _cachedPosition;
    private DateTime _lastPositionUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMilliseconds(50);

    public RobotControlService(
        ILogger<RobotControlService> logger,
        IRobotHardwareService hardwareService,
        IMapper mapper)
    {
        _logger = logger;
        _hardwareService = hardwareService;
        _mapper = mapper;
        _workEnvelope = WorkEnvelope.Default;

        // Subscribe to hardware events
        _hardwareService.PositionChanged += OnPositionChanged;
        _hardwareService.StatusChanged += OnStatusChanged;
    }

    public async Task<RobotPositionDto> GetCurrentPositionAsync(CancellationToken cancellationToken = default)
    {
        // Return cached position if still valid
        if (_cachedPosition != null && DateTime.UtcNow - _lastPositionUpdate < _cacheExpiration)
        {
            return _mapper.Map<RobotPositionDto>(_cachedPosition);
        }

        try
        {
            var position = await _hardwareService.GetCurrentPositionAsync(cancellationToken);
            _cachedPosition = position;
            _lastPositionUpdate = DateTime.UtcNow;
            return _mapper.Map<RobotPositionDto>(position);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Failed to get current position");
            throw;
        }
    }

    public async Task<RobotStatusDto> GetSystemStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _hardwareService.GetStatusAsync(cancellationToken);
            return _mapper.Map<RobotStatusDto>(status);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Failed to get system status");

            // Return disconnected status
            return new RobotStatusDto
            {
                IsConnected = false,
                State = RobotState.Disconnected.ToString(),
                ErrorCode = (int)ErrorCode.ConnectionLost,
                ErrorMessage = "Hardware connection lost",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<bool> MoveToPositionAsync(MoveCommandDto command, CancellationToken cancellationToken = default)
    {
        var targetPosition = _mapper.Map<RobotPosition>(command);

        // Validate work envelope
        if (!_workEnvelope.IsWithinBounds(targetPosition))
        {
            throw new CommandValidationException(
                $"Target position ({targetPosition.X}, {targetPosition.Y}, {targetPosition.Z}) " +
                "exceeds work envelope boundaries");
        }

        try
        {
            _logger.LogInformation("Moving to position: X={X}, Y={Y}, Z={Z}",
                targetPosition.X, targetPosition.Y, targetPosition.Z);

            var success = await _hardwareService.MoveToPositionAsync(targetPosition, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Move command completed successfully");
            }

            return success;
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Failed to execute move command");
            throw;
        }
    }

    public async Task<bool> JogAsync(JogCommandDto command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current position to validate resulting position
            var currentPosition = await _hardwareService.GetCurrentPositionAsync(cancellationToken);
            var resultingPosition = new RobotPosition(
                currentPosition.X + command.DeltaX,
                currentPosition.Y + command.DeltaY,
                currentPosition.Z + command.DeltaZ
            );

            // Validate work envelope
            if (!_workEnvelope.IsWithinBounds(resultingPosition))
            {
                throw new CommandValidationException(
                    $"Resulting position ({resultingPosition.X}, {resultingPosition.Y}, {resultingPosition.Z}) " +
                    "would exceed work envelope boundaries");
            }

            _logger.LogInformation("Jogging: ΔX={DX}, ΔY={DY}, ΔZ={DZ}",
                command.DeltaX, command.DeltaY, command.DeltaZ);

            return await _hardwareService.MoveRelativeAsync(
                command.DeltaX, command.DeltaY, command.DeltaZ, cancellationToken);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Failed to execute jog command");
            throw;
        }
    }

    public async Task<bool> ExecuteEmergencyStopAsync()
    {
        _logger.LogWarning("Emergency stop requested");

        try
        {
            var success = await _hardwareService.EmergencyStopAsync();

            if (success)
            {
                _logger.LogWarning("Emergency stop executed successfully");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute emergency stop");
            return false;
        }
    }

    public async Task<bool> HomeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing homing sequence");
            return await _hardwareService.HomeAsync(cancellationToken);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Failed to execute homing sequence");
            throw;
        }
    }

    public async Task<bool> ResetErrorAsync()
    {
        try
        {
            _logger.LogInformation("Resetting error state");
            return await _hardwareService.ResetErrorAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset error");
            return false;
        }
    }

    public WorkEnvelopeDto GetWorkEnvelope()
    {
        return _mapper.Map<WorkEnvelopeDto>(_workEnvelope);
    }

    private void OnPositionChanged(object? sender, RobotPosition position)
    {
        _cachedPosition = position;
        _lastPositionUpdate = DateTime.UtcNow;
    }

    private void OnStatusChanged(object? sender, RobotStatus status)
    {
        _logger.LogDebug("Status changed: {State}, Error: {Error}", status.State, status.ErrorCode);
    }
}
