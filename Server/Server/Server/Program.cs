using Core.Server;
using System;

internal class Program
{
    private static void Main(string[] args)
    {
        int port = 5000;
        float tickInterval = 0.02f;


        Console.WriteLine("Starting game server...");
        var server = new GameServer(port, tickInterval);
        Console.WriteLine($"Server started on port {port}.");
        Console.Write($"{Environment.NewLine}Press any key to exit...");
        Console.ReadKey(true);
    }
}