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
        public GameObject playerObject;
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
        playerInfo = 1, //contains data about player (cosmetics, name)
        startGame, //tells the players to start loading the game world
        playerReady, //player has finished loading game world
        mapHeader, //contains basic information about the generated map, ie total room count
        mapData, //contains room information, ie type of room, which halls are open
        mapDone, //map generation has finished
        playerPos, //player position and rotation updates
        openChest, //tell other players that a chest has been opened
        spawnItem, //tells other players that an item has spawned
        pickUpItem, //tells other players that an item has been picked up
        spawnHeal,  //tells other players that a healing item has spawned
        pickUpHeal, //tells other players that a healing item has been picked up
        spawnAmmo, //tells other players that an ammo item has spawned
        pickUpAmmo, //tells other players that an ammo item has been picked up
        playerHoldItem, //tells other players what item local player is holding
        playerGunRot,//tells other players what direction the gun is facing
        particleEffect, //tells other players to spawn a particle effect
        playerReloadSound, //sound effects that should travel with a player (ie, a reloading sound effect should follow the player)
        playerShot, //contains data about what gun was shot and where
        playerDamage, //tells all players that player x has been damaged
        playerHeal, //tells other players that the local player has healed
        playerOutOfGame, //tells other players when a player is no longer in the game
        soundEffect, //tells other players to play a sound effect
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
        Client.Disconnected += Disconnected;
    }

    public void Disconnected(object sender, EventArgs e)
    {
        readyPlayers = new List<ushort>();
        mapReadyPlayers = new List<ushort>();

        connectedPlayers = new List<MultiplayerPlayer>();

        gameIsStarted = false;
        stillInLobby = true;
        isDoneLoading = false;

        LoadingScreen.instance.LoadLevel("MainMenu");
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

        readyPlayers = new List<ushort>();
        mapReadyPlayers = new List<ushort>();

        connectedPlayers = new List<MultiplayerPlayer>();

        gameIsStarted = false;
        stillInLobby = true;
        isDoneLoading = false;

        LoadingScreen.instance.LoadLevel("MainMenu");
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
        msg.AddString(Application.version);
        Client.Send(msg);
    }

    [MessageHandler((ushort)MessageIds.playerInfo)]
    static void RecievedPlayerInfo(Message msg)
    {
        ushort id = msg.GetUShort();
        string username = msg.GetString();
        string version = msg.GetString();

        if (version != Application.version || instance.gameIsStarted)
        {
            if (instance.Server.IsRunning)
            {
                instance.Server.DisconnectClient(id);
            }

            return;
        }

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
