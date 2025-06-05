using Shared.Configuration;
using Shared.Projectiles;
using System.Numerics;

namespace Server
{
    internal class ServerProjectile : Projectile
    {
        private readonly int endTick;
        int tick = 0;

        public ServerProjectile(int Id, int ownerID, Vector3 initialPosition, Vector3 direction, ProjectileConfig projectileConfig, NetworkConfig networkConfig)
            : base(Id, ownerID, initialPosition, direction, projectileConfig, networkConfig)
        {
            endTick = (int)(projectileConfig.Duration / networkConfig.TickInterval);
        }

        public override void Tick()
        {
            tick++;
            if (tick >= endTick)
            {
                Expired = true;
                //don't need to return - that's taken care of in base.Tick()
            }

            base.Tick();
        }
    }
}
