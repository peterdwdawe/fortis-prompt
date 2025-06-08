using Client.Adapters.Character;
using Client.Input;
using Client.Adapters.Networking;
using Client.Adapters.Projectiles;
using Shared;
using Shared.Input;
using Shared.Networking.Messages;
using Shared.Player;
using Shared.Projectiles;
using UnityEngine;
using Shared.Networking;
using Shared.Networking.RPC;
using Client.Adapters.Player;

namespace Client
{
    public class ClientGameManager : GameManager<INetworkClient>
    {
        private readonly LocalInputListener _localInputListener;

        protected override INetworkClient GenerateNetworkManager()
        {
            var manager = new ClientNetworkManager(networkConfig);

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

        #region Network Message Handling

        //protected override IMessageHandler GenerateMessageHandler()
        //{
        //    var handler = new MessageEventHandler();

        //    handler.PeerDisconnected += OnServerDisconnected;
        //    handler.NetworkStopped += Cleanup;

        //    handler.PlayerRegistered += OnPlayerRegistrationReceived;
        //    handler.PlayerHPUpdated += OnPlayerHPUpdateReceived;
        //    handler.PlayerSpawned += OnPlayerSpawnReceived;
        //    handler.PlayerUpdateReceived += OnPlayerUpdateReceived;
        //    handler.PlayerDied += OnPlayerDeathReceived;
        //    handler.PlayerDeregistered += OnPlayerDeregistrationReceived;

        //    handler.ProjectileSpawned += OnProjectileSpawnReceived;
        //    handler.ProjectileDespawned += OnProjectileDespawnReceived;

        //    return handler;
        //}

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

        //private void OnProjectileSpawnReceived(ProjectileSpawnMessage message)
        //    => InstantiateProjectile(message.projectileID, message.ownerID, message.position, message.direction);

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

        //private string serverAddress;
        //private int serverPort;

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
            player.UpdateRequested -= SendPlayerUpdate;
            base.OnPlayerDestroyed(player);
        }

        private void SendPlayerUpdate(IPlayer player)
        {
            networkManager.Send(new PlayerUpdateMessage(player.ID, player.LastInput, player.Position, player.Rotation));
        }

        public event System.Action<string> MessageLogged;

        protected override void Log(string message)
        {
            Debug.Log(message);
            MessageLogged?.Invoke(message);
        }

        protected override Player CreateNewPlayer(int ID, IInputListener inputListener, bool local)
        {
            var player = new ClientPlayer(ID, inputListener, local, playerConfig, networkConfig, projectileConfig, networkManager);

            player.UpdateRequested += SendPlayerUpdate;

            var playerViewRoot = Object.Instantiate(Resources.Load<GameObject>("Player"));
            PlayerView view = playerViewRoot.GetComponentInChildren<PlayerView>();
            view.Setup(player, playerViewRoot);
            return player;
        }

        //protected override IProjectile InstantiateProjectileInternal(int ID, int ownerID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        //{
        //    var projectile =  new Projectile(ID, ownerID, position, direction, projectileConfig, networkConfig);

        //    ProjectileView projectileView = Object.Instantiate(Resources.Load<ProjectileView>("Projectile"));
        //    projectileView.Setup(projectile);
        //    return projectile;
        //}

        public void ConnectToServer(string serverAddress, int serverPort)
        {
            if(TryStartNetworkingInternal(0))
                networkManager.ConnectToServer(serverAddress, serverPort);
        }

        //protected override T Deserialize<T>(string jsonString) where T : class
        //{
        //    return JsonUtility.FromJson<T>(jsonString);
        //}

        //protected override string Serialize<T>(T obj)
        //{
        //    return JsonUtility.ToJson(obj);
        //}
    }
}
