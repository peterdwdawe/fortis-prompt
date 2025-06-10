using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RpcMessages;

namespace Client.Adapters.Networking
{
    public interface INetworkClient : INetworkManager
    {
        void ConnectToServer(string serverAddress, int serverPort);

        void Send(IStandardNetworkMessage message, 
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered);

        TResponse SendRpcRequest<TResponse>(IRpcRequestMessage<TResponse> message) 
            where TResponse : IRpcResponseMessage;
    }
}