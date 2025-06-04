using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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

        public bool StartNetworking(string address)
        {
            StopNetworking();

            networkManager = GenerateNetworkManager();
            var messageHandler = GenerateMessageHandler();
            if (!networkManager.Start(address, messageHandler))
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

            NetworkState.Tick();

            networkManager.PollEvents();

            if (networkManager == null) //PollEvents can make us close connection - we need to check here again to make sure we're still in-game.
                return;

            var networkState = networkManager.CheckConnectionState(out bool stateChanged);
            if (stateChanged)
                StateChanged?.Invoke(networkState);

            if (networkState != NetworkManager.ConnectionState.Connected)
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

        public GameManager()
        {
            playerLookup = new Dictionary<int, IPlayer>(NetworkConfig.MaxConnectionCount);
            projectileLookup = new Dictionary<int, IProjectile>(NetworkConfig.MaxConnectionCount * 16);
            networkedInputListenerLookup = new Dictionary<int, NetworkedInputListener>(NetworkConfig.MaxConnectionCount);
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
    }
}
