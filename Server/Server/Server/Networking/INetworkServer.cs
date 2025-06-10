using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RpcMessages;

namespace Server.Networking
{
    public interface INetworkServer : INetworkManager
    {
        void SendTo(int playerID, IStandardNetworkMessage message, 
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered);

        void SendTo(NetPeer peer, IStandardNetworkMessage message, 
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered);

        void SendToAll(IStandardNetworkMessage message, 
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered);

        void SendToAllExcept(int playerID, IStandardNetworkMessage message, 
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered);

        void SendToAllExcept(NetPeer peer, IStandardNetworkMessage message, 
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered);

        TResponse SendRpcRequestTo<TResponse>(int playerID, IRpcRequestMessage<TResponse> message) 
            where TResponse : IRpcResponseMessage;

        TResponse SendRpcRequestTo<TResponse>(NetPeer peer, IRpcRequestMessage<TResponse> message) 
            where TResponse : IRpcResponseMessage;

    }
}