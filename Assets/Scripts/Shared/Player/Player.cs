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
        public event Action<IPlayer> RespawnRequested;

        private readonly IInputListener _inputListener;
        protected Vector3 _lastMovementDirection { get; private set; } = Vector3.UnitZ;
        public bool Alive => HP > 0;

        public int ID { get; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        float timeSinceLastUpdate = 0f;

        public int HP { get; private set; } = 0;
        public int MaxHP => playerConfig.MaxHP;

        public bool LocalPlayer { get; private set; }

        protected readonly PlayerConfig playerConfig;
        protected readonly NetworkConfig networkConfig;
        protected readonly ProjectileConfig projectileConfig;
        protected readonly float rotationSpeedRad;

        protected Player(int id, IInputListener inputListener, bool localPlayer, PlayerConfig playerConfig, NetworkConfig networkConfig, ProjectileConfig projectileConfig)
        {
            ID = id;
            LocalPlayer = localPlayer;

            _inputListener = inputListener;
            _inputListener.OnShootRequested += ShootLocal;
            _inputListener.OnTransformUpdated += UpdateTransform;

            this.playerConfig = playerConfig;
            this.networkConfig = networkConfig;
            this.projectileConfig = projectileConfig;
            this.rotationSpeedRad = MathF.Abs(playerConfig.RotationSpeed * MathF.PI * 2f);
        }

        private void UpdateTransform(Vector3 position, Quaternion rotation)
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

        public void Update(float deltaTime)
        {
            if(Alive)
            {
                UpdateAlive(deltaTime);
            }
            else
            {
                UpdateDead(deltaTime);
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

            Vector3 movement = new Vector3(LastInput.X, 0, LastInput.Y) * (playerConfig.MovementSpeed * networkConfig.TickInterval);

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
            var t = MathF.Min(1f, angle / maxAngle);

            Rotation = Quaternion.Slerp(
                Rotation,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw),
                t
            );
        }

        protected virtual void UpdateAlive(float deltaTime)
        {
            UpdateTransform(deltaTime);
            timeSinceLastUpdate += deltaTime;
            if (LocalPlayer && timeSinceLastUpdate >= networkConfig.PlayerUpdateInterval)
            {
                UpdateRequested?.Invoke(this);
                timeSinceLastUpdate = 0;
            }
        }

        protected virtual void UpdateDead(float deltaTime)
        {
            timeSinceLastUpdate = 0;
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
            //TODO();
            Died?.Invoke(this);
        }

        public void Destroy()
        {
            _inputListener.OnShootRequested -= ShootLocal;
            _inputListener.OnTransformUpdated -= UpdateTransform;
            Destroyed?.Invoke(this);
        }

        protected void RequestRespawn()
        {
            //TODO();
            ////Looking at this code, I think I went backwards...
            ////stuff the player does really should exist inside that class. need to redo.
            RespawnRequested?.Invoke(this);
        }
        const float kEpsilon = 0.000001F;
        static float CalculateAngleMagnitudeRadians(Quaternion a, Quaternion b)
        {
            float dot = MathF.Min(MathF.Abs(Quaternion.Dot(a, b)), 1f);
            if (dot > 1.0f - kEpsilon)
                return 0f;
            return MathF.Acos(dot);
        }
    }
}
