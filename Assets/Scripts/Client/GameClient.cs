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
using Client.Configuration;

namespace Client
{
    public class GameClient : GameManager
    {
        private readonly LocalInputListener _localInputListener;

        public GameClient(LocalInputListener localInputListener) : base()
        {
            _localInputListener = localInputListener;
        }

        private INetworkClient _client;

        protected override INetworkManager GenerateNetworkManager()
        {
            var client = new NetworkClient(clientConfig.RpcTimeout,clientConfig.NetworkKey);

            client.GameConfigReceived += OnGameConfigReceived;
            client.PeerDisconnected += OnServerDisconnected;
            client.NetworkStopped += Cleanup;

            client.PlayerRegistered += OnPlayerRegistrationReceived;
            client.PlayerHPUpdated += OnPlayerHPUpdateReceived;
            client.PlayerSpawned += OnPlayerSpawnReceived;
            client.PlayerUpdateReceived += OnPlayerUpdateReceived;
            client.PlayerDied += OnPlayerDeathReceived;
            client.PlayerDeregistered += OnPlayerDeregistrationReceived;

            client.ProjectileSpawned += OnProjectileSpawnReceived;
            client.ProjectileDespawned += OnProjectileDespawnReceived;

            _client = client;
            return client;
        }

        protected const string clientConfigPath = "ClientConfig.json";

        ClientConfig clientConfig;
        GameConfig gameConfig;

        protected override void GetConfigData()
        {
            clientConfig = new ClientConfig(LoadConfig<ClientConfigData>(clientConfigPath));
        }

        public void ConnectToServer(string serverAddress, int serverPort)
        {
            if (TryStartNetworkingInternal(0))
                _client.ConnectToServer(serverAddress, serverPort);
        }

        public event System.Action<string> MessageLogged;

        protected override void Log(string message)
        {
            Debug.Log(message);
            MessageLogged?.Invoke(message);
        }

        #region Player Utils

        protected override Player InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local)
        {
            var player = new ClientPlayer(ID, inputListener, local,gameConfig, clientConfig, _client);

            var playerViewRoot = Object.Instantiate(Resources.Load<GameObject>("Player"));
            PlayerView view = playerViewRoot.GetComponentInChildren<PlayerView>();
            view.Setup(player, playerViewRoot);
            return player;
        }

        private void InstantiateLocalPlayer(int ID)
            => InstantiatePlayer(ID, _localInputListener, true);

        #endregion

        #region Network Message Handling

        void OnServerDisconnected(int peerID)
        {
            Log("Lost connection to server.");
            StopNetworking();
        }

        private void OnGameConfigReceived(GameConfigurationMessage message) 
            => gameConfig = new GameConfig(message.configData);

        private void OnPlayerRegistrationReceived(PlayerRegistrationMessage message)
        {
            if (message.localPlayer)
                InstantiateLocalPlayer(message.playerID);
            else
                InstantiateNetworkedPlayer(message.playerID);
        }

        private void OnPlayerHPUpdateReceived(PlayerHPUpdateMessage message)
        {
            if (!TryGetPlayer(message.playerID, out var player))
            {
                Log($"Update Player {message.playerID} HP failed: doesn't exist in lookup!");
                return;
            }

            player.SetHP(message.hp);
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
            if (!TryGetPlayer(message.playerID, out var player))
            {
                Log($"Failed to kill player {message.playerID}: can't find in lookup!");
                return;
            }

            //Log($"Kill player {ID}!");
            player.Kill();
        }

        protected void OnPlayerDeregistrationReceived(PlayerDeregistrationMessage message)
            => OnPlayerDisconnected(message.playerID);

        private void OnProjectileDespawnReceived(ProjectileDespawnMessage message)
        {
            if (!TryGetProjectile(message.projectileID, out var projectile))
            {
                Log($"Failed to despawn projectile {message.projectileID}: can't find in lookup!");
                return;
            }

            projectile.Destroy(true);
        }

        #endregion
    }
}
