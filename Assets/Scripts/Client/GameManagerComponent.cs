using Client.Input;
using Shared.Networking;
using System;
using UnityEngine;

namespace Client
{
    public class GameManagerComponent : MonoBehaviour
    {
        [SerializeField] private LocalInputListener _localInputListener;

        ClientGameManager _manager;

        public NetworkStatistics GetNetworkDiffStatistics()
            => _manager.GetNetworkDiffStatistics();

        private void Awake()
        {
            _manager = new ClientGameManager(_localInputListener);
            _manager.StateChanged += _manager_StateChanged;
            _manager.MessageLogged += _manager_MessageLogged;
        }

        private void Start()
        {
            StateChanged?.Invoke(NetworkManager.ConnectionState.Uninitialized);
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

        private void Update()
        {
            _manager.Update(Time.deltaTime);
        }

        public void StartClient(string address, int port)
        {
            _manager.ConnectToServer(address, port);
        }

        public void StopClient()
        {
            _manager.StopNetworking();
        }

    }
}
