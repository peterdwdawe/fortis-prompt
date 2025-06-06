using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Configuration;
using System;
using System.Net;
using System.Net.Sockets;

namespace Shared.Networking
{
    public abstract partial class NetworkManager : INetEventListener 
    {
        protected readonly int _port;
        protected NetDataWriter _dataWriter;
        protected NetManager _netManager;
        protected NetworkState networkState;

        public bool started { get; private set; } = false;

        public NetworkManager(NetworkState networkState, int port)
        {
            this.networkState = networkState;
            _port = port;
        }

        public bool IsConnected()
            => started && _netManager.ConnectedPeersCount > 0;

        public void PollEvents()
        {
            if (!started)
                return;

            //Log("Poll Events!");
            _netManager.PollEvents();
        }

        IMessageHandler _messageHandler = null;

        

        NetworkStatistics lastStats = NetworkStatistics.Empty;

        public NetworkStatistics GetStatistics()
        {
            return new NetworkStatistics(
                _netManager != null ? _netManager.Statistics.BytesSent : 0,
                _netManager != null ? _netManager.Statistics.BytesReceived : 0,
                networkState.CurrentTime
                );
        }

        public NetworkStatistics GetDiffStatistics()
        {
            var currentStats = GetStatistics();
            var diff = new NetworkStatistics(lastStats, currentStats);
            lastStats = currentStats;
            return diff;
        }

        public int GetPeerCount()
            => _netManager != null ? _netManager.ConnectedPeersCount : 0;

        public bool Start(IMessageHandler messageHandler)
        {
            if (started)
                return false;

            _dataWriter = new NetDataWriter();
            _netManager = new NetManager(this);
            _netManager.UpdateTime = networkState.config.TickIntervalMS;
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
            networkState.SetCurrentTick(0);
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
                networkState.SetCurrentTick(0);
            }
        }
        protected abstract void Log(string str);

        public virtual void OnConnectionRequest(ConnectionRequest request)
        {
            Log($"ConnectionRequest Received!");
        }

        public virtual void OnPeerConnected(NetPeer peer)
        {
            Log($"Connected to {peer.Id} ({peer})");
            OnConnected?.Invoke(peer);
        }

        public virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
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

        public virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            string peerName = peer != null ? $"{peer.Id} ({peer})" : "Null connection";

            Log($"{peer.Id} ({peer}) Disconnected: {disconnectInfo.Reason}");
            OnDisconnected?.Invoke(peer);
        }

        public virtual void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Log($"OnNetworkReceiveUnconnected!");
        }

        public virtual void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            //Log($"OnNetworkLatencyUpdate!");
        }

        public virtual void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
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

        public ConnectionState currentConnectionState { get; private set; } = ConnectionState.Uninitialized;

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