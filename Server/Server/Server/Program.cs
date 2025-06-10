using LiteNetLib;
using System;
using System.Diagnostics;
using System.Threading;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Console.WriteLine("Starting game server...\n");

            NetDebug.Logger = new SimpleNetLogger();
            var server = new GameServer();
            int port = server.Port;

            if (!server.StartServer())
            {
                Console.WriteLine(
                    $"\nServer failed to start on port {port}.\n" +
                    $"Press any key to exit...");
                Console.ReadKey(true);
                return;
            }

            Console.WriteLine(
                $"Server successfully started on port: {port}.\n" +
                $"Press any key to stop the server.\n\n");

            //Network Loop
            try
            {
                float tickInterval = server.TickInterval;
                int tickIntervalMS = (int)(tickInterval * 1000);

                Stopwatch tickStopwatch = new Stopwatch();
                double tickMultiplierMS = 1000.0 / Stopwatch.Frequency;

                float bandwidthUpdateInterval = 0.5f;
                int bandwidthUpdateTickCount = (int)(bandwidthUpdateInterval * 1000f / (tickIntervalMS));
                int ticksSinceLastBandwidthUpdate = bandwidthUpdateTickCount;

                var statsCursorPosition = Console.CursorTop;

                tickStopwatch.Start();
                string networkStats = string.Empty;
                while (!Console.KeyAvailable)
                {
                    server.Update(tickInterval);

                    //Thread.Sleep isn't very accurate... we can do better!
                    //Thread.Sleep(server.networkConfig.TickIntervalMS);

                    //just loop here until time is up - it's a little crazy but it works!
                    while (tickStopwatch.ElapsedMilliseconds < tickIntervalMS) { }

                    statsCursorPosition = Console.CursorTop;
                    ticksSinceLastBandwidthUpdate++;
                    if (ticksSinceLastBandwidthUpdate >= bandwidthUpdateTickCount)
                    {
                        networkStats = server.GetNetworkDiffStatistics().ToBandwidthString();
                        ticksSinceLastBandwidthUpdate = 0;
                    }
                    var tickTimeFormatted = SixCharacterFormat(tickStopwatch.ElapsedTicks * tickMultiplierMS);
                    Console.WriteLine($"Tick: {tickTimeFormatted}ms\n" +
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
                    $"-----------------------------------\n");
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

        class SimpleNetLogger : INetLogger
        {
            void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
            {
                Console.WriteLine(string.Format(str, args));
            }
        }

        static string SixCharacterFormat(double number)
        {
            if(number < 10)
                return $"  {number:0.00}";
            if (number < 100)
                return $" {number:00.00}";
            if (number < 1000)
                return $"{number:000.00}";
            if (number < 10000)
                return $"{number:0000.0}";
            if (number < 100000)
                return $" {number:00000}";
            if (number < 1000000)
                return $"{number:000000}";
            return ">1.0e6";
        }
    }
}