using Client.Adapters.Networking;
using Shared.Configuration;
using Shared.Networking.RPC;
using Shared.Networking.Messages;
using Shared.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Shared.Networking;
using Shared.Projectiles;
using Client.Adapters.Projectiles;

namespace Client.Adapters.Player
{
    public class ClientPlayer : Shared.Player.Player
    {
        private readonly INetworkClient netClient;

        public ClientPlayer(int id, IInputListener inputListener, bool localPlayer, PlayerConfig playerConfig, NetworkConfig networkConfig, ProjectileConfig projectileConfig, INetworkClient netClient) 
            : base(id, inputListener, localPlayer, playerConfig, networkConfig, projectileConfig)
        {
            this.netClient = netClient;
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
            var projectile = new Projectile(ID, this.ID, position, direction, projectileConfig, networkConfig);

            ProjectileView projectileView = UnityEngine.Object.Instantiate(Resources.Load<ProjectileView>("Projectile"));
            projectileView.Setup(projectile);

            if (LocalPlayer)
            {
                netClient.Send(new ProjectileSpawnMessage(ID, this.ID, position, direction));
            }

            return projectile;
        }
    }
}
