using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using System;

namespace Server
{
    internal class ServerMessageHandler : MessageHandler<ServerGameManager, ServerNetworkManager>
    {
        public ServerMessageHandler(ServerGameManager gameManager) : base(gameManager)
        {
        }
        protected override void Log(string message)
            => Console.WriteLine(message);

        protected override void CustomMessage_Received(NetPeer peer, CustomMessage message)
        {
            //Debug message, currently unused. this was just used to ping-pong messages, confirming that both client and server could send and receive
            //Console.WriteLine($"Server Received: {message.msg} from peer {peer.Id}");
            //gameManager.networkManager.SendTo(peer, message);
            //Console.WriteLine($"Server Sent: {message.msg} to peer {peer.Id}");
        }

        protected override void PlayerDeathMessage_Received(NetPeer peer, PlayerDeathMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void HealthUpdateMessage_Received(NetPeer peer, HealthUpdateMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void PlayerDeregistrationMessage_Received(NetPeer peer, PlayerDeregistrationMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void PlayerRegistrationMessage_Received(NetPeer peer, PlayerRegistrationMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void PlayerUpdateMessage_Received(NetPeer peer, PlayerUpdateMessage message)
        {
            gameManager.ApplyNetworkedMovement(message.playerID, message.input, message.position, message.rotation);
        }

        protected override void ProjectileDespawnMessage_Received(NetPeer peer, ProjectileDespawnMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void ProjectileSpawnMessage_Received(NetPeer peer, ProjectileSpawnMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void RequestProjectileSpawnMessage_Received(NetPeer peer, RequestProjectileSpawnMessage message)
        {
            gameManager.InstantiateProjectile(message.ownerID, message.position, message.direction);
        }

        protected override void PlayerSpawnMessage_Received(NetPeer peer, PlayerSpawnMessage message)
        {
            LogBadMessage(peer, message);
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            //Log("Peer {peer.Id} Connected.");
            gameManager.InstantiateNetworkedPlayer(peer.Id);
            gameManager.SpawnPlayerAtRandomLocation(peer.Id);
        }

        public override void OnPeerDisconnected(NetPeer peer)
        {
            gameManager.DestroyPlayer(peer.Id);
            gameManager.OnPeerDisconnected();
        }
    }
}
