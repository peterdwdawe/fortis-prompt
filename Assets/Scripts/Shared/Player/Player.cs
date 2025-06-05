using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Shared.Configuration;
using Shared.Input;
using Shared.Player;

namespace Shared.Player
{
    public delegate void OnShootHandler(ushort projectileID, Vector3 origin, Vector3 direction);
    public delegate void OnRequestShootHandler(int playerID, Vector3 origin, Vector3 direction);

    public class Player : IDisposable, IPlayer
    {
        public event OnRequestShootHandler OnShootRequested;
        public event Action<IPlayer> OnUpdateRequested;
        public event Action<IPlayer> HPSet;
        public event Action<IPlayer> HPReduced;
        public event Action<IPlayer> Spawned;
        public event Action<IPlayer> Died;
        public event Action<IPlayer> Destroyed;
        public event Action<IPlayer> RespawnRequested;

        private readonly IInputListener _inputListener;
        private Vector3 _lastMovementDirection;
        public bool Alive => HP > 0;

        public int ID { get; }
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        uint ticksSinceLastUpdate = 0;

        public int HP { get; private set; } = 0;
        public int MaxHP => playerConfig.MaxHP;

        public bool LocalPlayer { get; private set; }

        protected readonly PlayerConfig playerConfig;
        protected readonly NetworkConfig networkConfig;

        public Player(int id, IInputListener inputListener, bool localPlayer, PlayerConfig playerConfig, NetworkConfig networkConfig)
        {
            ID = id;

            _inputListener = inputListener;
            _inputListener.OnShootLocal += RequestShootNetworked;
            _inputListener.OnTransformUpdated += UpdateTransform;

            this.playerConfig = playerConfig;
            this.networkConfig = networkConfig;

            HP = 0;
            LocalPlayer = localPlayer;
            _lastMovementDirection = Vector3.UnitZ;
        }

        private void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        private void RequestShootNetworked()
        {
            OnShootRequested?.Invoke(ID, Position, _lastMovementDirection);
        }

        //private void HandleShootNetworked(ushort projectileID, Vector3 origin, Vector3 direction)
        //{
        //    OnShootNetworked?.Invoke(projectileID, origin, direction);
        //}

        //private void HandleShoot() => OnShoot?.Invoke(Position, _lastMovementDirection);    
        //TODO();// lastMovementDirection isnt necessarily right when networked

        public void Tick()
        {
            if(Alive)
            {
                TickAlive();
            }
            else
            {
                TickDead();
            }
        }

        public Vector2 LastInput { get; private set; }

        void UpdateTransform()
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

            Rotation = Quaternion.Slerp(
                Rotation,
                Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw),
                playerConfig.RotationSpeed
            );
        }

        protected virtual void TickAlive()
        {
            UpdateTransform();
            ticksSinceLastUpdate++;
            if (LocalPlayer && ticksSinceLastUpdate >= networkConfig.PlayerUpdateTickCount)
            {
                OnUpdateRequested?.Invoke(this);
                ticksSinceLastUpdate = 0;
            }
        }

        protected virtual void TickDead()
        {
            ticksSinceLastUpdate = 0;
        }

        public void Dispose()
        {
            //_inputListener.OnShootNetworked -= HandleShootNetworked;
            _inputListener.OnShootLocal -= RequestShootNetworked;
            _inputListener.OnTransformUpdated -= UpdateTransform;
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
            Destroyed?.Invoke(this);
        }

        protected void RequestRespawn()
        {
            RespawnRequested?.Invoke(this);
        }
    }
}
