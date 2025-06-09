using System;
using System.Numerics;

namespace Shared.Input
{
    public class NetworkedInputListener : IInputListener
    {
        public event Action<Vector3, Quaternion> OnTransformUpdated;

        //Currently never called by design. maybe revisit and improve
        public event Action OnShootRequested;

        public Vector2 Movement { get; private set; }

        public void Update(Vector2 input, Vector3 position, Quaternion rotation)
        {
            Movement = input;
            OnTransformUpdated?.Invoke(position, rotation);
        }
    }
}