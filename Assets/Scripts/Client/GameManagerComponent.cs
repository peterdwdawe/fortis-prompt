using Client.Input;
using Shared.Networking;
using System;
using UnityEngine;

namespace Client
{
    public class GameManagerComponent : MonoBehaviour
    {
        [SerializeField] private LocalInputListener _localInputListener;

        public event Action<string> MessageLogged;

        public event Action<NetworkManager.ConnectionState> StateChanged;

        public void StartClient(string address, int port)
        {
            client.ConnectToServer(address, port);
        }

        public void StopClient()
        {
            client.StopNetworking();
        }

        public NetworkStatistics GetNetworkDiffStatistics()
        {
            return client.GetNetworkDiffStatistics();
        }

        private GameClient client;

        private void Awake()
        {
            client = new GameClient(_localInputListener);
            client.StateChanged += OnStateChanged;
            client.MessageLogged += OnMessageLogged;
        }

        private void Start()
        {
            StateChanged?.Invoke(NetworkManager.ConnectionState.Uninitialized);
        }

        private void Update()
        {
            client.Update(Time.deltaTime);
        }

        private void OnStateChanged(Shared.Networking.NetworkManager.ConnectionState obj)
        {
            StateChanged?.Invoke(obj);
        }

        private void OnMessageLogged(string obj)
        {
            MessageLogged?.Invoke(obj);
        }
    }
}
