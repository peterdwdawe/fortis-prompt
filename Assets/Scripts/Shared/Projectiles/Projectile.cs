using Shared.Configuration;
using System;
using System.Numerics;

namespace Shared.Projectiles
{
    public class Projectile : IProjectile
    {
        public event Action<IProjectile> Destroyed;
        public event Action<IProjectile> Moved;

        private readonly Vector3 _direction;
        public Vector3 Direction => _direction;
        public Vector3 Position { get; private set; }
        public bool Expired { get; protected set; }
        public int ID { get; private set; }
        public int ownerID { get; private set; }
        private readonly ProjectileConfig projectileConfig;
        private readonly NetworkConfig networkConfig;

        public Projectile(int Id, int ownerID, Vector3 initialPosition, Vector3 direction, ProjectileConfig projectileConfig, NetworkConfig networkConfig)
        {
            this.ID = Id;
            this.ownerID = ownerID;

            Position = initialPosition;
            _direction = Vector3.Normalize(direction);

            this.projectileConfig = projectileConfig;
            this.networkConfig = networkConfig;
        }

        public virtual void Tick()
        {
            if(Expired) 
                return;

            Position += _direction * (projectileConfig.MovementSpeed * networkConfig.TickInterval);

            Moved?.Invoke(this);
        }

        public virtual void Destroy() 
        {
            Expired = true;
            Destroyed?.Invoke(this);
        }
    }
}