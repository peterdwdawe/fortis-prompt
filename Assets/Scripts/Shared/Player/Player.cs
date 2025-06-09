using System;
using System.Numerics;
using Shared.Configuration;
using Shared.Input;
using Shared.Projectiles;

namespace Shared.Player
{
    public delegate void OnShootHandler(IPlayer player, IProjectile projectile);

    public abstract class Player : IDisposable, IPlayer
    {
        public event OnShootHandler ShotProjectile;
        public event Action<IPlayer> UpdateRequested;
        public event Action<IPlayer> HPSet;
        public event Action<IPlayer> HPReduced;
        public event Action<IPlayer> Spawned;
        public event Action<IPlayer> Died;
        public event Action<IPlayer> Destroyed;

        private readonly IInputListener _inputListener;
        protected Vector3 _lastMovementDirection { get; private set; } = Vector3.UnitZ;
        public bool Alive => HP > 0;

        public int ID { get; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        public int HP { get; private set; } = 0;
        public int MaxHP => gameConfig.PlayerMaxHP;

        public bool LocalPlayer { get; private set; }

        public float Diameter => gameConfig.PlayerRadius * 2f;

        protected readonly GameConfig gameConfig;
        protected readonly float rotationSpeedRad;

        protected Player(int id, IInputListener inputListener, bool localPlayer, GameConfig gameConfig)
        {
            ID = id;
            LocalPlayer = localPlayer;

            _inputListener = inputListener;
            _inputListener.OnShootRequested += ShootLocal;
            _inputListener.OnTransformUpdated += UpdateTransform;

            this.gameConfig = gameConfig;
            rotationSpeedRad = MathF.Abs(gameConfig.PlayerRotateSpeed * MathF.PI * 2f);
        }

        protected void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        protected abstract void ShootLocal();

        public IProjectile Shoot(int projectileID, Vector3 position, Vector3 direction)
        {
            var projectile = SpawnProjectile(projectileID, position, direction);

            ShotProjectile?.Invoke(this, projectile);
            return projectile;
            //OnShootRequested?.Invoke(ID, Position, _lastMovementDirection);
        }

        protected abstract IProjectile SpawnProjectile(int ID, Vector3 position, Vector3 direction);

        public virtual void Update(float deltaTime)
        {
            if (Alive)
            {
                UpdateTransform(deltaTime);
            }
        }

        public Vector2 LastInput { get; private set; }

        void UpdateTransform(float deltaTime)
        {
            LastInput = _inputListener.Movement;

            if (LastInput == Vector2.Zero)
            {
                return;
            }

            LastInput = Vector2.Normalize(LastInput);

            Vector3 movement = new Vector3(LastInput.X, 0, LastInput.Y) * (gameConfig.PlayerMoveSpeed * deltaTime);

            if (movement == Vector3.Zero)
            {
                return;
            }

            Position += movement;

            _lastMovementDirection = movement;

            float yaw = MathF.Atan2(LastInput.X, LastInput.Y);

            Quaternion target = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw);

            float maxAngle = rotationSpeedRad * deltaTime;
            var angle = CalculateAngleMagnitudeRadians(Rotation, target);
            var t = MathF.Min(1f, maxAngle / angle);

            Rotation = Quaternion.Slerp(
                Rotation,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw),
                t
            );
        }

        public void Dispose()
        {
            Console.WriteLine($"Dispose Player {ID}");
        }

        public void SetHP(int HP)
        {
            bool reduced = HP < this.HP;

            this.HP = HP;

            HPSet?.Invoke(this);

            if (reduced)
                HPReduced?.Invoke(this);
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            UpdateTransform(position, rotation);
            Spawned?.Invoke(this);
        }

        public void Kill()
        {
            Died?.Invoke(this);
        }

        public void Destroy()
        {
            _inputListener.OnShootRequested -= ShootLocal;
            _inputListener.OnTransformUpdated -= UpdateTransform;
            Destroyed?.Invoke(this);
        }

        const float kEpsilon = 0.000001F;

        static float CalculateAngleMagnitudeRadians(Quaternion a, Quaternion b)
        {
            float dot = MathF.Min(MathF.Abs(Quaternion.Dot(a, b)), 1f);

            if (dot > 1.0f - kEpsilon)
                return 0;

            return MathF.Acos(dot) * 2f;
        }
    }
}
