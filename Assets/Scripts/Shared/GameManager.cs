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
using System.Text.Json;

namespace Shared
{
    public abstract class GameManager<TNetworkManager> where TNetworkManager : INetworkManager
    {
        protected GameManager()
        {
            //networkConfig = LoadConfig<NetworkConfig>(networkConfigPath);
            //playerConfig = LoadConfig<PlayerConfig>(playerConfigPath);
            //projectileConfig = LoadConfig<ProjectileConfig>(projectileConfigPath);
            //networkConfig = new NetworkConfig(LoadConfig<NetworkConfigData>(networkConfigPath));
            //playerConfig = new PlayerConfig(LoadConfig<PlayerConfigData>(playerConfigPath));
            //projectileConfig = new ProjectileConfig(LoadConfig<ProjectileConfigData>(projectileConfigPath));

            GetConfigData();

            playerLookup = new Dictionary<int, IPlayer>();
            projectileLookup = new Dictionary<int, IProjectile>();
            networkedInputListenerLookup = new Dictionary<int, NetworkedInputListener>();

            networkManager = GenerateNetworkManager();
            networkManager.MessageLogged += Log;
        }

        protected abstract void GetConfigData();


        //protected const string networkConfigPath = "NetworkConfig.json";
        //protected const string playerConfigPath = "PlayerConfig.json";
        //protected const string projectileConfigPath = "ProjectileConfig.json";

        protected abstract TNetworkManager GenerateNetworkManager();
        protected readonly TNetworkManager networkManager;

        protected abstract void Log(string message);

        public NetworkStatistics GetNetworkTotalStatistics()
            => networkManager.started ? networkManager.GetTotalStatistics() : NetworkStatistics.Empty;

        public NetworkStatistics GetNetworkDiffStatistics()
            => networkManager.started ? networkManager.GetDiffStatistics() : NetworkStatistics.Empty;

        protected bool TryStartNetworkingInternal(int port)
        {
            StopNetworking();

            if (!networkManager.TryStartNetworking(port))
            {
                Log("Failed to start networking!");
                return false;
            }
            //Log("Started networking!");
            return true;
        }

        public void StopNetworking()
        {
            if (!networkManager.started)
            {
                return;
            }

            networkManager.Stop();
            StateChanged?.Invoke(NetworkManager.ConnectionState.Uninitialized);
            //Log("Stopped networking!");
        }

        public event Action<NetworkManager.ConnectionState> StateChanged;

        public void Update(float deltaTime)
        {
            if (!networkManager.started)
                return;

            //Log("Tick!");

            networkManager.Update(deltaTime);

            var connectionState = networkManager.CheckConnectionState(out bool stateChanged);
            if (stateChanged)
                StateChanged?.Invoke(connectionState);

            if (connectionState != NetworkManager.ConnectionState.Connected)
                return;

            foreach (var player in playerLookup.Values)
            {
                player.Update(deltaTime);
            }

            foreach (var projectile in projectileLookup.Values)
            {
                projectile.Update(deltaTime);
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

        //public readonly NetworkConfig networkConfig;
        //protected readonly PlayerConfig playerConfig;
        //protected readonly ProjectileConfig projectileConfig;

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
            var player = InstantiatePlayer(ID,listener, false);
        }

        protected abstract Player.Player InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local);

        protected Player.Player InstantiatePlayer(int ID, IInputListener inputListener, bool local)
        {
            var player = InstantiatePlayerInternal(ID, inputListener, local);

            if (playerLookup.ContainsKey(ID))
            {
                Log($"InstantiatePlayerInternal Error: player ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            playerLookup[ID] = player;

            Log($"Player {ID} Created");
            player.Destroyed += OnPlayerDestroyed;
            player.ShotProjectile += OnPlayerShot;

            return player;
        }

        protected virtual void OnPlayerDestroyed(IPlayer player)
        {

            Log($"Player {player.ID} Destroyed");
            player.Destroyed -= OnPlayerDestroyed;
            player.ShotProjectile -= OnPlayerShot;

            networkedInputListenerLookup.Remove(player.ID);
            playerLookup.Remove(player.ID);
        }

        protected virtual void OnPlayerShot(IPlayer player, IProjectile projectile)
        {
            if (ProjectileExists(projectile.ID))
            {
                Log($"InstantiateProjectile Error: projectile ID {projectile.ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            projectile.Destroyed += OnProjectileDestroyed;

            projectileLookup[projectile.ID] = projectile;
        }

        protected virtual void OnProjectileSpawnReceived(ProjectileSpawnMessage message)
        {
            if (TryGetPlayer(message.ownerID, out var player))
            {
                player.Shoot(message.projectileID,message.position,message.direction);
            }
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
            => ApplyNetworkedMovement(message.playerID, message.input, message.position, message.rotation);

        static List<IProjectile> projectileCleanupList = new List<IProjectile>();
        static List<IPlayer> playerCleanupList = new List<IPlayer>();

        protected virtual void Cleanup()
        {
            projectileCleanupList.Clear();
            projectileCleanupList.AddRange(AllProjectiles);

            playerCleanupList.Clear();
            playerCleanupList.AddRange(AllPlayers);

            foreach (var projectile in projectileCleanupList)
                projectile.Destroy();

            foreach (var player in playerCleanupList)
                player.Destroy();
        }

        protected void OnPlayerDisconnected(int playerID)
        {
            if (TryGetPlayer(playerID, out var player))
            {
                player.Destroy();

                projectileCleanupList.Clear();

                foreach (var projectile in AllProjectiles)
                {
                    if(projectile.ownerID == playerID)
                        projectileCleanupList.Add(projectile);
                }

                foreach (var projectile in projectileCleanupList)
                    projectile.Destroy();
            }
            else
                Log($"DestroyPlayer Error: player ID {playerID} does not exist in lookup! ignoring...");
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

        protected T Deserialize<T>(string jsonString) where T : class
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }

        protected string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
