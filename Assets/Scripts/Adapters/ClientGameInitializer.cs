using Adapters.Input;
using Shared.Networking;
using System;
using UnityEngine;

namespace Adapters
{
    public class ClientGameInitializer : MonoBehaviour
    {
        [SerializeField] private LocalInputListener _localInputListener;

        ClientGameManager _manager;

        public NetworkManager.NetworkStatistics GetNetworkStatistics()
            => _manager.GetNetworkStatistics();

        private void Awake()
        {
            _manager = new ClientGameManager(_localInputListener);
            _manager.StateChanged += _manager_StateChanged;
            _manager.MessageLogged += _manager_MessageLogged;
        }
        public event Action<string> MessageLogged;

        private void _manager_MessageLogged(string obj)
        {
            MessageLogged?.Invoke(obj);
        }

        public event Action<NetworkManager.ConnectionState> StateChanged;

        private void _manager_StateChanged(Shared.Networking.NetworkManager.ConnectionState obj)
        {
            StateChanged?.Invoke(obj);
        }

        private void FixedUpdate()
        {
            _manager.Tick();
        }

        public void StartClient()
        {
            _manager.StartNetworking();
        }

        public void StopClient()
        {
            _manager.StopNetworking();
        }

    }
}
