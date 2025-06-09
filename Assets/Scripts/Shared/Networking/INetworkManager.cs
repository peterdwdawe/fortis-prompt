using System;

namespace Shared.Networking
{
    public interface INetworkManager
    {
        event Action NetworkStarted;
        event Action NetworkStopped;

        event Action<int> PeerConnected;
        event Action<int> PeerDisconnected;

        event Action<string> MessageLogged;

        bool TryStartNetworking(int port = 0);
        void StopNetworking();

        bool started { get; }
        bool IsConnected();

        NetworkManager.ConnectionState CheckConnectionState(out bool stateChanged);

        NetworkStatistics GetDiffStatistics();
        NetworkStatistics GetTotalStatistics();

        void Update(float deltaTime);
    }
}