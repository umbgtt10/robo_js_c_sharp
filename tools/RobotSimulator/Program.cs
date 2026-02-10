using RobotSimulator;

Console.WriteLine("===================================");
Console.WriteLine("  Robotic Control System Simulator");
Console.WriteLine("===================================");
Console.WriteLine();

var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : 5000;

var server = new SimulatedRobotServer(port);

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nShutting down simulator...");
    server.Stop();
};

await server.StartAsync();
