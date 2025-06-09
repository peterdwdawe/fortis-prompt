using LiteNetLib;
using Shared;
using Shared.Configuration;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RPC;
using Shared.Player;
using Shared.Projectiles;
using System;
using System.Numerics;

namespace Server
{
    public class ServerGameManager : GameManager<INetworkServer>, INetLogger
    {
        protected override INetworkServer GenerateNetworkManager()
        {

            var manager = new ServerNetworkManager(serverConfig.RpcTimeout, serverConfig.NetworkKey, gameConfig.MaxPlayerCount);
            manager.PeerConnected += OnPeerConnected;
            manager.PeerDisconnected += OnPeerDisconnected;

            manager.CustomMessageReceived += OnCustomMessageReceived;
            manager.PlayerUpdateReceived += OnPlayerUpdateReceived;
            manager.ProjectileSpawned += OnProjectileSpawnReceived;
            manager.SpawnProjectileHandler = HandleSpawnProjectileRpcRequest;

            manager.NetworkStopped += Cleanup;
            return manager;
        }


        protected const string serverConfigPath = "ServerConfig.json";
        protected const string gameConfigPath = "GameConfig.json";

        public ServerConfig serverConfig { get; private set; }
        public GameConfig gameConfig { get; private set; }

        protected override void GetConfigData()
        {
            serverConfig = new ServerConfig(LoadConfig<ServerConfigData>(serverConfigPath));
            gameConfig = new GameConfig(LoadConfig<GameConfigData>(gameConfigPath));
        }

        private SpawnProjectileRpcResponseMessage HandleSpawnProjectileRpcRequest(NetPeer peer, SpawnProjectileRpcRequestMessage requestMessage)
        {
            //Log("SpawnProjectile RPC Request Received.");
            lastProjectileID = GetNextProjectileID();
            return SpawnProjectileRpcResponseMessage.Approved(lastProjectileID, requestMessage.ownerID, requestMessage.position, requestMessage.direction);
        }

        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }

        void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Console.WriteLine(string.Format(str, args));
        }

        protected override Player InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local)
        {
            Log("Create New Player!");

            var player = new ServerPlayer(ID, inputListener, local, gameConfig);

            player.Spawned += OnPlayerSpawn;
            player.Died += OnPlayerDied;
            player.HPSet += OnPlayerHPSet;

            networkManager.SendTo(ID, new PlayerRegistrationMessage(ID, true));
            foreach (var otherPlayer in AllPlayers)
            {
                if (otherPlayer.ID == ID)
                    continue;
                networkManager.SendTo(ID, new PlayerRegistrationMessage(otherPlayer.ID, false));
                networkManager.SendTo(ID, new PlayerHPUpdateMessage(otherPlayer.ID, otherPlayer.HP));
                if (otherPlayer.HP > 0)
                {
                    networkManager.SendTo(ID, new PlayerSpawnMessage(otherPlayer.ID, otherPlayer.Position, otherPlayer.Rotation));
                }
            }
            foreach (var projectile in AllProjectiles)
            {
                networkManager.SendTo(ID, new ProjectileSpawnMessage(projectile.ID, projectile.ownerID, projectile.Position, projectile.Direction));
            }

            networkManager.SendToAllExcept(ID, new PlayerRegistrationMessage(ID, false));

            player.SpawnAtRandomLocation();

            return player;
        }

        protected override void OnPlayerDestroyed(IPlayer player)
        {
            player.Spawned -= OnPlayerSpawn;
            player.Died -= OnPlayerDied;
            player.HPSet -= OnPlayerHPSet;
            base.OnPlayerDestroyed(player);
            networkManager.SendToAll(new PlayerDeregistrationMessage(player.ID));
        }

        private void OnPlayerSpawn(IPlayer player)
        {
            networkManager.SendToAll(new PlayerSpawnMessage(player.ID, player.Position, player.Rotation));
        }

        private void OnPlayerDied(IPlayer player)
        {
            networkManager.SendToAll(new PlayerDeathMessage(player.ID));
        }

        private void OnPlayerHPSet(IPlayer player)
        {
            networkManager.SendToAll(new PlayerHPUpdateMessage(player.ID, player.HP));
            if (player.HP <= 0)
            {
                player.Kill();
            }
        }

        protected override void OnPlayerShot(IPlayer player, IProjectile projectile)
        {
            base.OnPlayerShot(player, projectile);
            projectile.Moved += OnProjectileMoved;
        }

        protected override void OnProjectileDestroyed(IProjectile projectile)
        {
            projectile.Moved -= OnProjectileMoved;

            base.OnProjectileDestroyed(projectile);

            networkManager.SendToAll(new ProjectileDespawnMessage(projectile.ID));
        }

        private void OnProjectileMoved(IProjectile projectile)
        {
            if (CollisionCheck(projectile, out var hitPlayer))
            {
                projectile.Destroy();
                hitPlayer.SetHP(hitPlayer.HP - gameConfig.ProjectileDamage);
            }
        }

        private bool CollisionCheck(IProjectile projectile, out IPlayer hitPlayer)
        {
            hitPlayer = null;
            bool foundPlayer = false;
            float targetSqrDist = gameConfig.PlayerRadius * gameConfig.PlayerRadius;

            foreach (var player in AllPlayers)
            {
                if (player.ID == projectile.ownerID || !player.Alive)
                {
                    continue;
                }
                float sqrDist = (projectile.Position - player.Position).LengthSquared();
                if ((sqrDist < targetSqrDist))
                {
                    hitPlayer = player;
                    foundPlayer = true;
                }
            }

            return foundPlayer;
        }

        private ushort lastProjectileID = ushort.MaxValue;

        private ushort GetNextProjectileID()
        {
            var nextProjectileID = lastProjectileID;

            unchecked
            {
                nextProjectileID++;
                do
                {
                    if (!ProjectileExists(nextProjectileID))
                    {
                        return nextProjectileID;
                    }

                    nextProjectileID++;
                }
                while (nextProjectileID != lastProjectileID);

                nextProjectileID--;
                Console.WriteLine($"Failed to find a valid projectile ID - using already taken ID: {nextProjectileID}");
                return nextProjectileID;
            }
        }

        protected override void OnProjectileSpawnReceived(ProjectileSpawnMessage message)
        {
            base.OnProjectileSpawnReceived(message);
            networkManager.SendToAllExcept(message.ownerID, message);
        }

        protected override void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            base.ApplyNetworkedMovement(playerID, input, position, rotation);
            networkManager.SendToAllExcept(playerID, new PlayerUpdateMessage(playerID, input, position, rotation));
        }

        public bool StartServer()
        {
            return TryStartNetworkingInternal(serverConfig.Port);
        }

        #region Network Message Handling

        private void OnPeerConnected(int peerID)
        {
            networkManager.SendTo(peerID, new GameConfigurationMessage(gameConfig.GetData()));
            InstantiateNetworkedPlayer(peerID);
        }

        private void OnPeerDisconnected(int peerID)
        {
            OnPlayerDisconnected(peerID);
            if (!networkManager.IsConnected())
            {
                Log("All clients disconnected. Cleaning up game data.");
                Cleanup();
            }
        }

        private void OnCustomMessageReceived(CustomMessage message)
        {
            //Debug message, currently unused. this was just used to ping - pong messages, confirming that both client and server could send and receive
            Log($"Received: {message.msg} from peer {message.playerID}");
            networkManager.SendTo(message.playerID, message);
            Console.WriteLine($"Server Sent: {message.msg} to peer {message.playerID}");
        }

        #endregion
    }
}