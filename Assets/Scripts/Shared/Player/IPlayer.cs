using System;
using System.Numerics;

namespace Shared.Player
{
    public interface IPlayer : IDisposable
    {
        event OnShootHandler ShotProjectile;
        event Action<IPlayer> HPSet;
        event Action<IPlayer> Spawned;
        event Action<IPlayer> Died;
        event Action<IPlayer> Destroyed;
        event Action<IPlayer> HPReduced;

        Vector3 Position { get; }
        Quaternion Rotation { get; }
        int ID { get; }
        int HP { get; }
        int MaxHP { get; }
        float Diameter { get; }
        bool Alive { get; }
        bool LocalPlayer { get; }
        Vector2 LastInput { get; }

        void SetHP(int HP);

        void Update(float deltaTime);

        void Spawn(Vector3 position, Quaternion rotation);
        void Kill();
        void Destroy();
        void Shoot(int projectileID, Vector3 position, Vector3 direction);
    }
}
