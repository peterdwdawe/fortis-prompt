using LiteNetLib;

namespace Shared.Networking
{
    public interface IMessageHandler
    {
        void OnNetworkStart();
        void OnNetworkStop();

        void OnPeerConnected(NetPeer peer);
        void OnPeerDisconnected(NetPeer peer);
    }
}
