using LiteNetLib;
using Shared.Networking.Messages;
using Shared.Networking.RPC;

namespace Shared.Networking
{
    public interface INetworkServer : INetworkManager
    {
        void SendTo(int playerID, IStandardNetworkMessage message);
        void SendTo(NetPeer peer, IStandardNetworkMessage message);

        void SendToAll(IStandardNetworkMessage message);

        void SendToAllExcept(int playerID, IStandardNetworkMessage message);
        void SendToAllExcept(NetPeer peer, IStandardNetworkMessage message);

        TResponse SendRpcRequestTo<TResponse>(int playerID, IRpcRequestMessage<TResponse> message) where TResponse : IRpcResponseMessage;
        TResponse SendRpcRequestTo<TResponse>(NetPeer peer, IRpcRequestMessage<TResponse> message) where TResponse : IRpcResponseMessage;

    }
}