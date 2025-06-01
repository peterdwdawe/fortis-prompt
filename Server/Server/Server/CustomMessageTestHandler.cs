using Core.Server;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    internal static class CustomMessageTestHandler
    {
        public static void OnReceivedCustomMessage(CustomMessage message, GameServer server, NetPeer peer)
        {
            Console.WriteLine($"Server Received: {message.msg}");
            server.SendTo(peer, message);
            Console.WriteLine($"Server Sent: {message.msg}");
        }
    }
}
