using LiteNetLib;
using System;

namespace Shared.Networking.Messages
{
    public class MessageEventHandler : IMessageHandler
    {
        public event Action NetworkStarted;
        public event Action NetworkStopped;

        public event Action<int> PeerConnected;
        public event Action<int> PeerDisconnected;

        void IMessageHandler.OnNetworkStart()
        {
            NetworkStarted?.Invoke();

            CustomMessage.Received += OnCustomMessageReceived;

            PlayerRegistrationMessage.Received += OnPlayerRegistrationMessageReceived;
            PlayerDeregistrationMessage.Received += OnPlayerDeregistrationMessageReceived;

            PlayerUpdateMessage.Received += OnPlayerUpdateMessageReceived;

            ProjectileSpawnMessage.Received += OnProjectileSpawnMessageReceived;
            RequestProjectileSpawnMessage.Received += OnRequestProjectileSpawnMessageReceived;
            ProjectileDespawnMessage.Received += OnProjectileDespawnMessageReceived;

            PlayerHPUpdateMessage.Received += OnPlayerHPUpdateMessageReceived;

            PlayerDeathMessage.Received += OnPlayerDeathMessageReceived;
            PlayerSpawnMessage.Received += OnPlayerSpawnMessageReceived;
        }

        void IMessageHandler.OnNetworkStop()
        {
            CustomMessage.Received -= OnCustomMessageReceived;

            PlayerRegistrationMessage.Received -= OnPlayerRegistrationMessageReceived;
            PlayerDeregistrationMessage.Received -= OnPlayerDeregistrationMessageReceived;

            PlayerUpdateMessage.Received -= OnPlayerUpdateMessageReceived;

            ProjectileSpawnMessage.Received -= OnProjectileSpawnMessageReceived;
            RequestProjectileSpawnMessage.Received -= OnRequestProjectileSpawnMessageReceived;
            ProjectileDespawnMessage.Received -= OnProjectileDespawnMessageReceived;

            PlayerHPUpdateMessage.Received -= OnPlayerHPUpdateMessageReceived;

            PlayerDeathMessage.Received -= OnPlayerDeathMessageReceived;
            PlayerSpawnMessage.Received -= OnPlayerSpawnMessageReceived;

            NetworkStopped?.Invoke();
        }

        void IMessageHandler.OnPeerConnected(NetPeer peer) => PeerConnected?.Invoke(peer.Id);
        void IMessageHandler.OnPeerDisconnected(NetPeer peer) => PeerDisconnected?.Invoke(peer.Id);

        public event Action<CustomMessage> CustomMessageReceived;

        public event Action<PlayerRegistrationMessage> PlayerRegistered;
        public event Action<PlayerHPUpdateMessage> PlayerHPUpdated;
        public event Action<PlayerSpawnMessage> PlayerSpawned;
        public event Action<PlayerUpdateMessage> PlayerUpdateReceived;
        public event Action<PlayerDeathMessage> PlayerDied;
        public event Action<PlayerDeregistrationMessage> PlayerDeregistered;

        public event Action<RequestProjectileSpawnMessage> ProjectileSpawnRequested;
        public event Action<ProjectileSpawnMessage> ProjectileSpawned;
        public event Action<ProjectileDespawnMessage> ProjectileDespawned;

        private void OnCustomMessageReceived(NetPeer peer, CustomMessage message) => CustomMessageReceived?.Invoke(message);

        private void OnPlayerRegistrationMessageReceived(NetPeer peer, PlayerRegistrationMessage message) => PlayerRegistered?.Invoke(message);
        private void OnPlayerHPUpdateMessageReceived(NetPeer peer, PlayerHPUpdateMessage message) => PlayerHPUpdated?.Invoke(message);
        private void OnPlayerSpawnMessageReceived(NetPeer peer, PlayerSpawnMessage message) => PlayerSpawned?.Invoke(message);
        private void OnPlayerUpdateMessageReceived(NetPeer peer, PlayerUpdateMessage message) => PlayerUpdateReceived?.Invoke(message);
        private void OnPlayerDeathMessageReceived(NetPeer peer, PlayerDeathMessage message) => PlayerDied?.Invoke(message);
        private void OnPlayerDeregistrationMessageReceived(NetPeer peer, PlayerDeregistrationMessage message) => PlayerDeregistered?.Invoke(message);

        private void OnRequestProjectileSpawnMessageReceived(NetPeer peer, RequestProjectileSpawnMessage message) => ProjectileSpawnRequested?.Invoke(message);
        private void OnProjectileSpawnMessageReceived(NetPeer peer, ProjectileSpawnMessage message) => ProjectileSpawned?.Invoke(message);
        private void OnProjectileDespawnMessageReceived(NetPeer peer, ProjectileDespawnMessage message) => ProjectileDespawned?.Invoke(message);


    }
}
