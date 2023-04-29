using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Transports.Steam;
using Riptide.Utils;
using Steamworks;
using TMPro;
using UnityEngine.Networking;

public class MainUIManager : MonoBehaviour
{
    public static MainUIManager instance;

    public TextMeshProUGUI debugVersionInfo;

    public ushort maxPlayers = 8;

    public GameObject lobby;
    public GameObject visRoot;
    public GameObject controlsScreen;
    public GameObject currentLobby;

    public string currentUsername = "";

    public TMP_InputField lobbyCodeInput;
    public TMP_InputField maxPlayersInput;
    public TMP_Dropdown lobbyType;

    public Texture2D cursor;

    public GameObject cosmeticsRoot;
    public GameObject camRot;

    public GameObject updateNotice;
    public TextMeshProUGUI updateNoticeText;

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
        StartCoroutine(GetRequest("https://hideyboi.pages.dev/Update/dungeon-of-guns-main"));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("[Update] Error: " + webRequest.error);
                    ErrorPrompt.ShowError("[Update] Unable to connect to the server to check for updates.");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("[Update] HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("[Update] Got version information successfully.");
                    if (webRequest.downloadHandler.text != Application.version)
                    {
                        Debug.Log("[Update] Versions do not match, showing prompt.");
                        updateNotice.SetActive(true);
                        updateNoticeText.text = $"You are not playing the latest version of this game!\nIn order to play with most other players you will need to download the update from the Itch.io page.\nCurrent Version: {Application.version} Newest Version: {webRequest.downloadHandler.text}";
                    }
                    else
                    {
                        Debug.Log("[Update] Version is up to date!");
                    }
                    break;
            }
        }
    }

    public void ConnectedToLobby()
    {
        currentLobby = Instantiate(lobby);
        controlsScreen.SetActive(false);
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

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
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
