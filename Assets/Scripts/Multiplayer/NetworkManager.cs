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
    public GameManager gameManager;

    public Server Server;
    public Client Client;

    public List<MultiplayerPlayer> connectedPlayers = new List<MultiplayerPlayer>();

    public bool gameIsStarted = false;
    public bool stillInLobby = true;
    public bool isDoneLoading = false;

    public List<ushort> readyPlayers = new List<ushort>();
    public List<ushort> mapReadyPlayers = new List<ushort>();

    [System.Serializable]
    public class MultiplayerPlayer
    {
        public ushort id;
        public string name;
        GameObject playerObject;
        public bool isReady = false;

        public ushort skinId;
        public ushort hatId;

        public MultiplayerPlayer(ushort id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public enum MessageIds : ushort
    {
        playerInfo = 1,
        startGame,
        playerReady,
        mapHeader,
        mapData,
        mapDone,
        playerPos
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = this;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server { AllowAutoMessageRelay = true };

        Client = new Client();
        Client.Connected += Connected;
        //Client.ConnectionFailed += FailedToConnect;
        Client.ClientConnected += PlayerJoined;
        Client.ClientDisconnected += PlayerLeft;
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
        connectedPlayers.Add(new MultiplayerPlayer(Client.Id, mainMenuUIManager.playerName));
        SendMyPlayerInfo();
    }

    void PlayerJoined(object sender, ClientConnectedEventArgs e)
    {
        SendMyPlayerInfo();
    }

    void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        if (gameManager != null)
            gameManager.PlayerLeft(e.Id);

        foreach (var player in connectedPlayers)
        {
            if (player.id == e.Id)
            {
                connectedPlayers.Remove(player);
                return;
            }
        }
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
        Client.Disconnect();
    }

    #region messages

    void SendMyPlayerInfo()
    {
        Message msg = Message.Create(MessageSendMode.reliable, MessageIds.playerInfo, shouldAutoRelay: true);
        msg.AddUShort(Client.Id);
        msg.AddString(mainMenuUIManager.playerName);
        Client.Send(msg);
    }

    [MessageHandler((ushort)MessageIds.playerInfo)]
    static void RecievedPlayerInfo(Message msg)
    {
        ushort id = msg.GetUShort();
        string username = msg.GetString();

        bool exists = false;
        foreach (var player in instance.connectedPlayers)
        {
            if (player.id == id)
            {
                exists = true;
            }
        }

        if (!exists)
        {
            instance.connectedPlayers.Add(new MultiplayerPlayer(id, username));
        }
    }

    [MessageHandler((ushort)MessageIds.startGame)]
    public static void StartGame(Message msg)
    {
        LoadingScreen.instance.LoadLevel("Game");
    }

    [MessageHandler((ushort)MessageIds.playerReady)]
    public static void PlayerReady(Message msg)
    {
        instance.PlayerIsReady(msg.GetUShort());
    }

    public void PlayerIsReady(ushort id)
    {
        readyPlayers.Add(id);

        if (instance.readyPlayers.Count == instance.connectedPlayers.Count)
        {
            instance.isDoneLoading = true;
            readyPlayers.Clear();
        }
    }

    [MessageHandler((ushort)MessageIds.mapDone)]
    public static void MapReady(Message msg)
    {
        instance.MapIsReady(msg.GetUShort());
    }

    public void MapIsReady(ushort id)
    {
        mapReadyPlayers.Add(id);

        if (instance.mapReadyPlayers.Count == instance.connectedPlayers.Count)
        {
            instance.gameIsStarted = true;
        }
    }

    #endregion
}
