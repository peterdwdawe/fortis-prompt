using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Configuration;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RPC;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Client.Adapters.Networking
{
    public class ClientNetworkManager : NetworkManager, INetworkClient
    {
        public ClientNetworkManager(NetworkConfig networkConfig) : base(networkConfig)
        {
        }


        public void ConnectToServer(string serverAddress, int serverPort)
        {
            Log($"Connecting to {serverAddress} (Port {serverPort})");
            _netManager.Connect(serverAddress, serverPort, _networkConfig.TestNetworkKey);
        }

        public void Send(IStandardNetworkMessage message)
            => Send(message, ChannelType.Standard);

        public TResponse SendRpcRequest<TResponse>(IRpcRequestMessage<TResponse> message)
            where TResponse : IRpcResponseMessage
        {
            //Log($"Send {message.MsgType} RPC Request!");
            Send(message, ChannelType.RpcRequest);
            return WaitForRpcResponse<TResponse>();
        }
    }
}