using System;
using System.Numerics;

namespace Shared.Projectiles
{
    public class Projectile : IProjectile
    {
        public event Action<IProjectile> Destroyed;
        public event Action<IProjectile> Moved;

        private const float MovementSpeed = 8f;
        private readonly Vector3 _direction;
        public Vector3 Direction => _direction;
        public Vector3 Position { get; private set; }
        public bool Expired { get; protected set; }
        public int ID { get; private set; }
        public int ownerID { get; private set; }

        public Projectile(int Id, int ownerID, Vector3 initialPosition, Vector3 direction)
        {
            this.ID = Id;
            this.ownerID = ownerID;
            Position = initialPosition;
            _direction = Vector3.Normalize(direction);
        }

        public virtual void Tick()
        {
            if(Expired) 
                return;

            Position += _direction * (MovementSpeed * NetworkConfig.TickInterval);

            Moved?.Invoke(this);
        }

        public virtual void Destroy() 
        {
            Expired = true;
            Destroyed?.Invoke(this);
        }
    }
}
