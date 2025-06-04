using LiteNetLib;
using Server;
using Shared.Networking;
using Shared.Networking.Messages;
using System;
using System.Threading;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting game server...");
        var server = new ServerGameManager();
        //server = new ServerNetworkManager(NetworkConfig.Port, NetworkConfig.TickInterval);
        NetDebug.Logger = server;

        if (!server.StartNetworking())
        {
            Console.WriteLine($"\nServer failed to start on port {NetworkConfig.Port}. Press any key to exit...");
            Console.ReadKey(true);
            return;
        }

        Console.WriteLine($"\nServer started on port {NetworkConfig.Port}. Press any key to stop server.{Environment.NewLine}");
        try
        {
            while (!Console.KeyAvailable)
            {
                server.Tick();
                Thread.Sleep(NetworkConfig.TickIntervalMS);
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