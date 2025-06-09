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
        public bool Expired { get; private set; } = false;
        public int ID { get; private set; }
        public int ownerID { get; private set; }
        private readonly GameConfig gameConfig;

        public Projectile(int ID, int ownerID, Vector3 initialPosition, Vector3 direction, GameConfig gameConfig)
        {
            this.ID = ID;
            this.ownerID = ownerID;

            Position = initialPosition;
            _direction = Vector3.Normalize(direction);

            this.gameConfig = gameConfig;
        }

        public virtual void Update(float deltaTime)
        {
            if(Expired) 
                return;

            Position += _direction * (gameConfig.ProjectileSpeed * deltaTime);

            Moved?.Invoke(this);
        }

        public virtual void Destroy(bool immediate) 
        {
            Expired = true;
            if(immediate)
                Destroyed?.Invoke(this);
        }
    }
}