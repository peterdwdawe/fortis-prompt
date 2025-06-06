using LiteNetLib;

namespace Shared.Networking.Messages
{
    public interface IMessageHandler
    {
        void OnNetworkStart();
        void OnNetworkStop();

        void OnPeerConnected(NetPeer peer);
        void OnPeerDisconnected(NetPeer peer);
    }
}
