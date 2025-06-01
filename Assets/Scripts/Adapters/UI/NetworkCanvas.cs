using LiteNetLib;
using Shared.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkCanvas : MonoBehaviour
{
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

    GameClient gameClient;

    public void StartClient()
    {
        StopClient();

        gameClient = new GameClient(NetworkConfig.Port, NetworkConfig.TickInterval);
        testMessageTimer = 0f;
        currentMessageIndex = 0;
        gameClient.MessageReceived += OnMessageReceived;
    }

    public void StopClient()
    {
        if (gameClient != null)
        {
            gameClient.MessageReceived -= OnMessageReceived;
            gameClient.Stop();
            gameClient = null;
        }
    }

    private void OnMessageReceived(NetPeer peer, INetworkMessage message)
    {
        switch (message.MsgType)
        {
            case MessageType.CustomMessage:
                OnCustomMessageReceived((CustomMessage)message);
                break;
            //case MessageType.PlayerUpdate:
            //    break;
            //case MessageType.ProjectileSpawn:
            //    break;
            //case MessageType.ProjectileDespawn:
            //    break;
            //case MessageType.HealthUpdate:
            //    break;
            //case MessageType.Death:
            //    break;
            //case MessageType.Respawn:
            //    break;
            default:
                Debug.LogError($"Unhandled network message type: {message.MsgType}");
                return;
        }
    }

    private void OnCustomMessageReceived(CustomMessage message)
    {
        Log($"Received: {message.msg} ({(testMessageTimer * 1000):0.0}ms later)");
    }

    enum ConnectionState : byte
    {
        Uninitialized,
        Inactive,
        Started,
        Connected
    }

    void Log(string message)
    {
        Debug.Log(message);

        if(logUI != null)
            logUI.Log(message);
    }

    ConnectionState currentConnectionState = ConnectionState.Uninitialized;

    ConnectionState GetConnectionState()
    {
        if (gameClient == null || !gameClient.started)
            return ConnectionState.Inactive;
            
        if (gameClient.IsConnected())
            return ConnectionState.Connected;
                
        return ConnectionState.Started;
    }

    // Update is called once per frame
    void Update()
    {
        var connState = GetConnectionState();
        if (connState != currentConnectionState)
        {
            currentConnectionState = connState;
            status.text = statusPrefix + connState.ToString();
        }

        if (currentConnectionState == ConnectionState.Connected)
        {
            gameClient.PollEvents();

            testMessageTimer += Time.deltaTime;
            if(testMessageTimer >= testMessageInterval)
            {
                testMessageTimer = 0f;
                var msg = $"{testMessage} {currentMessageIndex}";
                Log($"Sent: {msg}");
                gameClient.Send(new CustomMessage(msg));
                currentMessageIndex++;
            }
        }
    }
}
