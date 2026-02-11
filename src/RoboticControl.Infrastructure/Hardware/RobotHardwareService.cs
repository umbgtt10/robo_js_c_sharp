using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoboticControl.Domain.Entities;
using RoboticControl.Domain.Enums;
using RoboticControl.Domain.Exceptions;
using RoboticControl.Domain.Interfaces;
using RoboticControl.Infrastructure.Configuration;

namespace RoboticControl.Infrastructure.Hardware;

/// <summary>
/// Implementation of robot hardware service with automatic reconnection
/// </summary>
public class RobotHardwareService : IRobotHardwareService, IDisposable
{
    private readonly ILogger<RobotHardwareService> _logger;
    private readonly TcpRobotClient _client;
    private readonly HardwareSettings _settings;
    private readonly SemaphoreSlim _commandLock = new(1, 1);
    private CancellationTokenSource? _reconnectCts;
    private Task? _reconnectTask;
    private int _reconnectAttempts;

    public event EventHandler<RobotPosition>? PositionChanged;
    public event EventHandler<RobotStatus>? StatusChanged;

    public bool IsConnected => _client.IsConnected;

    public RobotHardwareService(
        ILogger<RobotHardwareService> logger,
        TcpRobotClient client,
        IOptions<HardwareSettings> settings)
    {
        _logger = logger;
        _client = client;
        _settings = settings.Value;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connected = await _client.ConnectAsync(cancellationToken);

        if (connected)
        {
            _reconnectAttempts = 0;
            _reconnectCts?.Cancel();
            OnStatusChanged(new RobotStatus
            {
                IsConnected = true,
                State = RobotState.Idle,
                ErrorCode = ErrorCode.None
            });
        }

        return connected;
    }

    public async Task DisconnectAsync()
    {
        _reconnectCts?.Cancel();
        await Task.Delay(100); // Give reconnect task time to complete
        _client.Disconnect();

        OnStatusChanged(new RobotStatus
        {
            IsConnected = false,
            State = RobotState.Disconnected,
            ErrorCode = ErrorCode.None
        });
    }

    public async Task<RobotPosition> GetCurrentPositionAsync(CancellationToken cancellationToken = default)
    {
        await _commandLock.WaitAsync(cancellationToken);
        try
        {
            var position = await _client.GetPositionAsync(cancellationToken);
            OnPositionChanged(position);
            return position;
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to get robot position");
            StartReconnectionIfNeeded();
            throw;
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task<RobotStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        await _commandLock.WaitAsync(cancellationToken);
        try
        {
            var status = await _client.GetStatusAsync(cancellationToken);
            OnStatusChanged(status);
            return status;
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to get robot status");
            StartReconnectionIfNeeded();
            throw;
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task<bool> MoveToPositionAsync(RobotPosition targetPosition, CancellationToken cancellationToken = default)
    {
        await _commandLock.WaitAsync(cancellationToken);
        try
        {
            return await _client.MoveToPositionAsync(targetPosition, cancellationToken);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to move robot to position");
            StartReconnectionIfNeeded();
            throw;
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task<bool> MoveRelativeAsync(double deltaX, double deltaY, double deltaZ, CancellationToken cancellationToken = default)
    {
        await _commandLock.WaitAsync(cancellationToken);
        try
        {
            return await _client.MoveRelativeAsync(deltaX, deltaY, deltaZ, cancellationToken);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to move robot");
            StartReconnectionIfNeeded();
            throw;
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task<bool> EmergencyStopAsync()
    {
        try
        {
            // Emergency stop bypasses command lock for immediate execution
            var result = await _client.EmergencyStopAsync();

            if (result)
            {
                OnStatusChanged(new RobotStatus
                {
                    IsConnected = true,
                    State = RobotState.EmergencyStopped,
                    ErrorCode = ErrorCode.None
                });
            }

            return result;
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogError(ex, "Failed to execute emergency stop");
            return false;
        }
    }

    public async Task<bool> HomeAsync(CancellationToken cancellationToken = default)
    {
        await _commandLock.WaitAsync(cancellationToken);
        try
        {
            return await _client.HomeAsync(cancellationToken);
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to home robot");
            StartReconnectionIfNeeded();
            throw;
        }
        finally
        {
            _commandLock.Release();
        }
    }

    public async Task<bool> ResetErrorAsync()
    {
        try
        {
            return await _client.ResetErrorAsync();
        }
        catch (HardwareConnectionException ex)
        {
            _logger.LogWarning(ex, "Failed to reset error");
            return false;
        }
    }

    private void StartReconnectionIfNeeded()
    {
        if (_reconnectTask != null && !_reconnectTask.IsCompleted)
        {
            return; // Already reconnecting
        }

        _reconnectCts = new CancellationTokenSource();
        _reconnectTask = Task.Run(async () => await ReconnectLoopAsync(_reconnectCts.Token));
    }

    private async Task ReconnectLoopAsync(CancellationToken cancellationToken)
    {
        _reconnectAttempts = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (_settings.MaxReconnectAttempts > 0 && _reconnectAttempts >= _settings.MaxReconnectAttempts)
            {
                _logger.LogError("Max reconnection attempts reached");
                break;
            }

            _reconnectAttempts++;
            var delay = Math.Min(
                _settings.ReconnectDelayMs * (int)Math.Pow(2, _reconnectAttempts - 1),
                _settings.MaxReconnectDelayMs
            );

            _logger.LogInformation("Reconnection attempt {Attempt} in {Delay}ms", _reconnectAttempts, delay);

            await Task.Delay(delay, cancellationToken);

            if (await ConnectAsync(cancellationToken))
            {
                _logger.LogInformation("Reconnection successful");
                return;
            }
        }
    }

    protected virtual void OnPositionChanged(RobotPosition position)
    {
        PositionChanged?.Invoke(this, position);
    }

    protected virtual void OnStatusChanged(RobotStatus status)
    {
        StatusChanged?.Invoke(this, status);
    }

    public void Dispose()
    {
        _reconnectCts?.Cancel();
        _reconnectCts?.Dispose();
        _commandLock.Dispose();
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
