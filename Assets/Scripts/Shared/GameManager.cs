using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Shared.Input;
using Shared.Networking;
using Shared.Networking.Messages;
using Shared.Player;
using Shared.Projectiles;
using System.Text.Json;

namespace Shared
{
    public abstract class GameManager
    {
        protected GameManager()
        {
            GetConfigData();
            networkManager = GenerateNetworkManager();
            networkManager.MessageLogged += Log;
        }

        private readonly INetworkManager networkManager;

        protected abstract INetworkManager GenerateNetworkManager();

        protected abstract void GetConfigData();

        protected abstract void Log(string message);

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

            foreach (var player in AllPlayers)
                player.Update(deltaTime);

            foreach (var projectile in AllProjectiles)
            {
                projectile.Update(deltaTime);

                if (projectile.Expired)
                    projectileCleanupList.Add(projectile);  //if we destroy here, we may get collection errors.
            }

            foreach (var projectile in projectileCleanupList)
                projectile.Destroy(true);

            projectileCleanupList.Clear();
        }

        public void StopNetworking()
        {
            if (!networkManager.started)
            {
                return;
            }

            networkManager.StopNetworking();
            StateChanged?.Invoke(NetworkManager.ConnectionState.Uninitialized);
            //Log("Stopped networking!");
        }

        public event Action<NetworkManager.ConnectionState> StateChanged;

        #region Networked Object Dictionaries

        private readonly Dictionary<int, IPlayer> playerLookup = new Dictionary<int, IPlayer>();
        private readonly Dictionary<int, IProjectile> projectileLookup = new Dictionary<int, IProjectile>();
        private readonly Dictionary<int, NetworkedInputListener> networkedInputListenerLookup = new Dictionary<int, NetworkedInputListener>();

        protected IEnumerable<IPlayer> AllPlayers => playerLookup.Values;
        protected IEnumerable<IProjectile> AllProjectiles => projectileLookup.Values;

        protected bool TryGetPlayer(int ID, out IPlayer player)
            => playerLookup.TryGetValue(ID, out player) && player != null;

        protected bool PlayerExists(int ID)
             => playerLookup.ContainsKey(ID);

        protected bool TryGetProjectile(int ID, out IProjectile projectile)
             => projectileLookup.TryGetValue(ID, out projectile) && projectile != null;

        protected bool ProjectileExists(int ID)
             => projectileLookup.ContainsKey(ID);

        #endregion

        #region Player Utils

        protected abstract IPlayer InstantiatePlayerInternal(int ID, IInputListener inputListener, bool local);

        protected IPlayer InstantiatePlayer(int ID, IInputListener inputListener, bool local)
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

        protected void InstantiateNetworkedPlayer(int ID)
        {
            var listener = new NetworkedInputListener();

            if (networkedInputListenerLookup.ContainsKey(ID))
            {
                Log($"InstantiateNetworkedPlayer Error: NetworkedInputListener ID {ID} already exists! overwriting...");
                //TODO();//cleanup previous? this shouldn't be happening anyway
            }

            networkedInputListenerLookup[ID] = listener;
            var player = InstantiatePlayer(ID, listener, false);
        }

        protected virtual void ApplyNetworkedMovement(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            if (networkedInputListenerLookup.TryGetValue(playerID, out var listener) && listener != null)
                listener.Update(input, position, rotation);
        }

        #endregion

        #region Event Handlers

        protected virtual void OnPlayerDisconnected(int playerID)
        {
            if (TryGetPlayer(playerID, out var player))
                player.Destroy();
            else
                Log($"DestroyPlayer Error: player ID {playerID} does not exist in lookup! ignoring...");
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

        protected virtual void OnProjectileDestroyed(IProjectile projectile)
        {
            projectile.Destroyed -= OnProjectileDestroyed;
            projectileLookup.Remove(projectile.ID);
        }

        #endregion

        #region Network Message Handlers

        protected virtual void OnProjectileSpawnReceived(ProjectileSpawnMessage message)
        {
            if (TryGetPlayer(message.ownerID, out var player))
            {
                player.Shoot(message.projectileID, message.position, message.direction);
            }
        }

        protected virtual void OnPlayerUpdateReceived(PlayerUpdateMessage message) 
            => ApplyNetworkedMovement(message.playerID, message.input, message.position, message.rotation);

        #endregion

        #region Cleanup

        protected static readonly List<IProjectile> projectileCleanupList = new List<IProjectile>();
        protected static readonly List<IPlayer> playerCleanupList = new List<IPlayer>();

        protected virtual void Cleanup()
        {
            projectileCleanupList.AddRange(AllProjectiles);

            playerCleanupList.AddRange(AllPlayers);

            foreach (var projectile in projectileCleanupList)
                projectile.Destroy(true);

            foreach (var player in playerCleanupList)
                player.Destroy();

            playerCleanupList.Clear();
            projectileCleanupList.Clear();
        }

        #endregion

        #region Config Utilities

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
            => JsonSerializer.Deserialize<T>(jsonString);

        protected string Serialize<T>(T obj)
            => JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });

        #endregion

        #region Bandwidth Stats

        public NetworkStatistics GetNetworkTotalStatistics()
            => networkManager.started ? networkManager.GetTotalStatistics() : NetworkStatistics.Empty;

        public NetworkStatistics GetNetworkDiffStatistics()
            => networkManager.started ? networkManager.GetDiffStatistics() : NetworkStatistics.Empty;

        #endregion
    }
}