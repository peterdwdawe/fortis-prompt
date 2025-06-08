using LiteNetLib;
using Shared.Configuration;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RPC;
using System;

namespace Server
{
    public class ServerNetworkManager : NetworkManager, INetworkServer
    {
        public ServerNetworkManager(NetworkConfig networkConfig) : base(networkConfig)
        {
        }
        public void SendToAll(IStandardNetworkMessage message)
            => SendToAll(message, ChannelType.Standard);

        public void SendToAllExcept(int playerID, IStandardNetworkMessage message)
            => SendToAllExcept(playerID, message, ChannelType.Standard);

        public void SendToAllExcept(NetPeer peer, IStandardNetworkMessage message)
            => SendToAllExcept(peer, message, ChannelType.Standard);

        public void SendTo(int playerID, IStandardNetworkMessage message)
            => SendTo(playerID, message, ChannelType.Standard);

        public void SendTo(NetPeer peer, IStandardNetworkMessage message)
            => SendTo(peer, message, ChannelType.Standard);

        public TResponse SendRpcRequestTo<TResponse>(int playerID, IRpcRequestMessage<TResponse> message)
            where TResponse : IRpcResponseMessage
            => SendRpcRequest(playerID, message);

        public TResponse SendRpcRequestTo<TResponse>(NetPeer peer, IRpcRequestMessage<TResponse> message) 
            where TResponse : IRpcResponseMessage
            => SendRpcRequest(peer, message);
    }
}