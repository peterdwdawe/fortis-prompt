using Server.Projectiles;
using Shared.Configuration;
using Shared.Input;
using Shared.Projectiles;
using System;
using System.Numerics;

namespace Server.Player
{
    internal class ServerPlayer : Shared.Player.Player
    {
        private float deadTimer = 0;
        private readonly float respawnWaitTime;

        public ServerPlayer(int id, IInputListener inputListener, bool localPlayer, GameConfig gameConfig)
            : base(id, inputListener, localPlayer, gameConfig)
        {
            respawnWaitTime = gameConfig.PlayerRespawnTime;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (Alive)
            {
                deadTimer = 0f;
            }
            else
            {
                deadTimer += deltaTime;

                if (deadTimer >= respawnWaitTime)
                    SpawnAtRandomLocation();
            }
        }

        public void SpawnAtRandomLocation()
        {
            SetHP(gameConfig.PlayerMaxHP);
            Spawn(GetRandomSpawnPosition(), Quaternion.Identity);
        }

        Random random = new Random();

        private Vector3 GetRandomSpawnPosition()
        {
            return new Vector3
                ((float)(random.NextDouble() * 8f) - 4f,
                1f,
                 (float)(random.NextDouble() * 8f) - 4f);
        }

        protected override void ShootLocal() 
            => Console.WriteLine("Error: ShootLocal called on server!");

        protected override IProjectile SpawnProjectile(int ID, Vector3 position, Vector3 direction) 
            => new ServerProjectile(ID, this.ID, position, direction, gameConfig);
    }
}
