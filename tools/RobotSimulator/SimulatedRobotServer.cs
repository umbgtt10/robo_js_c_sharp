using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RobotSimulator;

/// <summary>
/// Simulated robot hardware server for development and testing
/// Responds to TCP/IP commands and simulates robot behavior
/// </summary>
public class SimulatedRobotServer
{
    private readonly int _port;
    private readonly Random _random = new();
    private TcpListener? _listener;
    private bool _isRunning;

    // Simulated robot state
    private double _x = 0;
    private double _y = 0;
    private double _z = 100;
    private double _rotX = 0;
    private double _rotY = 0;
    private double _rotZ = 0;
    private string _state = "Idle";
    private double _temperature = 25.0;
    private int _errorCode = 0;
    private double _loadPercentage = 0;
    private bool _isEmergencyStopped = false;

    public SimulatedRobotServer(int port = 5000)
    {
        _port = port;
    }

    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"Robot Simulator started on port {_port}");
        Console.WriteLine("Protocol Commands:");
        Console.WriteLine("  MOVE_ABS x,y,z,rx,ry,rz - Move to absolute position");
        Console.WriteLine("  MOVE_REL dx,dy,dz - Move relative to current position");
        Console.WriteLine("  GET_POS - Get current position");
        Console.WriteLine("  GET_STATUS - Get system status");
        Console.WriteLine("  STOP - Emergency stop");
        Console.WriteLine("  HOME - Execute homing sequence");
        Console.WriteLine("  RESET - Reset error state");
        Console.WriteLine();

        while (_isRunning)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch (Exception ex) when (!_isRunning)
            {
                Console.WriteLine($"Server stopped: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"Client connected from {client.Client.RemoteEndPoint}");

        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[4096];

            while (client.Connected && _isRunning)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0) break;

                var command = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"Received: {command}");

                var response = ProcessCommand(command);
                var responseBytes = Encoding.ASCII.GetBytes(response + "\n");

                await stream.WriteAsync(responseBytes);
                await stream.FlushAsync();

                Console.WriteLine($"Sent: {response}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine("Client disconnected");
        }
    }

    private string ProcessCommand(string command)
    {
        // Simulate random errors (5% failure rate)
        if (_random.Next(100) < 5 && !command.StartsWith("STOP"))
        {
            return "ERROR Communication timeout";
        }

        var parts = command.Split(' ', 2);
        var cmd = parts[0].ToUpper();
        var args = parts.Length > 1 ? parts[1] : "";

        try
        {
            return cmd switch
            {
                "MOVE_ABS" => HandleMoveAbsolute(args),
                "MOVE_REL" => HandleMoveRelative(args),
                "GET_POS" => HandleGetPosition(),
                "GET_STATUS" => HandleGetStatus(),
                "STOP" => HandleEmergencyStop(),
                "HOME" => HandleHome(),
                "RESET" => HandleReset(),
                _ => $"ERROR Unknown command: {cmd}"
            };
        }
        catch (Exception ex)
        {
            return $"ERROR {ex.Message}";
        }
    }

    private string HandleMoveAbsolute(string args)
    {
        if (_isEmergencyStopped)
        {
            return "ERROR Robot in emergency stop state";
        }

        var coords = args.Split(',');
        if (coords.Length < 6)
        {
            return "ERROR Invalid parameters (expected: x,y,z,rx,ry,rz)";
        }

        _x = double.Parse(coords[0]);
        _y = double.Parse(coords[1]);
        _z = double.Parse(coords[2]);
        _rotX = double.Parse(coords[3]);
        _rotY = double.Parse(coords[4]);
        _rotZ = double.Parse(coords[5]);

        _state = "Moving";
        _loadPercentage = 75;
        _temperature += 0.5;

        // Simulate movement time
        Thread.Sleep(500);

        _state = "Idle";
        _loadPercentage = 10;

        return "OK Movement completed";
    }

    private string HandleMoveRelative(string args)
    {
        if (_isEmergencyStopped)
        {
            return "ERROR Robot in emergency stop state";
        }

        var deltas = args.Split(',');
        if (deltas.Length < 3)
        {
            return "ERROR Invalid parameters (expected: dx,dy,dz)";
        }

        _x += double.Parse(deltas[0]);
        _y += double.Parse(deltas[1]);
        _z += double.Parse(deltas[2]);

        _state = "Moving";
        _loadPercentage = 50;

        // Simulate movement time
        Thread.Sleep(200);

        _state = "Idle";
        _loadPercentage = 10;

        return "OK Movement completed";
    }

    private string HandleGetPosition()
    {
        // Add small random noise to simulate sensor readings
        var noise = () => (_random.NextDouble() - 0.5) * 0.1;

        return $"OK {_x + noise():F2},{_y + noise():F2},{_z + noise():F2}," +
               $"{_rotX + noise():F2},{_rotY + noise():F2},{_rotZ + noise():F2}";
    }

    private string HandleGetStatus()
    {
        // Simulate temperature fluctuation
        _temperature = 25.0 + (_random.NextDouble() * 5.0);

        return $"OK {_state},{_temperature:F1},{_errorCode},{_loadPercentage:F0}";
    }

    private string HandleEmergencyStop()
    {
        _state = "EmergencyStopped";
        _isEmergencyStopped = true;
        _loadPercentage = 0;
        _errorCode = 0;

        Console.WriteLine("!!! EMERGENCY STOP ACTIVATED !!!");

        return "OK Emergency stop executed";
    }

    private string HandleHome()
    {
        if (_isEmergencyStopped)
        {
            return "ERROR Robot in emergency stop state";
        }

        _state = "Homing";
        _loadPercentage = 60;

        // Simulate homing sequence
        Thread.Sleep(1000);

        _x = 0;
        _y = 0;
        _z = 100;
        _rotX = 0;
        _rotY = 0;
        _rotZ = 0;

        _state = "Idle";
        _loadPercentage = 10;

        return "OK Homing completed";
    }

    private string HandleReset()
    {
        _errorCode = 0;
        _isEmergencyStopped = false;
        _state = "Idle";

        Console.WriteLine("Error state reset");

        return "OK Error reset";
    }
}
