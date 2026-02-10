using RoboticControl.Domain.Interfaces;

namespace RoboticControl.API.BackgroundServices;

/// <summary>
/// Background service that establishes initial connection to robot hardware on startup
/// </summary>
public class HardwareConnectionService : BackgroundService
{
    private readonly ILogger<HardwareConnectionService> _logger;
    private readonly IRobotHardwareService _hardwareService;

    public HardwareConnectionService(
        ILogger<HardwareConnectionService> logger,
        IRobotHardwareService hardwareService)
    {
        _logger = logger;
        _hardwareService = hardwareService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Hardware connection service starting");

        // Give the simulator a moment to be ready if starting simultaneously
        await Task.Delay(500, stoppingToken);

        // Attempt initial connection
        var maxAttempts = 5;
        var attempt = 0;

        while (attempt < maxAttempts && !stoppingToken.IsCancellationRequested)
        {
            attempt++;
            _logger.LogInformation("Attempting to connect to robot hardware (attempt {Attempt}/{MaxAttempts})",
                attempt, maxAttempts);

            try
            {
                if (await _hardwareService.ConnectAsync(stoppingToken))
                {
                    _logger.LogInformation("Successfully connected to robot hardware");
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connection attempt {Attempt} failed", attempt);
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        if (!_hardwareService.IsConnected)
        {
            _logger.LogError("Failed to establish initial connection to robot hardware after {Attempts} attempts",
                maxAttempts);
        }

        // Keep service alive
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Normal shutdown
        }

        _logger.LogInformation("Hardware connection service stopped");
    }
}
