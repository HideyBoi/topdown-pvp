using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using TMPro;

public class LobbyManager : MonoBehaviour
{

    public GameObject settingsPanel;

    public GameObject disconnectPrompt;
    public TMP_Text disconnectText;

    public TMP_Text readyText;
    bool isReady;

    private void Awake()
    {
        if (NetworkManager.instance.Server.IsRunning)
        {
            settingsPanel.SetActive(true);
        }

        NetworkManager.instance.readyPlayers = new List<ushort>();
        NetworkManager.instance.mapReadyPlayers = new List<ushort>();

        NetworkManager.instance.gameIsStarted = false;
        NetworkManager.instance.stillInLobby = true;
        NetworkManager.instance.isDoneLoading = false;
    }

    public void ReadyUp()
    {
        Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.readyUp, shouldAutoRelay: true);
        msg.AddBool(!isReady);
        msg.AddUShort(NetworkManager.instance.Client.Id);
        NetworkManager.instance.Client.Send(msg);

        NetworkManager.instance.PlayerReadyUp(!isReady, NetworkManager.instance.Client.Id);

        isReady = !isReady;

        if (isReady)
        {
            readyText.text = "Unready";
        } else
        {
            readyText.text = "Ready!";
        }
    }

    public void DisconnectButtonPress()
    {
        disconnectPrompt.SetActive(true);
        if (NetworkManager.instance.Server.IsRunning)
        {
            disconnectText.text = "Are you sure you want to disconnect all players and go back to main menu?";
        }
        else
        {
            disconnectText.text = "Are you sure you want to disconnect from the host and go back to main menu?";
        }
    }

    public void DisconnectPromptDecline()
    {
        disconnectPrompt.SetActive(false);
    }

    public void DisconnectPromptAccept()
    {
        NetworkManager.instance.LeaveGame();
    }
}
