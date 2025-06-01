using System;
using System.Numerics;

namespace Core.Projectiles
{
    public interface IProjectile
    {
        event Action OnExpire;
        Vector3 Position { get; }
        bool Expired { get; }
        void Tick();
    }
}
