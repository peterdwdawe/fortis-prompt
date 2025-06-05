using Adapters;
using Adapters.Networking;
using LiteNetLib;
using Shared.Networking;
using Shared.Networking.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkCanvas : MonoBehaviour
{
    [SerializeField] ClientGameInitializer gameManager;

    //TODO: hook up connect/disconnect buttons, test sending messages

    //[SerializeField] int temp_port = 5000;
    //[SerializeField] float temp_interval = 0.02f;
    [SerializeField] float statsUpdateInterval = 0.5f;
    [SerializeField] float startedStateMaxDuration = 5f;
    [SerializeField][Range(0.1f,1f)] float menuVisibleAlpha = 0.75f;
    //[SerializeField] string testMessage = "test msg";

    [SerializeField] TextMeshProUGUI status;
    [SerializeField] TextMeshProUGUI statistics;
    [SerializeField] TMP_InputField serverAddress;
    [SerializeField] TMP_InputField serverPort;
    [SerializeField] LogGroup logUI;

    const string statusPrefix = "<b>Status:</b> ";

    float statsUpdateTimer = 0f;
    float stateTimer = 0f;


    [SerializeField] CanvasGroup mainMenuGroup;
    [SerializeField] CanvasGroup inGameGroup;

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

    //int currentMessageIndex = 0;

    public void StartClient()
    {
        if(!int.TryParse(serverPort.text, out var port)){
            LogUI($"{serverPort.text} is not a valid port number. using default port: 5000");
            port = 5000;
        }

        gameManager.StartClient(serverAddress.text, port);
        ShowInGameMenu();
    }

    public void StopClient()
    {
        //TODO: ignore / make non-interactable if client not already started
        //TODO: auto-stop if you can't connect to server
        gameManager.StopClient();
        ShowMainMenu();
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void OnApplicationQuit()
    {
        gameManager.StopClient();
    }

    //void SubscribeNetworkEvents()
    //{
    //    //CustomMessage.Received += OnCustomMessageReceived;
    //    //gameManager.StateChanged += GameClient_StateChanged;
    //}

    private void GameClient_StateChanged(NetworkManager.ConnectionState newState)
    {
        currentState = newState;
        status.text = statusPrefix + newState.ToString();
        stateTimer = 0f;
    }

    NetworkManager.ConnectionState currentState = NetworkManager.ConnectionState.Uninitialized;

    //void UnsubscribeNetworkEvents()
    //{
    //    //CustomMessage.Received -= OnCustomMessageReceived;
    //    //gameManager.StateChanged -= GameClient_StateChanged;
    //}

    ////private void OnCustomMessageReceived(NetPeer peer, CustomMessage message)
    ////{
    ////    Log($"Received: {message.msg} from server ({peer.Id})");
    ////}

    void LogUI(string message)
    {
        //Debug.Log(message);

        if(logUI != null)
            logUI.Log(message);
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

    string ToBandwidthString(float bandwidth)
    {
        if(bandwidth < 100)
        {
            return $"{bandwidth:0.0} B/s";
        }
        if (bandwidth < 1000)
        {
            return $"{bandwidth:0} B/s";
        }
        if (bandwidth < 10000)
        {
            return $"{(bandwidth / 1024):0.00} KB/s";
        }
        if (bandwidth < 100000)
        {
            return $"{(bandwidth / 1024):0.0} KB/s";
        }
        if (bandwidth < 1000000)
        {
            return $"{(bandwidth / 1024):0} KB/s";
        }
            
        return $"{(bandwidth / 1048576):0.00} MB/s";
    }

    string ToBandwidthString(NetworkManager.NetworkStatistics stats)
    {
        return 
            $"<b>Upload:</b> {ToBandwidthString(stats.AvgBandwidthUp)}\n" +
            $"<b>Download:</b> {ToBandwidthString(stats.AvgBandwidthDown)}";
    }

    // Update is called once per frame
    void Update()
    {
        stateTimer += Time.deltaTime;
        if (currentState == NetworkManager.ConnectionState.Started && stateTimer >= startedStateMaxDuration) 
        {
            StopClient();
        }

        if(currentState == NetworkManager.ConnectionState.Connected)
        {
            statsUpdateTimer += Time.deltaTime;
            if (statsUpdateTimer >= statsUpdateInterval)
            {
                statsUpdateTimer = 0f;
                statistics.text = ToBandwidthString(gameManager.GetNetworkStatistics());
            }
        }

        //if (gameManager.currentConnectionState == ClientGameManagerComponent.ConnectionState.Connected)
        //{
        //    testMessageTimer += Time.deltaTime;
        //    if(testMessageTimer >= testMessageInterval)
        //    {
        //        testMessageTimer = 0f;
        //        var msg = $"{testMessage} {currentMessageIndex}";
        //        Log($"Sent: {msg} to server");
        //        ClientNetworkManager.SendToServer(new CustomMessage(msg));
        //        currentMessageIndex++;
        //    }
        //}
        //else
        //{
        //    testMessageTimer = 0f;
        //    currentMessageIndex = 0;
        //}
    }
}
