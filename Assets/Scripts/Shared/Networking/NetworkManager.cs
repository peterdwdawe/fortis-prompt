using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Networking.Messages;
using Shared.Networking.RPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Shared.Networking
{
    public class NetworkManager : INetworkManager
    {
        protected readonly float _rpcTimeout;
        protected readonly string _networkKey;

        protected readonly NetManager _netManager;

        protected readonly EventBasedNetListener _listener;

        protected readonly NetDataWriter _dataWriter;

        protected NetworkManager(float rpcTimeout, string networkKey)
        {
            _rpcTimeout = rpcTimeout;
            _networkKey = networkKey;
            //tickInterval = networkConfig.TickInterval;

            _dataWriter = new NetDataWriter();

            _listener = new EventBasedNetListener();
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceive;
            _listener.NetworkErrorEvent += OnNetworkError;

            _netManager = new NetManager(_listener);
            _netManager.EnableStatistics = true;
            _netManager.ChannelsCount = (byte)ChannelType.LAST;
        }

        public bool started { get; private set; } = false;

        private float CurrentTime = 0f;

        public bool TryStartNetworking(int port = 0)
        {
            if (started)
                return false;

            NetworkStarted?.Invoke();

            if (!_netManager.Start(port))
            {
                NetworkStopped?.Invoke();
                return false;
            }
            started = true;
            CurrentTime = 0f;
            return true;
        }

        public void Update(float deltaTime)
        {
            if (!started)
                return;

            CurrentTime += deltaTime;

            while(queuedMessageReceives.Count > 0)
            {
                (var peer, var msg) = queuedMessageReceives.Dequeue();

                OnReceiveStandardMessage(peer, msg);
            }

            _netManager.PollEvents();
        }

        public void StopNetworking()
        {
            if (!started)
                return;

            if (_netManager != null)
            {
                _netManager.Stop();
                NetworkStopped?.Invoke();

                started = false;
                CurrentTime = 0f;
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            PeerConnected?.Invoke(peer.Id);
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            string peerName = peer != null ? $"{peer.Id} ({peer})" : "Null connection";

            Log($"{peer.Id} ({peer}) Disconnected: {disconnectInfo.Reason}");
            PeerDisconnected?.Invoke(peer.Id);
        }

        private void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            Log($"Error: {socketErrorCode}");
        }

        #region Bandwidth Tracking

        NetworkStatistics lastStats = NetworkStatistics.Empty;

        public NetworkStatistics GetTotalStatistics()
        {
            return new NetworkStatistics(
                _netManager.Statistics.BytesSent,
                _netManager.Statistics.BytesReceived,
                CurrentTime
                );
        }

        public NetworkStatistics GetDiffStatistics()
        {
            var currentStats = GetTotalStatistics();
            var diff = new NetworkStatistics(lastStats, currentStats);
            lastStats = currentStats;
            return diff;
        }
        #endregion

        #region Connection Status

        public bool IsConnected()
            => started && _netManager.ConnectedPeersCount > 0;

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
                return ConnectionState.Inactive;

            if (IsConnected())
                return ConnectionState.Connected;

            return ConnectionState.Started;
        }

        protected void Log(string str) 
            => MessageLogged?.Invoke(str);

        protected enum ChannelType : byte
        {
            Standard = 0,
            RpcRequest = 1,
            RpcResponse = 2,
            LAST
        }

        #endregion

        #region Send Functions

        protected void SendToAll(INetSerializable message, ChannelType channelType, DeliveryMethod deliveryMethod)
        {
            if (!IsConnected())
            {
                //Log($"{message.GetType()} SendToAll failed: not connected!");
                return;
            }

            _dataWriter.Reset();
            _dataWriter.Put(message);
            _netManager.SendToAll(_dataWriter, (byte)channelType, deliveryMethod);
        }

        protected void SendToAllExcept(int playerID, INetSerializable message, ChannelType channelType, DeliveryMethod deliveryMethod)
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
                _peer.Send(_dataWriter, (byte) channelType, deliveryMethod);
            }
        }

        protected void SendToAllExcept(NetPeer peer, INetSerializable message, ChannelType channelType, DeliveryMethod deliveryMethod)
            => SendToAllExcept(peer.Id, message, channelType, deliveryMethod);

        protected void SendTo(int playerID, INetSerializable message, ChannelType channelType, DeliveryMethod deliveryMethod)
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
            SendTo(peer, message, channelType, deliveryMethod);
        }

        protected void SendTo(NetPeer peer, INetSerializable message, ChannelType channelType, DeliveryMethod deliveryMethod)
        {
            if (!IsConnected())
            {
                //Log($"{message.GetType()} SendTo {peer.Id} failed: not connected!");
                return;
            }

            _dataWriter.Reset();
            _dataWriter.Put(message);
            peer.Send(_dataWriter, (byte)channelType, deliveryMethod);
        }
        
        protected void SendToFirstPeer(INetSerializable message, ChannelType channelType, DeliveryMethod deliveryMethod)
            => SendTo(_netManager.FirstPeer, message, channelType, deliveryMethod);

        #endregion

        #region Receive functions

        void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            switch ((ChannelType)channelNumber)
            {
                case ChannelType.Standard:
                    if (!reader.TryReadStandardNetworkMessage(out var stdMsg))
                    {
                        Log($"Unknown Standard Message Received -  did you forget to register it with TryReadNetworkMessage?");
                        break;
                    }

                    if (awaitingRpc)
                        queuedMessageReceives.Enqueue((peer, stdMsg));
                    else
                        OnReceiveStandardMessage(peer, stdMsg);
                    break;

                case ChannelType.RpcRequest:
                    if (!reader.TryReadRpcRequestMessage(out var rpcRequest))
                    {
                        Log($"Unknown RpcRequest Message Received -  did you forget to register it with TryReadRpcRequestMessage?");
                        break;
                    }

                    OnReceiveRpcRequest(peer, rpcRequest);
                    break;

                case ChannelType.RpcResponse:
                    if (!reader.TryReadRpcResponseMessage(out var rpcResponse))
                    {
                        Log($"Unknown RpcResponse Message Received -  did you forget to register it with TryReadRpcResponseMessage?");
                        break;
                    }

                    if (awaitingRpc)
                        receivedResponse = rpcResponse;
                    else
                        Log($"Received RpcResponse: {rpcResponse.MsgType} without requesting it. ignoring...");
                    break;

                default:
                    Log($"Error: Message received on unregistered channel: {channelNumber}.");
                    break;
            }
            reader.Recycle();
        }

        void OnReceiveStandardMessage(NetPeer peer, IStandardNetworkMessage msg)
        {

            switch (msg.MsgType)
            {
                case StandardMessageType.CustomMessage:
                    CustomMessageReceived?.Invoke((CustomMessage)msg);
                    break;
                case StandardMessageType.GameConfiguration:
                    GameConfigReceived?.Invoke((GameConfigurationMessage)msg);
                    break;
                case StandardMessageType.PlayerRegistration:
                    PlayerRegistered?.Invoke((PlayerRegistrationMessage)msg);
                    break;
                case StandardMessageType.PlayerDeregistration:
                    PlayerDeregistered?.Invoke((PlayerDeregistrationMessage)msg);
                    break;
                case StandardMessageType.PlayerUpdate:
                    PlayerUpdateReceived?.Invoke((PlayerUpdateMessage)msg);
                    break;
                //case MessageType.RequestProjectileSpawn:
                //    ProjectileSpawnRequested?.Invoke((RequestProjectileSpawnMessage)msg);
                //    break;
                case StandardMessageType.ProjectileSpawn:
                    ProjectileSpawned?.Invoke((ProjectileSpawnMessage)msg);
                    break;
                case StandardMessageType.ProjectileDespawn:
                    ProjectileDespawned?.Invoke((ProjectileDespawnMessage)msg);
                    break;
                case StandardMessageType.PlayerHPUpdate:
                    PlayerHPUpdated?.Invoke((PlayerHPUpdateMessage)msg);
                    break;
                case StandardMessageType.PlayerDeath:
                    PlayerDied?.Invoke((PlayerDeathMessage)msg);
                    break;
                case StandardMessageType.PlayerSpawn:
                    PlayerSpawned?.Invoke((PlayerSpawnMessage)msg);
                    break;
                default:
                    Log($"Unhandled Standard Message Received: {msg.MsgType}!");
                    break;
            }
        }

        IRpcResponseMessage receivedResponse = null;

        void OnReceiveRpcRequest(NetPeer peer, IRpcRequestMessage request)
        {
            if(!TryExecuteRpcRequest(peer, (SpawnProjectileRpcRequestMessage)request, out IRpcResponseMessage response))
            {
                Log($"Unhandled RpcRequest Message Received: {request.MsgType}!");
                return;
            }

            SendRpcResponseTo(peer, response);
        }

        bool TryExecuteRpcRequest(NetPeer peer, IRpcRequestMessage request, out IRpcResponseMessage response)
        {
            switch (request.MsgType)
            {
                case RpcMessageType.SpawnProjectile:
                    response = SpawnProjectileHandler(peer, (SpawnProjectileRpcRequestMessage)request);
                    return true;

                default:
                    response = null;
                    return false;
            }
        }


        #endregion

        #region Rpc Send & Receive

        protected bool awaitingRpc = false;
        protected Queue<(NetPeer, IStandardNetworkMessage)> queuedMessageReceives = new Queue<(NetPeer, IStandardNetworkMessage)>();

        protected TResponse SendRpcRequest<TResponse>(int playerID, IRpcRequestMessage<TResponse> message)
            where TResponse : IRpcResponseMessage
        {
            SendTo(playerID, message, ChannelType.RpcRequest, DeliveryMethod.ReliableUnordered);
            return WaitForRpcResponse<TResponse>();
        }

        protected TResponse SendRpcRequest<TResponse>(NetPeer peer, IRpcRequestMessage<TResponse> message)
            where TResponse : IRpcResponseMessage
        {
            SendTo(peer, message, ChannelType.RpcRequest, DeliveryMethod.ReliableUnordered);
            return WaitForRpcResponse<TResponse>();
        }

        protected TResponse WaitForRpcResponse<TResponse>()
            where TResponse : IRpcResponseMessage
        {
            //Log("WaitForRpcResponse!");
            int rpcTimeoutMS = (int)(_rpcTimeout * 1000);
            try
            {
                awaitingRpc = true;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (receivedResponse == null && stopwatch.ElapsedMilliseconds < rpcTimeoutMS)
                {
                    _netManager.PollEvents();
                }
            }
            finally
            {
                awaitingRpc = false;
            }

            if (receivedResponse != null)
            {
                if (receivedResponse is TResponse result)
                {
                    receivedResponse = null;
                    return result;
                }
                else
                {
                    Log($"Received incorrect RPC response type: expected {typeof(TResponse)}, {receivedResponse.GetType()} (Message type: {receivedResponse.MsgType}).");
                    receivedResponse = null;
                    return default;
                }
            }
            else
            {
                Log($"Failed to receive {typeof(TResponse)} after {rpcTimeoutMS}ms.");
                return default;
            }
        }

        private void SendRpcResponseTo(NetPeer peer, IRpcResponseMessage message)
            => SendTo(peer, message, ChannelType.RpcResponse, DeliveryMethod.ReliableUnordered);

        #endregion

        #region Events

        public event Action<int> PeerConnected;
        public event Action<int> PeerDisconnected;

        public event Action NetworkStarted;
        public event Action NetworkStopped;

        public event Action<string> MessageLogged;

        public event Action<CustomMessage> CustomMessageReceived;
        public event Action<GameConfigurationMessage> GameConfigReceived;

        public event Action<PlayerRegistrationMessage> PlayerRegistered;
        public event Action<PlayerHPUpdateMessage> PlayerHPUpdated;
        public event Action<PlayerSpawnMessage> PlayerSpawned;
        public event Action<PlayerUpdateMessage> PlayerUpdateReceived;
        public event Action<PlayerDeathMessage> PlayerDied;
        public event Action<PlayerDeregistrationMessage> PlayerDeregistered;

        public event Action<ProjectileSpawnMessage> ProjectileSpawned;
        public event Action<ProjectileDespawnMessage> ProjectileDespawned;

        #endregion

        #region Rpc Handlers

        public RpcRequestHandler<SpawnProjectileRpcRequestMessage, SpawnProjectileRpcResponseMessage> SpawnProjectileHandler 
            = CantHandleRequest<SpawnProjectileRpcRequestMessage, SpawnProjectileRpcResponseMessage>;

        public delegate TResponse RpcRequestHandler<TRequest, TResponse>(NetPeer peer, TRequest requestMessage)
            where TRequest : struct, IRpcRequestMessage<TRequest, TResponse>
            where TResponse : struct, IRpcResponseMessage<TResponse>, System.IEquatable<TResponse>;

        protected static TResponse CantHandleRequest<TRequest, TResponse>(NetPeer peer, TRequest requestMessage)
            where TRequest : struct, IRpcRequestMessage<TRequest, TResponse>
            where TResponse : struct, IRpcResponseMessage<TResponse>, System.IEquatable<TResponse>
        {
            Console.WriteLine($"No handler assigned for {typeof(TRequest)}! returning default {typeof(TResponse)}.");
            return default;
        }

        #endregion
    }
}
