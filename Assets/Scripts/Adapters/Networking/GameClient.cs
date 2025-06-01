using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Networking;
using System;

public class GameClient : INetEventListener
{
    private NetManager _netClient;
    private NetDataWriter _dataWriter;

    private readonly int _port;
    private readonly int _updateTime;

    public bool started { get; private set; } = false;

    public bool IsConnected() => IsConnected(out _);
    public bool IsConnected(out NetPeer peer)
    {
        if (!started)
        {
            peer = default;
            return false;
        }

        peer = _netClient.FirstPeer;
        return peer != null && peer.ConnectionState == ConnectionState.Connected;
    }

    public GameClient(int port, float tickInterval, bool startImmediately = true)
    {
        _port = port;
        _updateTime = Math.Max(1, (int)tickInterval * 1000);

        if (startImmediately)
        {
            Start();
        }
    }

    public void Start()
    {
        if (started)
            return;

        started = true;
        _netClient = new NetManager(this);
        _dataWriter = new NetDataWriter();
        _netClient.UnconnectedMessagesEnabled = true;
        _netClient.UpdateTime = _updateTime;
        _netClient.Start();
        _netClient.Connect(NetworkingUtils.testNetworkAddress, _port, NetworkingUtils.testNetworkKey);
    }

    public void Send(INetworkMessage message)
    {
        if (!started || !IsConnected(out var peer))
            return;

        _dataWriter.Reset();
        _dataWriter.Put(message);
        peer.Send(_dataWriter, DeliveryMethod.ReliableOrdered);
    }

    public void Stop()
    {
        if (!started)
            return;

        if (_netClient != null)
        {
            _netClient.Stop();
            started = false;
        }
    }

    public void PollEvents()
    {
        _netClient.PollEvents();

        //var peer = _netClient.FirstPeer;
        //if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        //{
        //    ////Fixed delta set to 0.05
        //    //var pos = _clientBallInterpolated.transform.position;
        //    //pos.x = Mathf.Lerp(_oldBallPosX, _newBallPosX, _lerpTime);
        //    //_clientBallInterpolated.transform.position = pos;

        //    ////Basic lerp
        //    //_lerpTime += Time.deltaTime / Time.fixedDeltaTime;
        //}
        //else
        //{
        //    _netClient.SendBroadcast(new byte[] { 1 }, 5000);
        //}
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[CLIENT] We connected to " + peer);
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Debug.Log("[CLIENT] We received error " + socketErrorCode);
    }

    public event Action<NetPeer, INetworkMessage> MessageReceived;

    void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        Debug.Log("[CLIENT] We received a message!");
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

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0 && reader.GetInt() == 1)
        {
            Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
            _netClient.Connect(remoteEndPoint, "sample_app");
        }
    }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    void INetEventListener.OnConnectionRequest(ConnectionRequest request)
    {

    }

    void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
    }
}
