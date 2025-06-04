using Adapters.Character;
using Adapters.Input;
using Adapters.Networking;
using Adapters.Projectiles;
using Shared;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Player;
using Shared.Projectiles;
using UnityEngine;

namespace Adapters
{
    public class ClientGameManager : GameManager<ClientGameManager, ClientNetworkManager>
    {
        private readonly LocalInputListener _localInputListener;


        protected override ClientNetworkManager GenerateNetworkManager()
        {
            return new ClientNetworkManager(NetworkConfig.Port, NetworkConfig.TickInterval);
        }

        protected override MessageHandler<ClientGameManager, ClientNetworkManager> GenerateMessageHandler()
        {
            return new ClientMessageHandler(this);
        }

        public ClientGameManager(LocalInputListener localInputListener) : base()
        {
            _localInputListener = localInputListener; 
            PlayerInstantiated += OnPlayerInstantiated;
            ShootRequested += OnShootRequested;
            ProjectileInstantiated += OnProjectileInstantiated;
        }

        public void InstantiateLocalPlayer(int ID)
        {
            InstantiatePlayerInternal(ID, _localInputListener, true);
        }

        private void OnPlayerInstantiated(Player player)
        {
            player.OnUpdateRequested += SendPlayerUpdate;

            var playerViewRoot = Object.Instantiate(Resources.Load<GameObject>("Player"));
            PlayerView view = playerViewRoot.GetComponentInChildren<PlayerView>();
            view.Setup(player, playerViewRoot);
        }
        public override void DestroyPlayer(int ID)
        {
            if (TryGetPlayer(ID, out var player))
            {
                player.OnUpdateRequested -= SendPlayerUpdate;
            }

            base.DestroyPlayer(ID);
        }

        private void SendPlayerUpdate(IPlayer player)
        {
            networkManager.SendToServer(new PlayerUpdateMessage(player.ID, player.LastInput, player.Position, player.Rotation));
        }

        private void OnShootRequested(int playerID, System.Numerics.Vector3 origin, System.Numerics.Vector3 direction)
        {
            networkManager.SendToServer(new RequestProjectileSpawnMessage(playerID, origin, direction));
        }

        private void OnProjectileInstantiated(Projectile projectile)
        {
            ProjectileView projectileView = Object.Instantiate(Resources.Load<ProjectileView>("Projectile"));
            projectileView.Setup(projectile);
        }

        public event System.Action<string> MessageLogged;

        protected override void Log(string message)
        {
            Debug.Log(message);
            MessageLogged?.Invoke(message);
        }

        protected override Player CreateNewPlayer(int ID, IInputListener inputListener, bool local)
        {
            return new Player(ID, inputListener, local);
        }

        protected override Projectile CreateNewProjectile(int ID, int ownerID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        {
            return new Projectile(ID, ownerID, position, direction);
        }
    }
}
