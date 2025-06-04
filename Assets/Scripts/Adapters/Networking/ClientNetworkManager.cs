using System.Net;
using UnityEngine;
using LiteNetLib;
using Shared.Networking;

namespace Adapters.Networking
{
    public class ClientNetworkManager : NetworkManager
    {
        public ClientNetworkManager(int port, float tickInterval) : base(port, tickInterval) { }

        protected override bool StartInternal()
        {
            _netManager.UnconnectedMessagesEnabled = true;

            if (!_netManager.Start())
                return false;

            Log($"Attempting connection to {NetworkingUtils.testNetworkAddress} (Port {_port})");
            _netManager.Connect(NetworkingUtils.testNetworkAddress, _port, NetworkingUtils.testNetworkKey);
            return true;
        }

        protected override void Log(string str)
        {
            Debug.Log("[CLIENT] " + str);
        }

        public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            base.OnNetworkReceiveUnconnected (remoteEndPoint, reader, messageType);

            if (messageType == UnconnectedMessageType.BasicMessage && _netManager.ConnectedPeersCount == 0 && reader.GetInt() == 1)
            {
                Log("Received discovery response. Connecting to: " + remoteEndPoint);
                _netManager.Connect(remoteEndPoint, "sample_app");
            }
        }

        public bool IsConnected(out NetPeer server)
        {
            if (!started)
            {
                server = default;
                return false;
            }

            server = _netManager.FirstPeer;
            return server != null && server.ConnectionState == LiteNetLib.ConnectionState.Connected;
        }

        public void SendToServer(INetworkMessage message)
        {
            if (!IsConnected(out var server))
                return;

            _dataWriter.Reset();
            _dataWriter.Put(message);
            server.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
    }
}