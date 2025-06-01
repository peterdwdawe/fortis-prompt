using System;
using System.Numerics;

namespace Core.Projectiles
{
    public class Projectile : IProjectile
    {
        public event Action OnExpire;

        private const float MovementSpeed = 8f;
        private const float Duration = 4f;

        private readonly Vector3 _direction;
        private readonly float endTime;

        public Vector3 Position { get; private set; }
        public bool Expired { get; private set; }

        public Projectile(Vector3 initialPosition, Vector3 direction)
        {
            Position = initialPosition;
            _direction = Vector3.Normalize(direction);
            endTime = NetworkState.CurrentTime + Duration;
        }

        public void Tick()
        {
            if (NetworkState.CurrentTime > endTime)
            {
                Expired = true;
                OnExpire?.Invoke();
                return;
            }

            Position += _direction * (MovementSpeed * NetworkConfig.TickInterval);
        }
    }
}
