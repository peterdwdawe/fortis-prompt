using System;
using System.Numerics;

namespace Shared.Projectiles
{
    public interface IProjectile
    {
        event Action<IProjectile> Destroyed;
        event Action<IProjectile> Moved;

        Vector3 Position { get; }
        int ID { get; }
        bool Expired { get; }
        int ownerID { get; }
        Vector3 Direction { get; }

        void Update(float deltaTime);
        void Destroy(bool immediate);
    }
}
