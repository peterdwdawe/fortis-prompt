using LiteNetLib;
using Shared;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Player;
using Shared.Projectiles;
using System;
using System.Numerics;
using System.Text.Json;

namespace Server
{
    public class ServerGameManager : GameManager<ServerNetworkManager>, INetLogger
    {
        private int serverPort;

        protected override ServerNetworkManager GenerateNetworkManager()
        {
            return new ServerNetworkManager(networkConfig, serverPort);
        }

        protected override void Log(string message)
        {
            Console.WriteLine(message);
        }

        void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
        {
            Console.WriteLine(string.Format(str, args));
        }

        protected override Player CreateNewPlayer(int ID, IInputListener inputListener, bool local)
        {
            Log("Create New Player!");

            var player = new ServerPlayer(ID, inputListener, local, playerConfig, networkConfig);

            player.Spawned += OnPlayerSpawn;
            player.Died += OnPlayerDied;
            player.HPSet += OnPlayerHPSet;
            player.RespawnRequested += SpawnPlayerAtRandomLocation;

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

            return player;
        }

        private Vector3 GetRandomSpawnPosition(IPlayer player)
        {
            return new Vector3
                (
                (Random.Shared.NextSingle() * 8f) - 4f,
                1f,
                (Random.Shared.NextSingle() * 8f) - 4f
                );
        }

        protected override void OnPlayerDestroyed(IPlayer player)
        {
            player.Spawned -= OnPlayerSpawn;
            player.Died -= OnPlayerDied;
            player.HPSet -= OnPlayerHPSet;
            player.RespawnRequested -= SpawnPlayerAtRandomLocation;
            base.OnPlayerDestroyed(player);
            networkManager.SendToAll(new PlayerDeregistrationMessage(player.ID));
        }

        private void SpawnPlayerAtRandomLocation(int playerID)
        {
            if (!TryGetPlayer(playerID, out var player))
            {
                Console.WriteLine("Failed to spawn player - not found in lookup!");
                return;
            }
            SpawnPlayerAtRandomLocation(player);
        }
        private void SpawnPlayerAtRandomLocation(IPlayer player)
        {
            player.SetHP(playerConfig.MaxHP);
            player.Spawn(GetRandomSpawnPosition(player), Quaternion.Identity);
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

        protected override Projectile InstantiateProjectileInternal(int ID, int ownerID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        {
            var projectile = new ServerProjectile(ID, ownerID, position, direction, projectileConfig, networkConfig);

            projectile.Moved += OnProjectileMoved;

            networkManager.SendToAll(new ProjectileSpawnMessage(ID, ownerID, position, direction));

            return projectile;
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
                hitPlayer.SetHP(hitPlayer.HP - projectileConfig.Damage);
            }
        }

        private bool CollisionCheck(IProjectile projectile, out IPlayer hitPlayer)
        {
            hitPlayer = null;
            bool foundPlayer = false;
            float targetSqrDist = playerConfig.Radius * playerConfig.Radius;

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

        private void InstantiateProjectile(int playerID, Vector3 position, Vector3 direction)
        {
            lastProjectileID = GetNextProjectileID();
            InstantiateProjectile(lastProjectileID, playerID, position, direction);
        }

        protected override void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            base.ApplyNetworkedMovement(playerID, input, position, rotation);
            networkManager.SendToAllExcept(playerID, new PlayerUpdateMessage(playerID, input, position, rotation));
        }

        public bool StartServer(int port)
        {
            serverPort = port;
            return StartNetworkingInternal();
        }

        protected override T Deserialize<T>(string jsonString) where T : class
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        protected override string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        #region Network Message Handling

        protected override IMessageHandler GenerateMessageHandler()
        {
            var handler = new MessageEventHandler();

            handler.PeerConnected += OnPeerConnected;
            handler.PeerDisconnected += OnPeerDisconnected;

            handler.CustomMessageReceived += OnCustomMessageReceived;
            handler.PlayerUpdateReceived += OnPlayerUpdateReceived;
            handler.ProjectileSpawnRequested += OnProjectileSpawnRequested;

            handler.NetworkStopped += Cleanup;

            return handler;
        }

        private void OnPeerConnected(int peerID)
        {
            InstantiateNetworkedPlayer(peerID);
            SpawnPlayerAtRandomLocation(peerID);
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

        private void OnProjectileSpawnRequested(RequestProjectileSpawnMessage message)
        {
            InstantiateProjectile(message.ownerID, message.position, message.direction);
        }

        #endregion
    }
}