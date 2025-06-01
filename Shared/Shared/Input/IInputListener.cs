using System;
using System.Numerics;

namespace Core.Input
{
    public interface IInputListener
    {
        event Action OnShoot;

        Vector2 Movement { get; }
    }
}
