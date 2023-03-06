using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalMenu : MonoBehaviour
{
    private Controls controls;

    public GameObject leaveHost;
    public GameObject leaveNormal;
    public GameObject settings;

    public GameObject MainMenu;
    public GameObject Lobby;
    public GameObject InGame;
    public GameObject InGameAlone;

    private void Awake()
    {
        controls = new Controls();

        controls.Player.UniMenu.performed += _ => OpenUniversalMenu();
    }

    void OpenUniversalMenu()
    {
        switch (SceneManager.GetActiveScene().name) 
        {
            case "MainMenu":
                if (MainUIManager.instance.currentLobby == null)
                {
                    MainMenu.SetActive(true);
                } else
                {
                    Lobby.SetActive(true);
                }
                break;
            case "Game":
                if (NetworkManager.instance.Server.IsRunning)
                {
                    if (NetworkManager.instance.connectedPlayers.Count == 1)
                    {
                        InGameAlone.SetActive(true);
                    }
                    else
                    {
                        InGame.SetActive(true);
                    }
                } else
                {
                    settings.SetActive(true);
                }
                break;
        }
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }

    public void LeaveGame()
    {
        if (NetworkManager.instance.Server.IsRunning)
        {
            leaveHost.SetActive(true);
        } else
        {
            leaveNormal.SetActive(true);
        }
    }

    public void ConfirmLeaveGame()
    {
        NetworkManager.instance.LeaveGame();
    }

    public void WinGame()
    {
        GameManager.instance.endedGame = true;
        ushort winningId = 0;

        foreach (var player in GameManager.instance.playersInGame)
        {
            winningId = player.id;
        }

        WinningRoom.instance.Win(winningId);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
