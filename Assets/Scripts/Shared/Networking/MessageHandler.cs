using LiteNetLib;
using Shared.Networking.Messages;

namespace Shared.Networking
{
    //TODO: try to get rid of all these generic type constraints in favor of interfaces
    public abstract class MessageHandler<TGameManager, TNetworkManager> : IMessageHandler
        where TGameManager : GameManager<TGameManager, TNetworkManager>
        where TNetworkManager : NetworkManager
    {
        protected TGameManager gameManager;

        protected MessageHandler(TGameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void OnNetworkStart()
        {
            CustomMessage.Received += CustomMessage_Received;

            PlayerRegistrationMessage.Received += PlayerRegistrationMessage_Received;
            PlayerDeregistrationMessage.Received += PlayerDeregistrationMessage_Received;

            PlayerUpdateMessage.Received += PlayerUpdateMessage_Received;

            ProjectileSpawnMessage.Received += ProjectileSpawnMessage_Received;
            RequestProjectileSpawnMessage.Received += RequestProjectileSpawnMessage_Received;
            ProjectileDespawnMessage.Received += ProjectileDespawnMessage_Received;

            HealthUpdateMessage.Received += HealthUpdateMessage_Received;

            PlayerDeathMessage.Received += PlayerDeathMessage_Received;
            PlayerSpawnMessage.Received += PlayerSpawnMessage_Received;
        }

        public void OnNetworkStop()
        {
            CustomMessage.Received -= CustomMessage_Received;

            PlayerRegistrationMessage.Received -= PlayerRegistrationMessage_Received;
            PlayerDeregistrationMessage.Received -= PlayerDeregistrationMessage_Received;

            PlayerUpdateMessage.Received -= PlayerUpdateMessage_Received;

            ProjectileSpawnMessage.Received -= ProjectileSpawnMessage_Received;
            RequestProjectileSpawnMessage.Received -= RequestProjectileSpawnMessage_Received;
            ProjectileDespawnMessage.Received -= ProjectileDespawnMessage_Received;

            HealthUpdateMessage.Received -= HealthUpdateMessage_Received;

            PlayerDeathMessage.Received -= PlayerDeathMessage_Received;
            PlayerSpawnMessage.Received -= PlayerSpawnMessage_Received;

            gameManager.Cleanup();
        }

        protected abstract void CustomMessage_Received(NetPeer peer, CustomMessage message);
        protected abstract void PlayerDeregistrationMessage_Received(NetPeer peer, PlayerDeregistrationMessage message);
        protected abstract void PlayerRegistrationMessage_Received(NetPeer peer, PlayerRegistrationMessage message);

        protected abstract void PlayerUpdateMessage_Received(NetPeer peer, PlayerUpdateMessage message);

        protected abstract void RequestProjectileSpawnMessage_Received(NetPeer peer, RequestProjectileSpawnMessage message);
        protected abstract void ProjectileSpawnMessage_Received(NetPeer peer, ProjectileSpawnMessage message);
        protected abstract void ProjectileDespawnMessage_Received(NetPeer peer, ProjectileDespawnMessage message);

        protected abstract void HealthUpdateMessage_Received(NetPeer peer, HealthUpdateMessage message);

        protected abstract void PlayerDeathMessage_Received(NetPeer peer, PlayerDeathMessage message);
        protected abstract void PlayerSpawnMessage_Received(NetPeer peer, PlayerSpawnMessage message);

        protected void LogBadMessage(NetPeer peer, INetworkMessage message)
        {
            Log($"Server received bad message of type: {message.GetType()} from peer {peer.Id}");
        }

        protected abstract void Log(string message);

        public abstract void OnPeerConnected(NetPeer peer);

        public abstract void OnPeerDisconnected(NetPeer peer);
    }
}
