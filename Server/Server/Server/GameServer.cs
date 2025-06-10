using LiteNetLib;
using Server.Configuration;
using Server.Networking;
using Server.Player;
using Shared;
using Shared.Configuration;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Networking.RpcMessages;
using Shared.Player;
using Shared.Projectiles;
using System;
using System.Numerics;

namespace Server
{
    public class GameServer : GameManager
    {
        private INetworkServer _server;

        protected override INetworkManager GenerateNetworkManager()
        {

            var server = new NetworkServer(serverConfig.RpcTimeout, serverConfig.NetworkKey, gameConfig.MaxPlayerCount);
            server.PeerConnected += OnPeerConnected;
            server.PeerDisconnected += OnPeerDisconnected;

            server.CustomMessageReceived += OnCustomMessageReceived;
            server.PlayerUpdateReceived += OnPlayerUpdateReceived;
            server.ProjectileSpawned += OnProjectileSpawnReceived;
            server.SpawnProjectileHandler = HandleSpawnProjectileRpcRequest;

            server.NetworkStopped += Cleanup;

            _server = server;

            return server;
        }

        protected const string serverConfigPath = "ServerConfig.json";
        protected const string gameConfigPath = "GameConfig.json";

        private ServerConfig serverConfig;
        private GameConfig gameConfig;

        protected override void GetConfigData()
        {
            serverConfig = new ServerConfig(LoadConfig<ServerConfigData>(serverConfigPath));
            gameConfig = new GameConfig(LoadConfig<GameConfigData>(gameConfigPath));
        }

        public int Port => serverConfig.Port;
        public float TickInterval => serverConfig.TickInterval;

        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }

        public bool StartServer()
        {
            return TryStartNetworkingInternal(serverConfig.Port);
        }

        #region Player Utils & Event Handlers

        protected override void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            base.ApplyNetworkedMovement(playerID, input, position, rotation);
            _server.SendToAllExcept(playerID, new PlayerUpdateMessage(playerID, input, position, rotation));
        }

        protected override IPlayer InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local)
        {
            Log("Create New Player!");

            var player = new ServerPlayer(ID, inputListener, local, gameConfig);

            player.Spawned += OnPlayerSpawn;
            player.Died += OnPlayerDied;
            player.HPSet += OnPlayerHPSet;

            _server.SendTo(ID, new PlayerRegistrationMessage(ID, true));
            foreach (var otherPlayer in AllPlayers)
            {
                if (otherPlayer.ID == ID)
                    continue;
                _server.SendTo(ID, new PlayerRegistrationMessage(otherPlayer.ID, false));
                _server.SendTo(ID, new PlayerHPUpdateMessage(otherPlayer.ID, otherPlayer.HP));
                if (otherPlayer.HP > 0)
                {
                    _server.SendTo(ID, new PlayerSpawnMessage(otherPlayer.ID, otherPlayer.Position, otherPlayer.Rotation));
                }
            }
            foreach (var projectile in AllProjectiles)
            {
                _server.SendTo(ID, new ProjectileSpawnMessage(projectile.ID, projectile.ownerID, projectile.Position, projectile.Direction));
            }

            _server.SendToAllExcept(ID, new PlayerRegistrationMessage(ID, false));

            player.SpawnAtRandomLocation();

            return player;
        }

        private void OnPlayerSpawn(IPlayer player)
        {
            _server.SendToAll(new PlayerSpawnMessage(player.ID, player.Position, player.Rotation));
        }

        private void OnPlayerDied(IPlayer player)
        {
            _server.SendToAll(new PlayerDeathMessage(player.ID));
        }

        private void OnPlayerHPSet(IPlayer player)
        {
            _server.SendToAll(new PlayerHPUpdateMessage(player.ID, player.HP));
            if (player.HP <= 0)
            {
                player.Kill();
            }
        }

        protected override void OnPlayerDisconnected(int playerID)
        {
            base.OnPlayerDisconnected(playerID);

            foreach (var projectile in AllProjectiles)
            {
                if (projectile.ownerID == playerID)
                    projectileCleanupList.Add(projectile);
            }

            foreach (var projectile in projectileCleanupList)
                projectile.Destroy(true);

            projectileCleanupList.Clear();
        }

        protected override void OnPlayerDestroyed(IPlayer player)
        {
            base.OnPlayerDestroyed(player);

            player.Spawned -= OnPlayerSpawn;
            player.Died -= OnPlayerDied;
            player.HPSet -= OnPlayerHPSet;

            _server.SendToAll(new PlayerDeregistrationMessage(player.ID));
        }

        protected override void OnPlayerShot(IPlayer player, IProjectile projectile)
        {
            base.OnPlayerShot(player, projectile);
            projectile.Moved += OnProjectileMoved;
        }

        #endregion

        #region Projectile Utils & Event Handlers

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
            _server.SendToAllExcept(message.ownerID, message);
        }

        protected override void OnProjectileDestroyed(IProjectile projectile)
        {
            projectile.Moved -= OnProjectileMoved;

            base.OnProjectileDestroyed(projectile);

            _server.SendToAll(new ProjectileDespawnMessage(projectile.ID));
        }

        #endregion

        #region Projectile Physics

        private void OnProjectileMoved(IProjectile projectile)
        {
            if (CollisionCheck(projectile, out var hitPlayer))
            {
                projectile.Destroy(false);
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

        #endregion

        #region Network Message Handling

        private void OnPeerConnected(int peerID)
        {
            _server.SendTo(peerID, new GameConfigurationMessage(gameConfig.GetData()));
            InstantiateNetworkedPlayer(peerID);
        }

        private void OnPeerDisconnected(int peerID)
        {
            OnPlayerDisconnected(peerID);
            if (!_server.IsConnected())
            {
                Log("All clients disconnected. Cleaning up game data.");
                Cleanup();
            }
        }

        private void OnCustomMessageReceived(CustomMessage message)
        {
            //Debug message, currently unused. this was just used to ping - pong messages,
            //confirming that both client and server could send and receive
            Log($"Received: {message.msg} from peer {message.playerID}");
            _server.SendTo(message.playerID, message);
            Console.WriteLine($"Server Sent: {message.msg} to peer {message.playerID}");
        }

        private SpawnProjectileRpcResponseMessage HandleSpawnProjectileRpcRequest(NetPeer peer, SpawnProjectileRpcRequestMessage requestMessage)
        {
            //Log("SpawnProjectile RPC Request Received.");
            lastProjectileID = GetNextProjectileID();
            return SpawnProjectileRpcResponseMessage.Approved(lastProjectileID, requestMessage.ownerID, requestMessage.position, requestMessage.direction);
        }

        #endregion
    }
}