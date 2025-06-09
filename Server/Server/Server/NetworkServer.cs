using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RPC;

namespace Server
{
    public class NetworkServer : NetworkManager, INetworkServer
    {
        private readonly byte _maxPlayers;

        public NetworkServer(float rpcTimeout, string networkKey, byte maxPlayers) 
            : base(rpcTimeout, networkKey)
        {
            _maxPlayers = maxPlayers;
            _listener.ConnectionRequestEvent += OnConnectionRequest;
        }

        private void OnConnectionRequest(ConnectionRequest request)
        {
            if (_netManager.ConnectedPeersCount < _maxPlayers)
                request.AcceptIfKey(_networkKey);
            else
                request.Reject();
        }

        public void SendToAll(IStandardNetworkMessage message, DeliveryMethod deliveryMethod)
            => SendToAll(message, ChannelType.Standard, deliveryMethod);

        public void SendToAllExcept(int playerID, IStandardNetworkMessage message, DeliveryMethod deliveryMethod)
            => SendToAllExcept(playerID, message, ChannelType.Standard, deliveryMethod);

        public void SendToAllExcept(NetPeer peer, IStandardNetworkMessage message, DeliveryMethod deliveryMethod)
            => SendToAllExcept(peer, message, ChannelType.Standard, deliveryMethod);

        public void SendTo(int playerID, IStandardNetworkMessage message, DeliveryMethod deliveryMethod)
            => SendTo(playerID, message, ChannelType.Standard, deliveryMethod);

        public void SendTo(NetPeer peer, IStandardNetworkMessage message, DeliveryMethod deliveryMethod)
            => SendTo(peer, message, ChannelType.Standard, deliveryMethod);

        public TResponse SendRpcRequestTo<TResponse>(int playerID, IRpcRequestMessage<TResponse> message)
            where TResponse : IRpcResponseMessage
            => SendRpcRequest(playerID, message);

        public TResponse SendRpcRequestTo<TResponse>(NetPeer peer, IRpcRequestMessage<TResponse> message) 
            where TResponse : IRpcResponseMessage
            => SendRpcRequest(peer, message);
    }
}