using LiteNetLib;
using Server;
using Shared.Networking;
using Shared.Networking.Messages;
using System;
using System.Threading;

internal class Program
{
    const int DEFAULT_PORT = 5000;

    private static void Main(string[] args)
    {
        int port = DEFAULT_PORT;
        //first arg can be optionally given to specify port
        if (args == null || args.Length < 1)
        {
            Console.WriteLine(
                $"To run on a particular port, pass it in as a command-line argument.\n" +
                $"Using default port: {DEFAULT_PORT}.\n");
        }
        else if (int.TryParse(args[0], out port))
        {
            port = DEFAULT_PORT;
            Console.WriteLine(
                $"Invalid command line argument: {args[0]}.\n" +
                $"Using default port: {DEFAULT_PORT}.\n");
        }
        else
        {
            Console.WriteLine(
                $"Using port from command line arg: {port}.\n");
        }

        Console.WriteLine("Starting game server...\n");
        var server = new ServerGameManager();
        //server = new ServerNetworkManager(NetworkConfig.Port, NetworkConfig.TickInterval);
        NetDebug.Logger = server;

        if (!server.StartServer(port))
        {
            Console.WriteLine(
                $"\nServer failed to start on port {port}.\n" +
                $"Press any key to exit...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine(
            $"Server successfully started on port: {port}.\n" +
            $"Press any key to stop server.\n");
        try
        {
            while (!Console.KeyAvailable)
            {
                server.Tick();
                Thread.Sleep(server.networkConfig.TickIntervalMS);
            }
        }
        catch (System.Exception exception)
        {
            Console.WriteLine($"\n\n" +
                $"-----------------------------------\n" +
                $"Exception encountered in network loop! {exception.GetType()}: {exception.Message}.\n" +
                $"Stack Trace:\n" +
                $"{exception.StackTrace}\n" +
                $"-----------------------------------\n\n");
            Thread.Sleep(500);
        }
        finally
        {
            server.StopNetworking();
        }

        Console.ReadKey(true);
        Console.WriteLine($"\nServer stopped.");
        Thread.Sleep(500);

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
    }
}