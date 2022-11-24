using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;
using TMPro;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager instance;

    public TextMeshProUGUI debugVersionInfo;

    public ushort maxPlayers = 8;

    public GameObject lobby;
    public GameObject currentLobby;

    public string currentUsername = "";

    public TMP_InputField lobbyCodeInput;

    public Texture2D cursor;

    private void Start()
    {
#if DEBUG
        debugVersionInfo.gameObject.SetActive(true);
        debugVersionInfo.text = $"THIS APPLICATION IS RUNNING IN <i>DEBUG MODE</i>\n{Application.companyName}       {Application.productName}\n<b>Version: {Application.version} running on Unity version {Application.unityVersion}</b>";
#endif

        if (NetworkManager.instance != null)
        {
            if (NetworkManager.instance.Client.IsConnected)
            {
                currentLobby = Instantiate(lobby);
            }
        }

        currentUsername = SteamFriends.GetPersonaName();

        instance = this;

        NetworkManager.instance.mainMenuUIManager = this;

        Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);
    }

    public void ConnectedToLobby()
    {
        currentLobby = Instantiate(lobby);
    }

    public void Join()
    {
        SteamLobbyManager.Singleton.JoinLobby(lobbyCodeInput.text);
    }

    public void Host()
    {
        SteamLobbyManager.Singleton.CreateLobby(maxPlayers);
    }
}
