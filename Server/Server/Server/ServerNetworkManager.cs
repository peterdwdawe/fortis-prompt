using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Configuration;
using Shared.Networking;

namespace Server
{
    public class ServerNetworkManager : NetworkManager
    {
        public ServerNetworkManager(NetworkState networkState, int port, GameConfig gameConfig) : base(networkState, port) 
        {
            this.gameConfig = gameConfig;
        }

        private readonly GameConfig gameConfig;

        protected override bool StartInternal()
        {

            if (!_netManager.Start(_port))
                return false;

            _netManager.BroadcastReceiveEnabled = true;

            return true;
        }

        protected override void Log(string str)
        {
            Console.WriteLine("[SERVER] " + str);
        }

        public override void OnConnectionRequest(ConnectionRequest request)
        {
            base.OnConnectionRequest(request);

            if (_netManager.ConnectedPeersCount < gameConfig.MaxPlayers)
                request.AcceptIfKey(NetworkingUtils.testNetworkKey);
            else
                request.Reject();
        }

        public override void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            base.OnNetworkReceiveUnconnected(remoteEndPoint, reader, messageType);

            //TODO();// remove? also turn off discovery in start?
            if (messageType == UnconnectedMessageType.Broadcast)
            {
                Log("Received discovery request. Send discovery response");
                NetDataWriter resp = new NetDataWriter();
                resp.Put(1);
                _netManager.SendUnconnectedMessage(resp, remoteEndPoint);
            }
        }

        public void SendToAll(INetworkMessage message)
        {
            if (!IsConnected())
            {
                //Log($"{message.GetType()} SendToAll failed: not connected!");
                return;
            }

            _dataWriter.Reset();
            _dataWriter.Put(message);
            _netManager.SendToAll(_dataWriter, DeliveryMethod.ReliableOrdered);
        }

        public void SendToAllExcept(int playerID, INetworkMessage message)
        {
            if (!IsConnected())
            {
                //Log($"{message.GetType()} SendToAllExecpt {playerID} failed: not connected!");
                return;
            }

            _dataWriter.Reset();
            _dataWriter.Put(message);
            foreach (var _peer in _netManager.ConnectedPeerList)
            {
                if (_peer.Id == playerID) continue;
                _peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
            }
        }

        public void SendToAllExcept(NetPeer peer, INetworkMessage message)
            => SendToAllExcept(peer.Id, message);

        public void SendTo(int playerID, INetworkMessage message)
        {
            if (!IsConnected())
            {
                //Log($"{message.GetType()} SendTo {playerID} failed: not connected!");
                return;
            }

            var peer = _netManager.GetPeerById(playerID);

            if (peer == null)
            {
                Log($"{message.GetType()} SendTo {playerID} failed: can't find peer!");
                return;
            }
            SendTo(peer, message);
        }

        public void SendTo(NetPeer peer, INetworkMessage message)
        {
            if (!IsConnected())
            {
                //Log($"{message.GetType()} SendTo {peer.Id} failed: not connected!");
                return;
            }

            _dataWriter.Reset();
            _dataWriter.Put(message);
            peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
        }
    }
}