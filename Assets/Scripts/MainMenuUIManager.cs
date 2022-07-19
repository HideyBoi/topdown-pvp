using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuUIManager : MonoBehaviour
{

    public GameObject joinPanel;
    public GameObject hostPanel;

    public GameObject lobbyMenu;
    public GameObject currentLobbyMenu;

    public string playerName;

    public TMP_InputField usernameInput;

    [Header("Join")]
    public TMP_InputField ipInput;
    public TMP_InputField portInput;

    [Header("Host")]
    public TMP_InputField hostPortInput;
    public TMP_InputField maxPlayerInput;

    public void OnJoinButtonClick()
    {
        joinPanel.SetActive(true);
        hostPanel.SetActive(false);
    }

    public void OnJoinStart()
    {
        if (usernameInput.text == "")
            return;
        NetworkManager.instance.mainMenuUIManager = this;
        NetworkManager.instance.JoinGame(ipInput.text, ushort.Parse(portInput.text));
        currentLobbyMenu = Instantiate(lobbyMenu);
    }

    public void OnHostButtonClick()
    {
        joinPanel.SetActive(false);
        hostPanel.SetActive(true);
    }

    public void OnHostStart()
    {
        if (usernameInput.text == "")
            return;
        NetworkManager.instance.mainMenuUIManager = this;
        NetworkManager.instance.HostGame(ushort.Parse(hostPortInput.text), ushort.Parse(maxPlayerInput.text));
        currentLobbyMenu = Instantiate(lobbyMenu);
    }

    public void OnCloseMenus()
    {
        joinPanel.SetActive(false);
        hostPanel.SetActive(false);
    }

    public void OnPlayerNameUpdated()
    {
        playerName = usernameInput.text;
        PlayerPrefs.SetString("PLAYER_LOCAL_USERNAME", playerName);
    }

    public void Awake()
    {
        if (PlayerPrefs.HasKey("PLAYER_LOCAL_USERNAME"))
        {
            usernameInput.text = PlayerPrefs.GetString("PLAYER_LOCAL_USERNAME");
        }

        if (NetworkManager.instance != null)
        {
            if (NetworkManager.instance.Client.IsConnected)
            {
                currentLobbyMenu = Instantiate(lobbyMenu);
            }
        }
    }

    public void Disconnected()
    {
        Destroy(currentLobbyMenu);
        hostPanel.SetActive(false);
        joinPanel.SetActive(false);
    }
}
