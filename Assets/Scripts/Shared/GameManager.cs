using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Shared.Configuration;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Player;
using Shared.Projectiles;

namespace Shared
{
    public abstract class GameManager<TNetworkManager> where TNetworkManager : NetworkManager
    {
        protected GameManager()
        {
            networkConfig = LoadConfig<NetworkConfig>(networkConfigPath);
            playerConfig = LoadConfig<PlayerConfig>(playerConfigPath);
            projectileConfig = LoadConfig<ProjectileConfig>(projectileConfigPath);

            playerLookup = new Dictionary<int, IPlayer>(networkConfig.MaxPlayers);
            projectileLookup = new Dictionary<int, IProjectile>(networkConfig.MaxPlayers * 16);
            networkedInputListenerLookup = new Dictionary<int, NetworkedInputListener>(networkConfig.MaxPlayers);
            messageHandler = GenerateMessageHandler();
        }

        protected readonly IMessageHandler messageHandler;

        protected const string networkConfigPath = "NetworkConfig.json";
        protected const string playerConfigPath = "PlayerConfig.json";
        protected const string projectileConfigPath = "ProjectileConfig.json";

        protected abstract TNetworkManager GenerateNetworkManager();
        protected abstract IMessageHandler GenerateMessageHandler();
        protected TNetworkManager networkManager { get; private set; } = null;

        protected abstract void Log(string message);

        public NetworkStatistics GetNetworkTotalStatistics()
            => networkManager != null ? networkManager.GetStatistics() : NetworkStatistics.Empty;

        public NetworkStatistics GetNetworkDiffStatistics()
            => networkManager != null ? networkManager.GetDiffStatistics() : NetworkStatistics.Empty;

        protected bool StartNetworkingInternal()
        {
            StopNetworking();

            networkManager = GenerateNetworkManager();
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

            networkManager.Tick();

            if (networkManager == null) //networkManager.Tick() can make us close connection - we need to check here again to make sure we're still in-game.
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
                    projectile.Destroy();
                }
            }
        }

        protected IEnumerable<IPlayer> AllPlayers => playerLookup.Values;
        protected IEnumerable<IProjectile> AllProjectiles => projectileLookup.Values;


        Dictionary<int, IPlayer> playerLookup;
        Dictionary<int, IProjectile> projectileLookup;
        Dictionary<int, NetworkedInputListener> networkedInputListenerLookup;

        public readonly NetworkConfig networkConfig;
        protected readonly PlayerConfig playerConfig;
        protected readonly ProjectileConfig projectileConfig;

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

        protected void InstantiateNetworkedPlayer(int ID)
        {
            var listener = new NetworkedInputListener();

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

            if (playerLookup.ContainsKey(ID))
            {
                Log($"InstantiatePlayerInternal Error: player ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            playerLookup[ID] = player;

            Log($"Player {ID} Created");
            player.Destroyed += OnPlayerDestroyed;

            return player;
        }

        protected virtual void OnPlayerDestroyed(IPlayer player)
        {

            Log($"Player {player.ID} Destroyed");
            player.Destroyed -= OnPlayerDestroyed;

            networkedInputListenerLookup.Remove(player.ID);
            playerLookup.Remove(player.ID);
        }

        protected abstract Projectile InstantiateProjectileInternal(int ID, int ownerID, Vector3 position, Vector3 direction);

        protected void InstantiateProjectile(int ID, int ownerID, Vector3 position, Vector3 direction)
        {
            Projectile projectile = InstantiateProjectileInternal(ID, ownerID, position, direction);

            if (projectileLookup.ContainsKey(ID))
            {
                Log($"InstantiateProjectile Error: projectile ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }
            projectile.Destroyed += OnProjectileDestroyed;

            projectileLookup[ID] = projectile;
        }

        protected virtual void OnProjectileDestroyed(IProjectile projectile)
        {
            projectile.Destroyed -= OnProjectileDestroyed;
            projectileLookup.Remove(projectile.ID);
        }
        protected virtual void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            if (TryGetNetworkedInputListener(playerID, out var listener))
            {
                listener.Update(input, position, rotation);
            }
        }

        protected virtual void OnPlayerUpdateReceived(PlayerUpdateMessage message)
        {
            ApplyNetworkedMovement(message.playerID, message.input, message.position, message.rotation);
        }

        static List<IProjectile> projectileCleanupList = new List<IProjectile>();
        static List<IPlayer> playerCleanupList = new List<IPlayer>();

        protected virtual void Cleanup()
        {
            projectileCleanupList.Clear();
            projectileCleanupList.AddRange(AllProjectiles);

            playerCleanupList.Clear();
            playerCleanupList.AddRange(AllPlayers);

            foreach (var projectile in projectileCleanupList)
            {
                projectile.Destroy();
            }
            foreach (var player in playerCleanupList)
            {
                player.Destroy();
            }
        }

        protected void OnPlayerDisconnected(int playerID)
        {
            if (TryGetPlayer(playerID, out var player))
            {
                player.Destroy();
            }
            else
            {
                Log($"DestroyPlayer Error: player ID {playerID} does not exist in lookup! ignoring...");
            }
        }

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
