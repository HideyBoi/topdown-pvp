using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using TMPro;

public class LobbyManager : MonoBehaviour
{

    public GameObject startButton;

    public GameObject disconnectPrompt;
    public TMP_Text disconnectText;

    private void Awake()
    {
        if (NetworkManager.instance.Server.IsRunning)
        {
            startButton.SetActive(true);
        }
    }

    public void StartGame()
    {
        if (NetworkManager.instance.Server.IsRunning)
        {
            startButton.SetActive(false);

            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.startGame, shouldAutoRelay: true);
            msg.AddBool(true);
            NetworkManager.instance.Client.Send(msg);
            NetworkManager.StartGame(msg);
            NetworkManager.instance.stillInLobby = false;
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
