using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using Riptide.Transports.Steam;
using System;

public class NetworkManager : MonoBehaviour
{
    public bool isSteam = true;
    public GameObject steamManager;

    public static NetworkManager instance;

    public MainUIManager mainMenuUIManager;
    public GameManager gameManager;

    public Server Server;
    public Client Client;

    public List<MultiplayerPlayer> connectedPlayers = new List<MultiplayerPlayer>();

    public bool localPlayerIsHost = false;

    public bool gameIsStarted = false;
    public bool stillInLobby = true;
    public bool isDoneLoading = false;

    public List<ushort> playersReadyToStart = new List<ushort>();
    public List<ushort> readyPlayers = new List<ushort>();
    public List<ushort> mapReadyPlayers = new List<ushort>();

    [System.Serializable]
    public class MultiplayerPlayer
    {
        public ushort id;
        public string name;
        public GameObject playerObject;
        public bool isHost = false;

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
        rules, //sends game rules from host to player
        startGame, //tells the players to start loading the game world
        playerReady, //player has finished loading game world
        readyUp, //lets everyone else know that you are ready to start the game
        mapHeader, //contains basic information about the generated map, ie total room count
        mapData, //contains room information, ie type of room, which halls are open
        mapDone, //map generation has finished
        playerPos, //player position and rotation updates
        playerHealth, //player health updates
        openChest, //tells other players that a chest has been opened
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

    private void OnEnable()
    {

        if (!isSteam)
        {
            if (steamManager)
            {
                Destroy(steamManager);
            }
        }

        if (instance == null)
        {
            Debug.Log("[Network Manager] Setting NetworkManager instance!");
            DontDestroyOnLoad(gameObject);
            instance = this;
        } else
        {
            Destroy(gameObject);
            return;
        }

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        if (SteamManager.Initialized)
        {
            Debug.Log("[Network Manager] Steam is initalized, starting steam server.");
            SteamServer steamServer = new SteamServer();

            Server = new Server(steamServer);
            Client = new Client(new SteamClient(steamServer));
        } else
        {
            Server = new Server();
            Client = new Client();
            Debug.Log("[Network Manager] Steam is not initalized, using direction connection server.");
        }

        List<ushort> msgIdsToRelay = new List<ushort>();

        foreach (MessageIds id in Enum.GetValues(typeof(MessageIds)))
        {
            msgIdsToRelay.Add((ushort)id);
        }

        //value needs to be 1 more than the highest ID in MessageIds
        MessageRelayFilter filter = new MessageRelayFilter(27);

        foreach (var id in msgIdsToRelay)
        {
            filter.EnableRelay(id);
        }

        Server.RelayFilter = filter;
        

        //Server.RelayFilter = new MessageRelayFilter((ushort)MessageIds.mapData, (ushort)MessageIds.mapDone, (ushort)MessageIds.mapHeader, (ushort)MessageIds.openChest, (ushort)MessageIds.particleEffect, (ushort)MessageIds.pickUpAmmo, (ushort)MessageIds.pickUpHeal, (ushort)MessageIds.pickUpItem, (ushort)MessageIds.playerDamage, (ushort)MessageIds.playerGunRot, (ushort)MessageIds.playerHeal, (ushort)MessageIds.playerHoldItem, (ushort)MessageIds.playerInfo, (ushort)MessageIds.playerOutOfGame, (ushort)MessageIds.playerPos, (ushort)MessageIds.playerReady, (ushort)MessageIds.playerReloadSound, (ushort)MessageIds.playerShot, (ushort)MessageIds.readyUp, (ushort)MessageIds.rules, (ushort)MessageIds.soundEffect, (ushort)MessageIds.spawnAmmo, (ushort)MessageIds.spawnHeal, (ushort)MessageIds.spawnItem, (ushort)MessageIds.startGame);

        Client.Connected += Connected;
        //Client.ConnectionFailed += FailedToConnect;
        Client.ClientConnected += PlayerJoined;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += Disconnected;
    }

    public void Disconnected(object sender, EventArgs e)
    {
        Debug.Log($"[Network Manager] Disconnected from server.");

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
        Debug.Log("Starting to host.");
        Server.Start(port, maxPlayers);
        Client.Connect($"127.0.0.1:{port}");
    }

    public void JoinGame(string ip, ushort port)
    {
        Client.Connect($"{ip}:{port}");
    }

