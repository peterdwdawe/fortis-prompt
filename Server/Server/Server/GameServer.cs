using System;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Core.Server
{
    public class GameServer : INetEventListener, INetLogger
    {
        private NetManager _netServer;
        private NetPeer _ourPeer;
        private NetDataWriter _dataWriter;

        private readonly int _port;
        private readonly int _updateTime;

        public GameServer(int port, float tickInterval)
        {
            _port = port;
            _updateTime = Math.Max(1, (int)tickInterval * 1000);
        }

        public void Start()
        {
            NetDebug.Logger = this;
            _dataWriter = new NetDataWriter();
            _netServer = new NetManager(this);
            _netServer.Start(_port);
            _netServer.BroadcastReceiveEnabled = true;
            _netServer.UpdateTime = _updateTime;
        }

        public void Stop()
        {
            NetDebug.Logger = null;
            if (_netServer != null)
                _netServer.Stop();
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
            Console.WriteLine($"[SERVER] We have new peer: {peer}");
            _ourPeer = peer;
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
            request.AcceptIfKey("sample_app");
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"[SERVER] peer disconnected {peer}, info: {disconnectInfo.Reason}");
            if (peer == _ourPeer)
                _ourPeer = null;
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            //TODO: identify message, update state, and update clients
        }

        void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Console.WriteLine(string.Format(str, args));
        }
    }
}