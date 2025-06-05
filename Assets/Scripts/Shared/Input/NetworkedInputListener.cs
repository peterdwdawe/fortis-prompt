using System;
using System.Numerics;
using LiteNetLib;
using Shared.Networking.Messages;
using Shared.Player;

namespace Shared.Input
{
    public class NetworkedInputListener : IInputListener
    {
        public event Action<Vector3, Quaternion> OnTransformUpdated;

        //TODO: Currently never called by design. maybe revisit and improve
        public event Action OnShootLocal;

        public Vector2 Movement { get; private set; }

        public void Update(Vector2 input, Vector3 position, Quaternion rotation)
        {
            Movement = input;
            OnTransformUpdated?.Invoke(position, rotation);
        }

        public readonly int ID;

        public NetworkedInputListener(int ID)
        {
            this.ID = ID;
        }
    }
}