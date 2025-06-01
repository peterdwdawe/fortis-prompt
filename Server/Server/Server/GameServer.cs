using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;
using Server;
using Shared.Networking;

namespace Core.Server
{
    public class GameServer : INetEventListener, INetLogger
    {
        private NetManager _netServer;
        private NetDataWriter _dataWriter;

        private readonly int _port;
        private readonly int _updateTime;

        public bool started { get; private set; } = false;

        public GameServer(int port, float tickInterval)
        {
            _port = port;
            _updateTime = Math.Max(1, (int)tickInterval * 1000);
        }

        public bool Start()
        {
            if (started)
                return false;

            NetDebug.Logger = this;
            _dataWriter = new NetDataWriter();
            _netServer = new NetManager(this);



            if (!_netServer.Start(_port))
            {
                return false;
            }
            started = true;
            _netServer.BroadcastReceiveEnabled = true;
            _netServer.UpdateTime = _updateTime;

            return true;
        }

        public void Stop()
        {
            if (!started)
                return;

            NetDebug.Logger = null;
            if (_netServer != null)
                _netServer.Stop();
        }

        public void SendTo(NetPeer peer, INetworkMessage message)
        {
            if (!started)
                return;

            _dataWriter.Reset();
            _dataWriter.Put(message);
            peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
        }

        public void SendToAll(INetworkMessage message)
        {
            if (!started)
                return;

            _dataWriter.Reset();
            _dataWriter.Put(message);
            foreach (var peer in _netServer.ConnectedPeerList)
            {
                peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
            }
        }

        public void SendToAllExcept(NetPeer peer, INetworkMessage message)
        {
            if (!started)
                return;

            _dataWriter.Reset();
            _dataWriter.Put(message);
            foreach(var _peer in _netServer.ConnectedPeerList)
            {
                if(_peer == peer) continue;
                _peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
            }
        }

        //private void Update()
        //{
        //    _netServer.PollEvents();
        //}

        //private void FixedUpdate()
        //{
        //    if (_ourPeer != null)
        //    {
        //        _serverBall.transform.Translate(1f * Time.fixedDeltaTime, 0f, 0f);
        //        _dataWriter.Reset();
        //        _dataWriter.Put(_serverBall.transform.position.x);
        //        _ourPeer.Send(_dataWriter, DeliveryMethod.Sequenced);
        //    }
        //}

        //private void OnDestroy()
        //{
        //    NetDebug.Logger = null;
        //    if (_netServer != null)
        //        _netServer.Stop();
        //}

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine($"[SERVER] We have new peer: {peer.Id} ({peer})");
            PeerLookup.OnPeerConnected(peer);
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            Console.WriteLine($"[SERVER] error: {socketErrorCode}");
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.Broadcast)
            {
                Console.WriteLine("[SERVER] Received discovery request. Send discovery response");
                NetDataWriter resp = new NetDataWriter();
                resp.Put(1);
                _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
            }
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            if (_netServer.ConnectedPeersCount < NetworkConfig.MaxConnectionCount)
                request.AcceptIfKey(NetworkingUtils.testNetworkKey);
            else
                request.Reject();
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"[SERVER] peer disconnected {peer.Id} ({peer}), info: {disconnectInfo.Reason}");

            PeerLookup.OnPeerDisconnected(peer);
        }

        public event Action<NetPeer, INetworkMessage> MessageReceived;

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (reader.TryReadNetworkMessage(out var msg))
            {
                MessageReceived?.Invoke(peer, msg);
            }
            reader.Recycle();
            //_newBallPosX = reader.GetFloat();

            //var pos = _clientBall.transform.position;

            //_oldBallPosX = pos.x;
            //pos.x = _newBallPosX;

            //_clientBall.transform.position = pos;

            //_lerpTime = 0f;
        }

        void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Console.WriteLine(string.Format(str, args));
        }

        internal void PollEvents()
        {
            if (!started)
                return;

            _netServer.PollEvents();
        }
    }
}