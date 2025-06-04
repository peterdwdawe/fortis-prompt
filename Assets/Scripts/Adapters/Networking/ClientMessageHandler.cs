using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Adapters.Networking
{
    public class ClientMessageHandler : MessageHandler<ClientGameManager, ClientNetworkManager>
    {
        public ClientMessageHandler(ClientGameManager gameManager) : base(gameManager)
        {
        }

        protected override void Log(string message)
            => Debug.LogWarning(message);

        protected override void CustomMessage_Received(NetPeer peer, CustomMessage message)
        {
            
        }

        protected override void PlayerDeathMessage_Received(NetPeer peer, PlayerDeathMessage message)
        {
            Log("Received KillPlayer Message!");
            gameManager.KillPlayer(message.playerID);
        }

        protected override void HealthUpdateMessage_Received(NetPeer peer, HealthUpdateMessage message)
        {
            gameManager.SetPlayerHP(message.playerID, message.hp);
        }

        protected override void PlayerDeregistrationMessage_Received(NetPeer peer, PlayerDeregistrationMessage message)
        {
            gameManager.DestroyPlayer(message.playerID);
        }

        protected override void PlayerRegistrationMessage_Received(NetPeer peer, PlayerRegistrationMessage message)
        {
            if (message.localPlayer)
                gameManager.InstantiateLocalPlayer(message.playerID);
            else
                gameManager.InstantiateNetworkedPlayer(message.playerID);
        }

        protected override void PlayerUpdateMessage_Received(NetPeer peer, PlayerUpdateMessage message)
        {
            gameManager.ApplyNetworkedMovement(message.playerID, message.input, message.position, message.rotation);
        }

        protected override void ProjectileDespawnMessage_Received(NetPeer peer, ProjectileDespawnMessage message)
        {
            gameManager.DestroyProjectile(message.projectileID);
        }

        protected override void RequestProjectileSpawnMessage_Received(NetPeer peer, RequestProjectileSpawnMessage message)
        {
            LogBadMessage(peer, message);
        }

        protected override void ProjectileSpawnMessage_Received(NetPeer peer, ProjectileSpawnMessage message)
        {
            gameManager.InstantiateProjectile(message.projectileID, message.ownerID, message.position, message.direction);
        }

        protected override void PlayerSpawnMessage_Received(NetPeer peer, PlayerSpawnMessage message)
        {
            gameManager.SpawnPlayer(message.playerID, message.position, message.rotation);
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            //Do Nothing?
        }

        public override void OnPeerDisconnected(NetPeer peer)
        {
            gameManager.OnServerDisconnected();
        }

    }
}
