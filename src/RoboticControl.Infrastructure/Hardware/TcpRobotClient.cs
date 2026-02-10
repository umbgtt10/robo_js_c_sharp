using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoboticControl.Domain.Entities;
using RoboticControl.Domain.Exceptions;
using RoboticControl.Infrastructure.Configuration;

namespace RoboticControl.Infrastructure.Hardware;

/// <summary>
/// TCP/IP client for communicating with robot hardware
/// Protocol: Text-based commands and responses
/// Commands: MOVE_ABS x,y,z,rx,ry,rz | MOVE_REL dx,dy,dz | GET_POS | GET_STATUS | STOP | HOME | RESET
/// Responses: OK data | ERROR message
/// </summary>
public class TcpRobotClient : IDisposable
{
    private readonly ILogger<TcpRobotClient> _logger;
    private readonly HardwareSettings _settings;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _isDisposed;

    public bool IsConnected => _client?.Connected ?? false;

    public TcpRobotClient(ILogger<TcpRobotClient> logger, IOptions<HardwareSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                return true;
            }

            _logger.LogInformation("Connecting to robot at {Host}:{Port}", _settings.Host, _settings.Port);

            _client = new TcpClient();
            using var cts = new CancellationTokenSource(_settings.ConnectionTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            await _client.ConnectAsync(_settings.Host, _settings.Port, linkedCts.Token);
            _stream = _client.GetStream();

            _logger.LogInformation("Successfully connected to robot");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to robot");
            Disconnect();
            return false;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Disconnect()
    {
        _stream?.Close();
        _client?.Close();
        _stream = null;
        _client = null;
        _logger.LogInformation("Disconnected from robot");
    }

    public async Task<string> SendCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        if (!IsConnected || _stream == null)
        {
            throw new HardwareConnectionException("Not connected to robot hardware");
        }

        try
        {
            using var cts = new CancellationTokenSource(_settings.CommandTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

            // Send command
            var commandBytes = Encoding.ASCII.GetBytes(command + "\n");
            await _stream.WriteAsync(commandBytes, linkedCts.Token);
            await _stream.FlushAsync(linkedCts.Token);

            _logger.LogDebug("Sent command: {Command}", command);

            // Read response
            var buffer = new byte[4096];
            var bytesRead = await _stream.ReadAsync(buffer, linkedCts.Token);
            var response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

            _logger.LogDebug("Received response: {Response}", response);

            return response;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Command timeout: {Command}", command);
            throw new HardwareConnectionException("Command timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending command: {Command}", command);
            throw new HardwareConnectionException($"Communication error: {ex.Message}", ex);
        }
    }

    public async Task<RobotPosition> GetPositionAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("GET_POS", cancellationToken);
        return ParsePositionResponse(response);
    }

    public async Task<RobotStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("GET_STATUS", cancellationToken);
        return ParseStatusResponse(response);
    }

    public async Task<bool> MoveToPositionAsync(RobotPosition position, CancellationToken cancellationToken = default)
    {
        var command = $"MOVE_ABS {position.X:F2},{position.Y:F2},{position.Z:F2},{position.RotationX:F2},{position.RotationY:F2},{position.RotationZ:F2}";
        var response = await SendCommandAsync(command, cancellationToken);
        return response.StartsWith("OK");
    }

    public async Task<bool> MoveRelativeAsync(double dx, double dy, double dz, CancellationToken cancellationToken = default)
    {
        var command = $"MOVE_REL {dx:F2},{dy:F2},{dz:F2}";
        var response = await SendCommandAsync(command, cancellationToken);
        return response.StartsWith("OK");
    }

    public async Task<bool> EmergencyStopAsync()
    {
        var response = await SendCommandAsync("STOP", CancellationToken.None);
        return response.StartsWith("OK");
    }

    public async Task<bool> HomeAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("HOME", cancellationToken);
        return response.StartsWith("OK");
    }

    public async Task<bool> ResetErrorAsync()
    {
        var response = await SendCommandAsync("RESET", CancellationToken.None);
        return response.StartsWith("OK");
    }

    private RobotPosition ParsePositionResponse(string response)
    {
        // Expected format: "OK x,y,z,rx,ry,rz"
        if (!response.StartsWith("OK "))
        {
            throw new HardwareConnectionException($"Invalid position response: {response}");
        }

        var data = response[3..].Split(',');
        if (data.Length < 6)
        {
            throw new HardwareConnectionException($"Incomplete position data: {response}");
        }

        return new RobotPosition(
            double.Parse(data[0]),
            double.Parse(data[1]),
            double.Parse(data[2]),
            double.Parse(data[3]),
            double.Parse(data[4]),
            double.Parse(data[5])
        );
    }

    private RobotStatus ParseStatusResponse(string response)
    {
        // Expected format: "OK state,temp,error,load"
        if (!response.StartsWith("OK "))
        {
            throw new HardwareConnectionException($"Invalid status response: {response}");
        }

        var data = response[3..].Split(',');
        if (data.Length < 4)
        {
            throw new HardwareConnectionException($"Incomplete status data: {response}");
        }

        return new RobotStatus
        {
            IsConnected = true,
            State = Enum.Parse<Domain.Enums.RobotState>(data[0]),
            Temperature = double.Parse(data[1]),
            ErrorCode = (Domain.Enums.ErrorCode)int.Parse(data[2]),
            LoadPercentage = double.Parse(data[3]),
            Timestamp = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        Disconnect();
        _connectionLock.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
