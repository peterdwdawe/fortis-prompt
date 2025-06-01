using Core.Server;
using LiteNetLib;
using Server;
using Shared.Networking;
using Shared.Networking.Messages;
using System;
using System.Threading;

internal class Program
{
    static GameServer server;

    private static void Main(string[] args)
    {
        Console.WriteLine("Starting game server...");
        server = new GameServer(NetworkConfig.Port, NetworkConfig.TickInterval);
        server.MessageReceived += OnMessageReceived;
        if (!server.Start())
        {
            Console.WriteLine($"Server failed to start on port {NetworkConfig.Port}. Press any key to exit...");
            Console.ReadKey(true);
            return;
        }
        Console.WriteLine($"Server started on port {NetworkConfig.Port}. Press any key to stop server.{Environment.NewLine}");
        while (!Console.KeyAvailable)
        {
            server.PollEvents();
            Thread.Sleep(NetworkConfig.TickIntervalMS);
        }
        server.Stop();
        server.MessageReceived -= OnMessageReceived;
        Console.ReadKey(true);
        Console.WriteLine($"{Environment.NewLine} Server stopped.");
        Thread.Sleep(500);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(true);
    }


    private static void OnMessageReceived(NetPeer peer, INetworkMessage message)
    {
        switch (message.MsgType)
        {
            case MessageType.CustomMessage:
                CustomMessageTestHandler.OnReceivedCustomMessage((CustomMessage)message, server, peer);
                break;
            //case MessageType.PlayerUpdate:
            //    break;
            //case MessageType.ProjectileSpawn:
            //    break;
            //case MessageType.ProjectileDespawn:
            //    break;
            //case MessageType.HealthUpdate:
            //    break;
            //case MessageType.Death:
            //    break;
            //case MessageType.Respawn:
            //    break;
            default:
                Console.WriteLine($"Unhandled network message type: {message.MsgType}");
                return;
        }
    }
}