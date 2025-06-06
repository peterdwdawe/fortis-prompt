using Shared.Networking;
using System;
using TMPro;
using UnityEngine;

namespace Client.Adapters.UI
{
    public class NetworkCanvas : MonoBehaviour
    {
        [SerializeField] GameManagerComponent gameManager;

        [SerializeField] float statsUpdateInterval = 0.5f;
        [SerializeField] float startedStateMaxDuration = 5f;
        [SerializeField][Range(0.1f, 1f)] float menuVisibleAlpha = 0.75f;

        [SerializeField] TextMeshProUGUI status;
        [SerializeField] TextMeshProUGUI statistics;
        [SerializeField] TMP_InputField serverAddress;
        [SerializeField] TMP_InputField serverPort;
        [SerializeField] LogGroup logUI;

        [SerializeField] CanvasGroup mainMenuGroup;
        [SerializeField] CanvasGroup inGameGroup;

        const string statusPrefix = "<b>Status:</b> ";

        float statsUpdateTimer = 0f;
        float stateTimer = 0f;
        NetworkManager.ConnectionState currentState = NetworkManager.ConnectionState.Uninitialized;

        private void GameClient_StateChanged(NetworkManager.ConnectionState newState)
        {
            currentState = newState;
            status.text = statusPrefix + newState.ToString();
            stateTimer = 0f;

            if (newState == NetworkManager.ConnectionState.Uninitialized)
            {
                ShowMainMenu(); //if server disconnects, go back to menu
            }
        }

        private void Awake()
        {
            gameManager.StateChanged += GameClient_StateChanged;
            gameManager.MessageLogged += LogUI;

            ShowMainMenu();
        }

        private void OnDestroy()
        {
            gameManager.StateChanged -= GameClient_StateChanged;
            gameManager.MessageLogged -= LogUI;
        }

        // Update is called once per frame
        void Update()
        {
            stateTimer += Time.deltaTime;
            if (currentState == NetworkManager.ConnectionState.Started && stateTimer >= startedStateMaxDuration)
            {
                StopClient();
            }

            if (currentState == NetworkManager.ConnectionState.Connected)
            {
                statsUpdateTimer += Time.deltaTime;
                if (statsUpdateTimer >= statsUpdateInterval)
                {
                    statsUpdateTimer = 0f;
                    statistics.text = gameManager.GetNetworkDiffStatistics().ToBandwidthString();
                }
            }
        }

        private void OnApplicationQuit()
        {
            gameManager.StopClient();
        }

        void LogUI(string message)
        {
            if (logUI != null)
                logUI.Log(message);
        }

        public void ShowMainMenu()
        {
            mainMenuGroup.alpha = menuVisibleAlpha;
            mainMenuGroup.interactable = true;
            mainMenuGroup.blocksRaycasts = true;

            inGameGroup.alpha = 0f;
            inGameGroup.interactable = false;
            inGameGroup.blocksRaycasts = false;
        }

        public void ShowInGameMenu()
        {
            inGameGroup.alpha = menuVisibleAlpha;
            inGameGroup.interactable = true;
            inGameGroup.blocksRaycasts = true;

            mainMenuGroup.alpha = 0f;
            mainMenuGroup.interactable = false;
            mainMenuGroup.blocksRaycasts = false;
        }

        public void StartClient()
        {
            if (!int.TryParse(serverPort.text, out var port))
            {
                LogUI($"{serverPort.text} is not a valid port number. using default port: 5000");
                port = 5000;
            }

            gameManager.StartClient(serverAddress.text, port);
            ShowInGameMenu();
        }

        public void StopClient()
        {
            gameManager.StopClient();
            ShowMainMenu();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}