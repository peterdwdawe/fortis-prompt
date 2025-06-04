using Shared.Player;
using System;
using System.Numerics;

namespace Shared.Input
{
    public interface IInputListener
    {
        event Action<Vector3, Quaternion> OnTransformUpdated;

        //TODO(); // think of a better way to do this - maybe it shouldn't be part of this interface?
        event Action OnShootLocal;
        //event OnShootHandler OnShootNetworked;

        Vector2 Movement { get; }
    }
}
