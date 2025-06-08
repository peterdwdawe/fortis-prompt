using Shared.Networking.Messages;
using Shared.Networking.RPC;

namespace Shared.Networking
{
    public interface INetworkClient : INetworkManager
    {
        void ConnectToServer(string serverAddress, int serverPort);

        void Send(IStandardNetworkMessage message);

        TResponse SendRpcRequest<TResponse>(IRpcRequestMessage<TResponse> message) where TResponse : IRpcResponseMessage;
    }
}