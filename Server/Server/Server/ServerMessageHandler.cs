using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Networking.Messages;
using Shared.Networking;
using System;
using System.Collections.Generic;
using System.Text;

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
            Log("Peer Connected!");
            gameManager.InstantiateNetworkedPlayer(peer.Id);
            //TODO(); //instantiate player
            //    TODO(); //send local PlayerRegistrationMessage with peer.Id to sender
            //    TODO(); //send networked PlayerRegistrationMessage with peer.Id to all others
                //TODO(); //send PlayerSpawnMessage with all non-dead players to sender

            gameManager.SpawnPlayerAtRandomLocation(peer.Id);
            //TODO(); //spawn player with maxHP
            //    TODO(); //send PlayerSpawnMessage with peer.Id to all
        }

        public override void OnPeerDisconnected(NetPeer peer)
        {
            gameManager.DestroyPlayer(peer.Id);
            gameManager.OnPeerDisconnected();
            //TODO(); //Destroy Player
            //    TODO(); //send message to all remaining peers
        }
    }
}