    public void LeaveGame()
    {
        Debug.Log("[Network Manager] Leaving game.");
        if (isSteam)
        {
            SteamLobbyManager.Singleton.LeaveLobby();
        } else
        {
            Client.Disconnect();
            if (Server.IsRunning)
            {
                Server.Stop();
            }
        }

        readyPlayers = new List<ushort>();
        mapReadyPlayers = new List<ushort>();

        connectedPlayers = new List<MultiplayerPlayer>();

        gameIsStarted = false;
        stillInLobby = true;
        isDoneLoading = false;
    }

    private void FixedUpdate()
    {
        if (Server.IsRunning)
        {
            Server.Update();
        }

        Client.Update();
    }

    void Connected(object sender, EventArgs e)
    {
        Debug.Log("[Network Manager] Successfully connected to server, sending player data.");
        connectedPlayers.Add(new MultiplayerPlayer(Client.Id, mainMenuUIManager.currentUsername));
        SendMyPlayerInfo();
    }

    void PlayerJoined(object sender, ClientConnectedEventArgs e)
    {
        Debug.Log("[Network Manager] Player has joined, sending client info.");
        SendMyPlayerInfo();
        if (instance.Server.IsRunning)
        {
            Debug.Log("[Network Manager] Player has joined, client is hosting, sending game rules.");
            RulesManager.instance.SendRuleChangesToPlayers();
        }
    }

    void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Debug.Log($"[Network Manager] Player is disconnecting.");

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
        Message msg = Message.Create(MessageSendMode.Reliable, MessageIds.playerInfo);
        msg.AddUShort(Client.Id);
        msg.AddString(mainMenuUIManager.currentUsername);
        msg.AddString(Application.version);
        Client.Send(msg);
    }

    [MessageHandler((ushort)MessageIds.playerInfo)]
    static void RecievedPlayerInfo(Message msg)
    {
        ushort id = msg.GetUShort();
        string username = msg.GetString();
        string version = msg.GetString();

        Debug.Log($"[Network Manager] Got player info: ID:{id} Name:{username} Remote client version:{version}");

        if (version != Application.version || instance.gameIsStarted)
        {
            Debug.Log("[Network Manager] Remote client[" + id + " + " + username + "] is not playing on the same version as local client.");
            if (instance.Server.IsRunning)
            {
                Debug.Log("[Network Manager] Disconnecting remote client[" + id + " + " + username + " from server because remote client is not playing on the same version as local client.");
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

    [MessageHandler((ushort)MessageIds.readyUp)]
    public static void ReadyUp(Message msg)
    {
        bool ready = msg.GetBool();
        ushort id = msg.GetUShort();

        instance.PlayerReadyUp(ready, id);
    }

    public void PlayerReadyUp(bool ready, ushort id)
    {
        if (ready)
        {
            Debug.Log($"[Network Manager] {id} is ready to start.");
        } else
        {
            Debug.Log($"[Network Manager] {id} is not ready to start.");
        }

        if (ready)
        {
            instance.playersReadyToStart.Add(id);
        }
        else
        {
            instance.playersReadyToStart.Remove(id);
        }

        if (instance.playersReadyToStart.Count == instance.connectedPlayers.Count)
        {
            instance.playersReadyToStart.Clear();

            if (instance.Server.IsRunning)
            {
                Message message = Message.Create(MessageSendMode.Reliable, MessageIds.startGame);
                message.AddBool(true);
                instance.Client.Send(message);
                StartGame(message);
                instance.stillInLobby = false;
            }
        }
    }

    [MessageHandler((ushort)MessageIds.startGame)]
    public static void StartGame(Message msg)
    {
        Debug.Log($"[Network Manager] Recieved start game packet. Loading game scene.");
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

        Debug.Log($"[Network Manager] {id} is done loading game scene. {instance.readyPlayers.Count}/{instance.connectedPlayers.Count} done.");

        if (instance.readyPlayers.Count == instance.connectedPlayers.Count)
        {
            Debug.Log($"[Network Manager] All players finished loading game scene, starting world generation.");
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

        Debug.Log($"[Network Manager] {id} is done loading generated world. {instance.mapReadyPlayers.Count}/{instance.connectedPlayers.Count} done.");

        if (instance.mapReadyPlayers.Count == instance.connectedPlayers.Count)
        {
            Debug.Log($"[Network Manager] All players are done loading generated world, unpausing game.");
            instance.gameIsStarted = true;
        }
    }

    #endregion
}
