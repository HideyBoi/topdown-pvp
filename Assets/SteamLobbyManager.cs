using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using Riptide.Transports.Steam;
using Steamworks;

public class SteamLobbyManager : MonoBehaviour
{
    private static SteamLobbyManager _singleton;
    internal static SteamLobbyManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(LobbyManager)} instance already exists, destroying object!");
                Destroy(value);
            }
        }
    }

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEnter;

    private const string HostAddressKey = "HostAddress";
    public CSteamID lobbyId;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
    }

    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MainUIManager.instance.maxPlayers);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            LoadingScreen.instance.LoadLevel("MainMenu");
            return;
        }

        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        MainUIManager.instance.ConnectedToLobby();

        NetworkManager.instance.Server.Start(0, MainUIManager.instance.maxPlayers, messageHandlerGroupId: 255);
        NetworkManager.instance.Client.Connect("127.0.0.1", messageHandlerGroupId: 255);
    }

    public void JoinLobby()
    {
        SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(MainUIManager.instance.lobbyCodeInput.text)));
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        if (NetworkManager.instance.Server.IsRunning)
            return;

        lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, HostAddressKey);

        NetworkManager.instance.Client.Connect(hostAddress, messageHandlerGroupId: 255);
        MainUIManager.instance.ConnectedToLobby();
    }

    internal void LeaveLobby()
    {
        NetworkManager.instance.Client.Disconnect();
        NetworkManager.instance.Server.Stop();
        SteamMatchmaking.LeaveLobby(lobbyId);
    }
}
