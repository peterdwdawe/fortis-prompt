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
    [SerializeField] float testMessageInterval = 1f;
    [SerializeField] string testMessage = "test msg";

    [SerializeField] TextMeshProUGUI status;
    [SerializeField] LogGroup logUI;

    const string statusPrefix = "Status: ";

    float testMessageTimer = 0f;

    int currentMessageIndex = 0;

    public void StartClient()
    {
        gameManager.StartClient();
    }

    public void StopClient()
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
        status.text = statusPrefix + newState.ToString();
    }

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
    }

    private void OnDestroy()
    {
        gameManager.StateChanged -= GameClient_StateChanged;
        gameManager.MessageLogged -= LogUI;
    }

    // Update is called once per frame
    void Update()
    {
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
