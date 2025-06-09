using Client.Adapters.Character;
using Client.Input;
using Client.Adapters.Networking;
using Shared;
using Shared.Input;
using Shared.Networking.Messages;
using Shared.Player;
using UnityEngine;
using Shared.Networking;
using Client.Adapters.Player;
using Shared.Configuration;

namespace Client
{
    public class ClientGameManager : GameManager<INetworkClient>
    {
        private readonly LocalInputListener _localInputListener;

        protected override INetworkClient GenerateNetworkManager()
        {
            var manager = new ClientNetworkManager(clientConfig.RpcTimeout,clientConfig.NetworkKey);

            manager.GameConfigReceived += OnGameConfigReceived;
            manager.PeerDisconnected += OnServerDisconnected;
            manager.NetworkStopped += Cleanup;

            manager.PlayerRegistered += OnPlayerRegistrationReceived;
            manager.PlayerHPUpdated += OnPlayerHPUpdateReceived;
            manager.PlayerSpawned += OnPlayerSpawnReceived;
            manager.PlayerUpdateReceived += OnPlayerUpdateReceived;
            manager.PlayerDied += OnPlayerDeathReceived;
            manager.PlayerDeregistered += OnPlayerDeregistrationReceived;

            manager.ProjectileSpawned += OnProjectileSpawnReceived;
            manager.ProjectileDespawned += OnProjectileDespawnReceived;

            return manager;
        }

        private void OnGameConfigReceived(GameConfigurationMessage message)
        {
            gameConfig = new GameConfig(message.configData);
        }

        protected const string clientConfigPath = "ClientConfig.json";

        public ClientConfig clientConfig { get; private set; }
        public GameConfig gameConfig { get; private set; } = new GameConfig(new GameConfigData());

        protected override void GetConfigData()
        {
            clientConfig = new ClientConfig(LoadConfig<ClientConfigData>(clientConfigPath));
        }

        #region Network Message Handling

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


        public ClientGameManager(LocalInputListener localInputListener) : base()
        {
            _localInputListener = localInputListener;
        }

        private void InstantiateLocalPlayer(int ID)
        {
            InstantiatePlayer(ID, _localInputListener, true);
        }

        public event System.Action<string> MessageLogged;

        protected override void Log(string message)
        {
            Debug.Log(message);
            MessageLogged?.Invoke(message);
        }

        protected override Player InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local)
        {
            var player = new ClientPlayer(ID, inputListener, local,gameConfig, clientConfig, networkManager);

            var playerViewRoot = Object.Instantiate(Resources.Load<GameObject>("Player"));
            PlayerView view = playerViewRoot.GetComponentInChildren<PlayerView>();
            view.Setup(player, playerViewRoot);
            return player;
        }

        public void ConnectToServer(string serverAddress, int serverPort)
        {
            if(TryStartNetworkingInternal(0))
                networkManager.ConnectToServer(serverAddress, serverPort);
        }
    }
}
