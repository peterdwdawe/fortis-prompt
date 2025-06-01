using System;
using System.Numerics;

namespace Core.Player
{
    public interface IPlayer : IDisposable
    {
        event OnShootHandler OnShoot;

        Vector3 Position { get; }
        Quaternion Rotation { get; }

        void Tick();
    }
}
