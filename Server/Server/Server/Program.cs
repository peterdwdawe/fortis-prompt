using LiteNetLib;
using Server;
using Shared.Networking;
using System;
using System.Diagnostics;
using System.Threading;

internal class Program
{
    const int DEFAULT_PORT = 5000;

    private static void Main(string[] args)
    {
        Console.CursorVisible = false;

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
            $"Press any key to stop server.\n\n");
        try
        {
            int tickIntervalMS = server.networkConfig.TickIntervalMS;

            Stopwatch tickStopwatch = new Stopwatch();
            double tickMultiplierMS = 1000.0 / Stopwatch.Frequency;

            float bandwidthUpdateInterval = 0.5f;
            int bandwidthUpdateTickCount =(int)( bandwidthUpdateInterval * 1000f / (tickIntervalMS));
            int ticksSinceLastBandwidthUpdate = bandwidthUpdateTickCount;

            var statsCursorPosition = Console.CursorTop;
            //var logCursorPosition = tickCursorPosition + 4;

            tickStopwatch.Start();
            string networkStats = string.Empty;
            while (!Console.KeyAvailable)
            {
                //Console.SetCursorPosition(0, logCursorPosition);

                server.Tick();
                while (tickStopwatch.ElapsedMilliseconds < tickIntervalMS)
                {
                    //just loop here until time is up - it's a little crazy but it works!
                }
                //Thread.Sleep(server.networkConfig.TickIntervalMS);
                statsCursorPosition = Console.CursorTop;
                ticksSinceLastBandwidthUpdate++;
                if (ticksSinceLastBandwidthUpdate >= bandwidthUpdateTickCount)
                {
                    networkStats = server.GetNetworkDiffStatistics().ToBandwidthString();
                    ticksSinceLastBandwidthUpdate = 0;
                }

                Console.WriteLine($"Tick: {(tickStopwatch.ElapsedTicks * tickMultiplierMS):00.00}ms\n" +
                    $"{networkStats}");

                Console.SetCursorPosition(0, statsCursorPosition);
                tickStopwatch.Restart();
            }
            Console.SetCursorPosition(0, statsCursorPosition + 4);
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