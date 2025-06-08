using Shared.Configuration;
using Shared.Input;
using Shared.Player;
using Shared.Projectiles;
using System;
using System.Numerics;

namespace Server
{
    internal class ServerPlayer : Player
    {
        private int deadTick = 0;
        private readonly int respawnWaitTicks;

        public ServerPlayer(int id, IInputListener inputListener, bool localPlayer, PlayerConfig playerConfig, NetworkConfig networkConfig, ProjectileConfig projectileConfig)
            : base(id, inputListener, localPlayer, playerConfig, networkConfig, projectileConfig)
        {
            respawnWaitTicks = (int)(playerConfig.RespawnTime / networkConfig.TickInterval);
        }

        protected override void UpdateAlive(float deltaTime)
        {
            base.UpdateAlive(deltaTime);
            deadTick = 0;
        }

        protected override void UpdateDead(float deltaTime)
        {
            base.UpdateDead(deltaTime);
            deadTick++;

            if (deadTick >= respawnWaitTicks)
            {
                RequestRespawn();
            }

            //TODO();// just do this in gameManager?
        }

        protected override void ShootLocal()
        {
            Console.WriteLine("Error: ShootLocal called on server!");
        }

        protected override IProjectile SpawnProjectile(int ID, Vector3 position, Vector3 direction)
        {
            var projectile = new ServerProjectile(ID, this.ID, position, direction, projectileConfig, networkConfig);

            //projectile.Moved += OnProjectileMoved;

            //networkManager.SendToAll(new ProjectileSpawnMessage(ID, ownerID, position, direction));

            return projectile;
            //throw new NotImplementedException();
        }
    }
}
