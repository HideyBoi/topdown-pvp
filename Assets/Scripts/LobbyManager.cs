using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class LobbyManager : MonoBehaviour
{

    public GameObject startButton;

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
}
