using Shared.Configuration;
using Shared.Networking.RPC;
using Shared.Networking.Messages;
using Shared.Input;
using UnityEngine;
using Shared.Networking;
using Shared.Projectiles;
using Client.Adapters.Projectiles;
using Client.Configuration;

namespace Client.Adapters.Player
{
    public class ClientPlayer : Shared.Player.Player
    {
        private readonly INetworkClient netClient;
        private readonly ClientConfig clientConfig;

        float timeSinceLastUpdate = 0f;

        public ClientPlayer(int id, IInputListener inputListener, bool localPlayer, 
            GameConfig gameConfig, ClientConfig clientConfig, INetworkClient netClient) 
            : base(id, inputListener, localPlayer, gameConfig)
        {
            this.netClient = netClient;
            this.clientConfig = clientConfig;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!LocalPlayer)
                return;

            if (Alive)
            {
                timeSinceLastUpdate += deltaTime;
                if (timeSinceLastUpdate >= clientConfig.PlayerUpdateInterval)
                {
                    netClient.Send(new PlayerUpdateMessage(ID, LastInput, Position, Rotation), LiteNetLib.DeliveryMethod.Sequenced);
                    timeSinceLastUpdate = 0;
                }
            }
            else
            {
                timeSinceLastUpdate = 0;
            }
        }

        protected override void ShootLocal() 
            => ShootLocalRpc();


        private void ShootLocalRpc()
        {
            //Debug.LogWarning("ShootLocalRpc!");
            var response = netClient.SendRpcRequest(new SpawnProjectileRpcRequestMessage(ID, Position, _lastMovementDirection));
            if (response.approved)
            {
                if (response.ownerID == ID)
                {
                    Shoot(response.projectileID, response.position, response.direction);
                }
                else
                {
                    Debug.LogWarning("ShootLocalRpc response had incorrect ownerID!");
                }
            }
            else
            {
                Debug.LogWarning("ShootLocalRpc request denied!");
            }
        }

        protected override IProjectile SpawnProjectile(int ID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        {
            var projectile = new Projectile(ID, this.ID, position, direction, gameConfig);

            ProjectileView projectileView = Object.Instantiate(Resources.Load<ProjectileView>("Projectile"));

            projectileView.Setup(projectile);

            if (LocalPlayer)
                netClient.Send(new ProjectileSpawnMessage(ID, this.ID, position, direction));

            return projectile;
        }
    }
}
