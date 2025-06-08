using Shared.Configuration;
using Shared.Projectiles;
using System.Numerics;

namespace Server
{
    internal class ServerProjectile : Projectile
    {
        private readonly float lifetime;
        float lifetimeTimer = 0f;

        public ServerProjectile(int Id, int ownerID, Vector3 initialPosition, Vector3 direction, ProjectileConfig projectileConfig, NetworkConfig networkConfig)
            : base(Id, ownerID, initialPosition, direction, projectileConfig, networkConfig)
        {
            lifetime = projectileConfig.Duration;
        }

        public override void Update(float deltaTime)
        {
            lifetimeTimer+= deltaTime;
            if (lifetimeTimer >= lifetime)
            {
                Expired = true;
                //don't need to return - that's taken care of in base.Tick()
            }

            base.Update(deltaTime);
        }
    }
}
