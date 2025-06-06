using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Configuration;
using Shared.Networking.Messages;
using System;
using System.Net;
using System.Net.Sockets;

namespace Shared.Networking
{
    public abstract class NetworkManager : INetEventListener 
    {
        protected readonly int _port;
        protected NetDataWriter _dataWriter;
        protected NetManager _netManager;
        protected NetworkConfig networkConfig;

        float CurrentTime => CurrentTick * networkConfig.TickInterval;
        uint CurrentTick = 0;

        void SetCurrentTick(uint currentTick)
        {
            CurrentTick = currentTick;
        }

        public bool started { get; private set; } = false;

        protected NetworkManager(NetworkConfig networkConfig, int port)
        {
            this.networkConfig = networkConfig;
            _port = port;
        }

        public bool IsConnected()
            => started && _netManager.ConnectedPeersCount > 0;

        public void Tick()
        {
            if (!started)
                return;

            CurrentTick++;
            _netManager.PollEvents();
        }

        IMessageHandler _messageHandler = null;

        

        NetworkStatistics lastStats = NetworkStatistics.Empty;

        public NetworkStatistics GetStatistics()
        {
            return new NetworkStatistics(
                _netManager != null ? _netManager.Statistics.BytesSent : 0,
                _netManager != null ? _netManager.Statistics.BytesReceived : 0,
                CurrentTime
                );
        }

        public NetworkStatistics GetDiffStatistics()
        {
            var currentStats = GetStatistics();
            var diff = new NetworkStatistics(lastStats, currentStats);
            lastStats = currentStats;
            return diff;
        }

        public bool Start(IMessageHandler messageHandler)
        {
            if (started)
                return false;

            _dataWriter = new NetDataWriter();
            _netManager = new NetManager(this);
            _netManager.UpdateTime = networkConfig.TickIntervalMS;
            _netManager.EnableStatistics = true;

            _messageHandler = messageHandler;

            if (_messageHandler != null)
            {
                OnConnected += _messageHandler.OnPeerConnected;
                OnDisconnected += _messageHandler.OnPeerDisconnected;

                _messageHandler.OnNetworkStart();
            }

            if (!StartInternal())
            {
                if (_messageHandler != null)
                {
                    _messageHandler.OnNetworkStop();

                    OnConnected -= _messageHandler.OnPeerConnected;
                    OnDisconnected -= _messageHandler.OnPeerDisconnected;

                    _messageHandler = null;
                }
                return false;
            }
            started = true;
            SetCurrentTick(0);
            return true;
        }

        protected abstract bool StartInternal();

        public void Stop()
        {
            if (!started)
                return;

            if (_netManager != null)
            {
                if (_messageHandler != null)
                {
                    _messageHandler.OnNetworkStop();

                    OnConnected -= _messageHandler.OnPeerConnected;
                    OnDisconnected -= _messageHandler.OnPeerDisconnected;

                    _messageHandler = null;
                }

                _netManager.Stop();

                started = false;
                SetCurrentTick(0);
            }
        }
        protected abstract void Log(string str);

        public virtual void OnConnectionRequest(ConnectionRequest request)
        {
            Log($"ConnectionRequest Received!");
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Log($"Connected to {peer.Id} ({peer})");
            OnConnected?.Invoke(peer);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (reader.TryReadNetworkMessage(out var msg))
            {
                //Log($"{msg.GetType()} Received!");
                msg.Receive(peer);
                //MessageReceived?.Invoke(peer, msg);
            }
            else
            {
                Log($"Unknown Message Received -  did you forget to register it with TryReadNetworkMessage?");
            }
                reader.Recycle();
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            string peerName = peer != null ? $"{peer.Id} ({peer})" : "Null connection";

            Log($"{peer.Id} ({peer}) Disconnected: {disconnectInfo.Reason}");
            OnDisconnected?.Invoke(peer);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Log($"OnNetworkReceiveUnconnected!");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Log($"OnNetworkLatencyUpdate!");
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            Log($"Error: {socketErrorCode}");
        }

        public enum ConnectionState : byte
        {
            Uninitialized,
            Inactive,
            Started,
            Connected
        }

        ConnectionState currentConnectionState = ConnectionState.Uninitialized;

        public ConnectionState CheckConnectionState(out bool stateChanged)
        {
            var newState = GetConnectionState();
            if (newState != currentConnectionState)
            {
                stateChanged = true;
                currentConnectionState = newState;
            }
            else
            {
                stateChanged = false;
            }
            return newState;
        }

        ConnectionState GetConnectionState()
        {
            if (!started)
            {
                return ConnectionState.Inactive;
            }

            if (IsConnected())
                return ConnectionState.Connected;

            return ConnectionState.Started;
        }

        public event Action<NetPeer> OnConnected;
        public event Action<NetPeer> OnDisconnected;
    }
}