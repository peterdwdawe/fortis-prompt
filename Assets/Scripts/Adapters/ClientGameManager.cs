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
    public class ClientGameManager : GameManager<ClientNetworkManager>
    {
        private readonly LocalInputListener _localInputListener;

        protected override ClientNetworkManager GenerateNetworkManager()
        {
            var manager = new ClientNetworkManager(networkConfig, serverPort, serverAddress);

            return manager;
        }

        #region Network Message Handling

        protected override IMessageHandler GenerateMessageHandler()
        {
            var handler = new MessageEventHandler();

            handler.PeerDisconnected += OnServerDisconnected;
            handler.NetworkStopped += Cleanup;

            handler.PlayerRegistered += OnPlayerRegistrationReceived;
            handler.PlayerHPUpdated += OnPlayerHPUpdateReceived;
            handler.PlayerSpawned += OnPlayerSpawnReceived;
            handler.PlayerUpdateReceived += OnPlayerUpdateReceived;
            handler.PlayerDied += OnPlayerDeathReceived;
            handler.PlayerDeregistered += OnPlayerDeregistrationReceived;

            handler.ProjectileSpawned += OnProjectileSpawnReceived;
            handler.ProjectileDespawned += OnProjectileDespawnReceived;

            return handler;
        }

        void OnServerDisconnected(int peerID)
        {
            Log("Lost connection to server.");
            StopNetworking();
        }

        private void OnPlayerRegistrationReceived(PlayerRegistrationMessage message)
        {
            if (message.localPlayer)
                InstantiateLocalPlayer(message.playerID);
            else
                InstantiateNetworkedPlayer(message.playerID);
        }

        private void OnPlayerHPUpdateReceived(PlayerHPUpdateMessage message)
        {
            if (TryGetPlayer(message.playerID, out var player))
            {
                player.SetHP(message.hp);
            }
        }

        private void OnPlayerSpawnReceived(PlayerSpawnMessage message)
        {
            if (!TryGetPlayer(message.playerID, out var player))
            {
                Log($"Spawn Player {message.playerID} failed: doesn't exist in lookup!");
                return;
            }
            player.Spawn(message.position, message.rotation);
        }

        private void OnPlayerDeathReceived(PlayerDeathMessage message)
        {
            if (TryGetPlayer(message.playerID, out var player))
            {
                //Log($"Kill player {ID}!");
                player.Kill();
            }
            else
            {
                Log($"Failed to kill player {message.playerID}: can't find in lookup!");
            }
        }

        protected void OnPlayerDeregistrationReceived(PlayerDeregistrationMessage message)
            => OnPlayerDisconnected(message.playerID);

        private void OnProjectileSpawnReceived(ProjectileSpawnMessage message)
            => InstantiateProjectile(message.projectileID, message.ownerID, message.position, message.direction);

        private void OnProjectileDespawnReceived(ProjectileDespawnMessage message)
        {
            if (TryGetProjectile(message.projectileID, out var projectile))
            {
                projectile.Destroy();
            }
            else
            {
                Log($"Failed to despawn projectile {message.projectileID}: can't find in lookup!");
            }
        }

        #endregion

        private string serverAddress;
        private int serverPort;

        public ClientGameManager(LocalInputListener localInputListener) : base()
        {
            _localInputListener = localInputListener;
        }

        private void InstantiateLocalPlayer(int ID)
        {
            InstantiatePlayerInternal(ID, _localInputListener, true);
        }

        protected override void OnPlayerDestroyed(IPlayer player)
        {
            player.OnUpdateRequested -= SendPlayerUpdate;
            player.OnShootRequested -= OnShootRequested;
            base.OnPlayerDestroyed(player);
        }

        private void SendPlayerUpdate(IPlayer player)
        {
            networkManager.SendToServer(new PlayerUpdateMessage(player.ID, player.LastInput, player.Position, player.Rotation));
        }

        private void OnShootRequested(int playerID, System.Numerics.Vector3 origin, System.Numerics.Vector3 direction)
        {
            networkManager.SendToServer(new RequestProjectileSpawnMessage(playerID, origin, direction));
        }

        public event System.Action<string> MessageLogged;

        protected override void Log(string message)
        {
            Debug.Log(message);
            MessageLogged?.Invoke(message);
        }

        protected override Player CreateNewPlayer(int ID, IInputListener inputListener, bool local)
        {
            var player = new Player(ID, inputListener, local, playerConfig, networkConfig);

            player.OnUpdateRequested += SendPlayerUpdate;
            player.OnShootRequested += OnShootRequested;

            var playerViewRoot = Object.Instantiate(Resources.Load<GameObject>("Player"));
            PlayerView view = playerViewRoot.GetComponentInChildren<PlayerView>();
            view.Setup(player, playerViewRoot);
            return player;
        }

        protected override Projectile InstantiateProjectileInternal(int ID, int ownerID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        {
            var projectile =  new Projectile(ID, ownerID, position, direction, projectileConfig, networkConfig);

            ProjectileView projectileView = Object.Instantiate(Resources.Load<ProjectileView>("Projectile"));
            projectileView.Setup(projectile);
            return projectile;
        }

        void SetServerInfo(string serverAddress, int serverPort)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
        }

        public void ConnectToServer(string serverAddress, int serverPort)
        {
            SetServerInfo(serverAddress, serverPort);
            StartNetworkingInternal();
        }

        protected override T Deserialize<T>(string jsonString) where T : class
        {
            return JsonUtility.FromJson<T>(jsonString);
        }

        protected override string Serialize<T>(T obj)
        {
            return JsonUtility.ToJson(obj);
        }
    }
}
