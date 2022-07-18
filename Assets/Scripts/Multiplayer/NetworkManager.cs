using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public MainMenuUIManager mainMenuUIManager;

    public Server Server;
    public Client Client;

    public List<MultiplayerPlayer> connectedPlayers = new List<MultiplayerPlayer>();

    [System.Serializable]
    public class MultiplayerPlayer
    {
        ushort id;
        string name;
        GameObject playerObject;
        bool isReady = false;

        ushort skinId;
        ushort hatId;

        public MultiplayerPlayer(ushort id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server { AllowAutoMessageRelay = true };

        Client = new Client();
        Client.Connected += Connected;
        //Client.ConnectionFailed += FailedToConnect;
        //Client.ClientConnected += PlayerJoined;
        //Client.ClientDisconnected += PlayerLeft;
        //Client.Disconnected += DidDisconnect;
    }

    public void HostGame(ushort port, ushort maxPlayers)
    {
        Server.Start(port, maxPlayers);
        Client.Connect($"127.0.0.1:{port}");
    }

    public void JoinGame(string ip, ushort port)
    {
        Client.Connect($"{ip}:{port}");
    }

    public void LeaveGame()
    {
        if (Server.IsRunning)
        {
            Server.Stop();
        }

        Client.Disconnect();
    }

    private void FixedUpdate()
    {
        if (Server.IsRunning)
        {
            Server.Tick();
        }

        Client.Tick();
    }

    void Connected(object sender, EventArgs e)
    {
        connectedPlayers[Client.Id] = new MultiplayerPlayer(Client.Id, mainMenuUIManager.playerName);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
        Client.Disconnect();
    }
}
