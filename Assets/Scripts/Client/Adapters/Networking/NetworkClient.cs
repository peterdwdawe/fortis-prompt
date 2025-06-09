using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RPC;

namespace Client.Adapters.Networking
{
    public class NetworkClient : NetworkManager, INetworkClient
    {
        public NetworkClient(float rpcTimeout, string networkKey) : base(rpcTimeout, networkKey) { }

        public void ConnectToServer(string serverAddress, int serverPort)
        {
            Log($"Connecting to {serverAddress} (Port {serverPort})");
            _netManager.Connect(serverAddress, serverPort, _networkKey);
        }

        public void Send(IStandardNetworkMessage message, DeliveryMethod deliveryMethod)
            => SendToFirstPeer(message, ChannelType.Standard, deliveryMethod);

        public TResponse SendRpcRequest<TResponse>(IRpcRequestMessage<TResponse> message)
            where TResponse : IRpcResponseMessage
        {
            //Log($"Send {message.MsgType} RPC Request!");
            SendToFirstPeer(message, ChannelType.RpcRequest, DeliveryMethod.ReliableUnordered);
            return WaitForRpcResponse<TResponse>();
        }
    }
}