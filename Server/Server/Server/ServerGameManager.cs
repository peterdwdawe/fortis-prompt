using LiteNetLib;
using Shared;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Player;
using Shared.Projectiles;
using System;
using System.Numerics;

namespace Server
{
    public class ServerGameManager : GameManager<ServerGameManager,ServerNetworkManager>, INetLogger
    {
        protected override MessageHandler<ServerGameManager, ServerNetworkManager> GenerateMessageHandler()
        {
            return new ServerMessageHandler(this);
        }

        protected override ServerNetworkManager GenerateNetworkManager()
        {
            return new ServerNetworkManager(NetworkConfig.Port, NetworkConfig.TickInterval);
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

            var player =  new ServerPlayer(ID, inputListener, local);

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
                networkManager.SendTo(ID, new HealthUpdateMessage(otherPlayer.ID, otherPlayer.HP));
                if(otherPlayer.HP > 0)
                {
                    networkManager.SendTo(ID, new PlayerSpawnMessage(otherPlayer.ID, otherPlayer.Position, otherPlayer.Rotation));
                }
            }
            foreach(var projectile in AllProjectiles)
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

        public override void DestroyPlayer(int ID)
        {
            if(TryGetPlayer(ID, out var player))
            {


                player.Spawned -= OnPlayerSpawn;
                player.Died -= OnPlayerDied;
                player.HPSet -= OnPlayerHPSet;
                player.RespawnRequested -= SpawnPlayerAtRandomLocation;
            }

            base.DestroyPlayer(ID);

            networkManager.SendToAll(new PlayerDeregistrationMessage(ID));
        }

        public void SpawnPlayerAtRandomLocation(int playerID)
        {
            if (!TryGetPlayer(playerID, out var player))
            {
                Console.WriteLine("Failed to spawn player - not found in lookup!");
                return;
            }
            SpawnPlayerAtRandomLocation(player);
        }
        public void SpawnPlayerAtRandomLocation(IPlayer player)
        {
            player.SetHP(PlayerConfig.MaxHP);
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
            networkManager.SendToAll(new HealthUpdateMessage(player.ID, player.HP));
            if (player.HP <= 0)
            {
                KillPlayer(player.ID);
            }
        }

        protected override Projectile CreateNewProjectile(int ID, int ownerID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        {
            var projectile = new ServerProjectile(ID, ownerID, position, direction);

            projectile.Destroyed += OnProjectileDestroyed;
            projectile.Moved += OnProjectileMoved;

            networkManager.SendToAll(new ProjectileSpawnMessage(ID, ownerID, position, direction));

            return projectile;
        }

        private void OnProjectileDestroyed(IProjectile projectile)
        {
            networkManager.SendToAll(new ProjectileDespawnMessage(projectile.ID));
        }

        private void OnProjectileMoved(IProjectile projectile)
        {
            if (CollisionCheck(projectile, out var hitPlayer))
            {
                projectile.Destroy();
                hitPlayer.SetHP(hitPlayer.HP - ProjectileConfig.Damage);
            }
        }

        public bool CollisionCheck(IProjectile projectile, out IPlayer hitPlayer)
        {
            hitPlayer = null;
            bool foundPlayer = false;
            float targetSqrDist = PlayerConfig.Radius * PlayerConfig.Radius;

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

        ushort lastProjectileID = ushort.MaxValue;

        ushort GetNextProjectileID()
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

        public void InstantiateProjectile(int playerID, Vector3 position, Vector3 direction)
        {
            lastProjectileID = GetNextProjectileID();
            InstantiateProjectileInternal(lastProjectileID, playerID, position, direction);
        }

        public override void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            base.ApplyNetworkedMovement(playerID, input, position, rotation);
            networkManager.SendToAllExcept(playerID, new PlayerUpdateMessage(playerID, input, position, rotation));
        }
    }
}