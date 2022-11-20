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
    GameObject currentLobby;

    public string currentUsername = "";

    public TextMeshProUGUI lobbyIdText;
    public TMP_InputField lobbyCodeInput;

    private void Awake()
    {
#if DEBUG
        debugVersionInfo.gameObject.SetActive(true);
        debugVersionInfo.text = $"THIS APPLICATION IS RUNNING IN <i>DEBUG MODE</i>\n{Application.companyName}       {Application.productName}\n<b>Version: {Application.version} running on Unity version {Application.unityVersion}</b>";
#endif

        currentUsername = SteamFriends.GetPersonaName();

        instance = this;
    }

    public void ConnectedToLobby()
    {
        currentLobby = Instantiate(lobby);
        lobbyIdText.text = SteamLobbyManager.Singleton.lobbyId.ToString();
    }
}
