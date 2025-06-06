using LiteNetLib;
using Shared.Configuration;
using Shared.Networking;
using Shared.Networking.Messages;
using System;

namespace Server
{
    public class ServerNetworkManager : NetworkManager
    {
        public ServerNetworkManager(NetworkConfig networkConfig, int port) : base(networkConfig, port){ }

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

            if (_netManager.ConnectedPeersCount < networkConfig.MaxPlayers)
                request.AcceptIfKey(networkConfig.TestNetworkKey);
            else
                request.Reject();
        }

        #region Server Send Functions

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
        #endregion
    }
}