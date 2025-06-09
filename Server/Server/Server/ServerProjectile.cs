using Shared.Configuration;
using Shared.Projectiles;
using System.Numerics;

namespace Server
{
    internal class ServerProjectile : Projectile
    {
        private readonly float lifetime;
        float lifetimeTimer = 0f;

        public ServerProjectile(int Id, int ownerID, Vector3 initialPosition, Vector3 direction, GameConfig gameConfig)
            : base(Id, ownerID, initialPosition, direction, gameConfig)
        {
            lifetime = gameConfig.ProjectileLifetime;
        }

        public override void Update(float deltaTime)
        {
            lifetimeTimer+= deltaTime;
            if (lifetimeTimer >= lifetime)
            {
                Destroy(false);
                //don't need to return - that's taken care of in base.Tick()
            }

            base.Update(deltaTime);
        }
    }
}
