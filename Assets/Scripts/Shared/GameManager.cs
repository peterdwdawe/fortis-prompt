using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Shared.Configuration;
using Shared.Input;
using Shared.Networking;
using Shared.Player;
using Shared.Projectiles;

namespace Shared
{
    public abstract class GameManager<TGameManager,TNetworkManager>
        where TGameManager : GameManager<TGameManager, TNetworkManager>
        where TNetworkManager : NetworkManager
    {
        protected const string gameConfigPath = "GameConfig.json";

        protected const string networkConfigPath = "NetworkConfig.json";
        protected const string playerConfigPath = "PlayerConfig.json";
        protected const string projectileConfigPath = "ProjectileConfig.json";
        //private Player.Player _player;
        //private List<Player.Player> _players = new List<Player.Player>();
        //private List<IProjectile> _projectiles = new List<IProjectile>();

        //public void InstantiatePlayer(int id, Input.IInputListener inputListener)
        //{
        //    var player = new Player.Player(id, inputListener);
        //    player.OnShootNetworked += HandleShoot;
        //    player.OnShootRequested += RequestShoot;
        //    //PlayerView view = Instantiate(Resources.Load<GameObject>("Player").GetComponentInChildren<PlayerView>());
        //    //view.Setup(_player);
        //    _players.Add(player);
        //    PlayerInstantiated?.Invoke(player);
        //}

        //protected abstract NetworkConfig LoadNetworkConfig();
        //protected abstract PlayerConfig LoadPlayerConfig();
        //protected abstract ProjectileConfig LoadProjectileConfig();
        //protected abstract GameConfig LoadGameConfig();


        protected abstract TNetworkManager GenerateNetworkManager();
        protected abstract MessageHandler<TGameManager, TNetworkManager> GenerateMessageHandler();
        public TNetworkManager networkManager { get; private set; } = null;

        protected abstract void Log(string message);

        private void RequestShoot(int playerID, Vector3 origin, Vector3 direction)
        {
            ShootRequested?.Invoke(playerID, origin, direction);
        }

        //protected void HandleShoot(ushort projectileID, System.Numerics.Vector3 position, System.Numerics.Vector3 direction)
        //{
        //    Projectile projectile = new Projectile(projectileID, position, direction);
        //    //ProjectileView projectileView = Instantiate(Resources.Load<ProjectileView>("Projectile"));
        //    //projectileView.Setup(projectile);
        //    _projectiles.Add(projectile);
        //    ProjectileInstantiated?.Invoke(projectile);
        //}

        public event Action<Projectile> ProjectileInstantiated;
        public event Action<Player.Player> PlayerInstantiated;
        public event OnRequestShootHandler ShootRequested;

        public NetworkManager.NetworkStatistics GetNetworkStatistics()
            => networkManager != null ? networkManager.GetStatistics() : NetworkManager.NetworkStatistics.Empty;

        protected bool StartNetworkingInternal()
        {
            StopNetworking();

            networkManager = GenerateNetworkManager();
            var messageHandler = GenerateMessageHandler();
            if (!networkManager.Start(messageHandler))
            {
                Log("Failed to start networking!");
                networkManager = null;
                return false;
            }
            //Log("Started networking!");
            return true;
        }

        public void StopNetworking()
        {
            if (networkManager == null)
            {
                return;
            }

            networkManager.Stop();
            networkManager = null;
            StateChanged?.Invoke(NetworkManager.ConnectionState.Uninitialized);
            //Log("Stopped networking!");
        }

        public event Action<NetworkManager.ConnectionState> StateChanged;


        public void Tick()
        {
            if (networkManager == null)
                return;

            //Log("Tick!");

            networkState.Tick();

            networkManager.PollEvents();

            if (networkManager == null) //PollEvents can make us close connection - we need to check here again to make sure we're still in-game.
                return;

            var connectionState = networkManager.CheckConnectionState(out bool stateChanged);
            if (stateChanged)
                StateChanged?.Invoke(connectionState);

            if (connectionState != NetworkManager.ConnectionState.Connected)
                return;

            foreach (var player in playerLookup.Values)
            {
                player.Tick();
            }

            foreach (var projectile in projectileLookup.Values)
            {
                projectile.Tick();
                if (projectile.Expired)
                {
                    DestroyProjectile(projectile.ID);
                }
            }
        }

        protected IEnumerable<IPlayer> AllPlayers => playerLookup.Values;
        protected IEnumerable<IProjectile> AllProjectiles => projectileLookup.Values;


        Dictionary<int, IPlayer> playerLookup;
        Dictionary<int, IProjectile> projectileLookup;
        Dictionary<int, NetworkedInputListener> networkedInputListenerLookup;

        public readonly NetworkConfig networkConfig;
        public readonly GameConfig gameConfig;
        public readonly PlayerConfig playerConfig;
        public readonly ProjectileConfig projectileConfig;
        public readonly NetworkState networkState;

        public GameManager()
        {

            //networkConfig = LoadNetworkConfig();
            //gameConfig = LoadGameConfig();
            //playerConfig = LoadPlayerConfig();
            //projectileConfig = LoadProjectileConfig();

            networkConfig = LoadConfig<NetworkConfig>(networkConfigPath);
            gameConfig = LoadConfig<GameConfig>(gameConfigPath);
            playerConfig = LoadConfig<PlayerConfig>(playerConfigPath);
            projectileConfig = LoadConfig<ProjectileConfig>(projectileConfigPath);

            networkState = new NetworkState(networkConfig);

            playerLookup = new Dictionary<int, IPlayer>(gameConfig.MaxPlayers);
            projectileLookup = new Dictionary<int, IProjectile>(gameConfig.MaxPlayers * 16);
            networkedInputListenerLookup = new Dictionary<int, NetworkedInputListener>(gameConfig.MaxPlayers);
        }

        protected bool TryGetPlayer(int ID, out IPlayer player)
            => playerLookup.TryGetValue(ID, out player) && player != null;

        protected bool TryGetProjectile(int ID, out IProjectile projectile)
             => projectileLookup.TryGetValue(ID, out projectile) && projectile != null;

        protected bool PlayerExists(int ID)
            => playerLookup.ContainsKey(ID);

        protected bool ProjectileExists(int ID)
             => projectileLookup.ContainsKey(ID);

        protected bool TryGetNetworkedInputListener(int ID, out NetworkedInputListener listener)
             => networkedInputListenerLookup.TryGetValue(ID, out listener) && listener != null;

        //private void OnProjectileInstantiated(IProjectile projectile)
        //{
        //    TODO();// Replace with InstantiateProjectile

        //    var ID = projectile.ID;
        //    if (projectileLookup.ContainsKey(ID))
        //    {
        //        Log($"OnProjectileInstantiated Error: projectile ID {ID} already exists! overwriting...");
        //        //TODO();//cleanup previous? this shouldn't be happening anyway
        //    }

        //    projectileLookup[ID] = projectile;
        //}
        //private void OnProjectileDestroyed(IProjectile projectile)
        //{
        //    TODO();// Replace with DestroyProjectile

        //    var ID = projectile.ID;
        //    if (!projectileLookup.ContainsKey(ID))
        //    {
        //        Log($"OnProjectileDestroyed Error: projectile ID {ID} not found in lookup! ignoring...");
        //        return;
        //    }

        //    projectileLookup.Remove(ID);
        //}


        public void InstantiateNetworkedPlayer(int ID)
        {
            var listener = new NetworkedInputListener(ID);

            if (networkedInputListenerLookup.ContainsKey(ID))
            {
                Log($"InstantiateNetworkedPlayer Error: NetworkedInputListener ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            networkedInputListenerLookup[ID] = listener;
            var player = InstantiatePlayerInternal(ID,listener, false);
        }

        protected abstract Player.Player CreateNewPlayer(int ID, IInputListener inputListener, bool local);

        protected Player.Player InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local)
        {
            var player = CreateNewPlayer(ID, inputListener, local);
            //player.OnShootNetworked += HandleShoot;
            player.OnShootRequested += RequestShoot;
            //PlayerView view = Instantiate(Resources.Load<GameObject>("Player").GetComponentInChildren<PlayerView>());
            //view.Setup(_player);
            //_players.Add(player);

            if (playerLookup.ContainsKey(ID))
            {
                Log($"InstantiatePlayerInternal Error: player ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            playerLookup[ID] = player;

            Log($"Player {ID} Created");
            PlayerInstantiated?.Invoke(player);

            return player;
        }
        public virtual void DestroyPlayer(int ID)
        {

            if (playerLookup.TryGetValue(ID, out var player))
            {
                //player.OnShootNetworked += HandleShoot;
                player.OnShootRequested -= RequestShoot;
                Log($"Player {ID} Removed");
                player.Destroy();
            }
            else
            {
                Log($"DestroyPlayer Error: player ID {ID} does not exist in lookup! ignoring...");
            }

            networkedInputListenerLookup.Remove(ID);
            playerLookup.Remove(ID);
        }
        public void SpawnPlayer(int ID, Vector3 position, Quaternion rotation)
        {
            if (!TryGetPlayer(ID, out var player))
            {
                Log($"Spawn Player {ID} failed: doesn't exist in lookup!");
                return;
            }
            player.Spawn(position, rotation);
        }
        public void KillPlayer(int ID)
        {
            if (TryGetPlayer(ID, out var player))
            {
                //Log($"Kill player {ID}!");
                player.Kill();
            }
            else
            {
                Log($"Failed to kill player {ID}: can't find in lookup!");
            }
        }
        public void SetPlayerHP(int ID, int HP)
        {
            if (TryGetPlayer(ID, out var player))
            {
                player.SetHP(HP);
            }
        }

        protected abstract Projectile CreateNewProjectile(int ID, int ownerID, Vector3 position, Vector3 direction);

        protected void InstantiateProjectileInternal(int ID, int ownerID, Vector3 position, Vector3 direction)
        {
            Projectile projectile = CreateNewProjectile(ID, ownerID, position, direction);

            if (projectileLookup.ContainsKey(ID))
            {
                Log($"InstantiateProjectile Error: projectile ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            projectileLookup[ID] = projectile;

            //Log($"Projectile {ID} Created");
            ProjectileInstantiated?.Invoke(projectile);
        }

        public void InstantiateProjectile(int ID, int ownerID, Vector3 position, Vector3 direction)
            => InstantiateProjectileInternal(ID, ownerID, position, direction);

        public virtual void DestroyProjectile(int ID)
        {
            if (TryGetProjectile(ID, out var projectile))
            {
                projectile.Destroy();
            }
            projectileLookup.Remove(ID);
        }
        public virtual void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            if (TryGetNetworkedInputListener(playerID, out var listener))
            {
                listener.Update(input, position, rotation);
            }
        }

        static List<IProjectile> projectileCleanupList = new List<IProjectile>();
        static List<IPlayer> playerCleanupList = new List<IPlayer>();

        public void Cleanup()
        {
            projectileCleanupList.Clear();
            projectileCleanupList.AddRange(AllProjectiles);

            playerCleanupList.Clear();
            playerCleanupList.AddRange(AllPlayers);

            foreach (var projectile in projectileCleanupList)
            {
                DestroyProjectile(projectile.ID);
            }
            foreach (var player in playerCleanupList)
            {
                DestroyPlayer(player.ID);
            }
        }


        public virtual void OnServerDisconnected() { }

        protected T LoadConfig<T>(string path) where T : class, new()
        {
            T config;
            string jsonString;
            if (File.Exists(path))
            {
                jsonString = File.ReadAllText(path);
                config = Deserialize<T>(jsonString);

                if (config != null)
                    return config;
            }

            Log($"Failed to load {typeof(T)} from disk. Generating a new {path} file.");

            config = new T();
            jsonString = Serialize(config);
            File.WriteAllText(path, jsonString);

            return config;
        }

        protected abstract T Deserialize<T>(string jsonString) where T : class;
        protected abstract string Serialize<T>(T obj) where T : new();
    }
}
