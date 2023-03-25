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
    public GameObject visRoot;
    public GameObject currentLobby;

    public string currentUsername = "";

    public TMP_InputField lobbyCodeInput;
    public TMP_InputField maxPlayersInput;
    public TMP_Dropdown lobbyType;

    public Texture2D cursor;

    public GameObject cosmeticsRoot;
    public GameObject camRot;

    private void Start()
    {
        Cursor.SetCursor(cursor, new Vector2(cursor.width / 2, cursor.height / 2), CursorMode.Auto);

#if DEBUG
        debugVersionInfo.gameObject.SetActive(true);
        debugVersionInfo.text = $"THIS GAME IS IN DEVELOPMENT AND IS NOT REPRESENTATIVE OF THE FINISHED PRODUCT.\n\n{Application.productName} - {Application.companyName}\n<b>Version: {Application.version} running on Unity version: {Application.unityVersion}</b>\nMAKE SURE TO READ <b><i>LICENSE AND DISCLAIMER.txt</i></b>!!";
#endif

        if (NetworkManager.instance != null)
        {
            if (NetworkManager.instance.Client.IsConnected)
            {
                visRoot.SetActive(false);
                currentLobby = Instantiate(lobby);
            }
        }

        currentUsername = SteamFriends.GetPersonaName();

        instance = this;

        NetworkManager.instance.mainMenuUIManager = this;
    }

    public void ConnectedToLobby()
    {
        currentLobby = Instantiate(lobby);
        CloseCosmetics();
        visRoot.SetActive(false);
    }

    public void Join()
    {
        SteamLobbyManager.Singleton.JoinLobby(lobbyCodeInput.text);
    }

    public void Host()
    {
        int maxPlayers;

        try
        {
            maxPlayers = int.Parse(maxPlayersInput.text);
            if (maxPlayers < 2)
            {
                return;
            } else
            {
                SteamLobbyManager.Singleton.CreateLobby((ushort)maxPlayers, lobbyType.value);
            }
        } catch { return; }    
    }

    void CloseCosmetics()
    {
        cosmeticsRoot.SetActive(false);
        camRot.SetActive(true);
    }

    public void OpenSettings()
    {
        SettingsUIManager.instance.ShowSettings();
    }

    public void PasteInJoin()
    {
        lobbyCodeInput.text = GUIUtility.systemCopyBuffer;
    }
}
