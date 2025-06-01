using System;
using System.Numerics;
using Core.Input;

namespace Core.Player
{
    public delegate void OnShootHandler(Vector3 origin, Vector3 direction);

    public class Player : IDisposable, IPlayer
    {
        public event OnShootHandler OnShoot;

        private readonly IInputListener _inputListener;
        private Vector3 _lastMovementDirection;

        public string Id { get; }
        public Vector3 Position { get; private set; } = new Vector3(0, 1, 0);
        public Quaternion Rotation { get; private set; }

        public Player(string id, IInputListener inputListener)
        {
            Id = id;
            _inputListener = inputListener;
            _inputListener.OnShoot += HandleShoot;
        }

        private void HandleShoot() => OnShoot?.Invoke(Position, _lastMovementDirection);

        public void Tick()
        {
            Vector2 input = _inputListener.Movement;

            if (input == Vector2.Zero)
            {
                return;
            }

            input = Vector2.Normalize(input);
            Vector3 movement = new Vector3(input.X, 0, input.Y) * (PlayerConfig.MovementSpeed * NetworkConfig.TickInterval);

            if (movement == Vector3.Zero)
            {
                return;
            }

            Position += movement;

            _lastMovementDirection = movement;

            float yaw = MathF.Atan2(input.X,input.Y);

            Rotation = Quaternion.Slerp(
                Rotation,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw),
                PlayerConfig.RotationSpeed
            );
        }

        public void Dispose()
        {
            _inputListener.OnShoot -= HandleShoot;
        }
    }
}
