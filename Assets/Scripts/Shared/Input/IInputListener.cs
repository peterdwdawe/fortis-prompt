using System;
using System.Numerics;

namespace Shared.Input
{
    public interface IInputListener
    {
        event Action<Vector3, Quaternion> OnTransformUpdated;
        event Action OnShootRequested;

        Vector2 Movement { get; }
    }
}
