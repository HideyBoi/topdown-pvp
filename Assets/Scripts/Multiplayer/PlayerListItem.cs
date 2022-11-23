using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerListItem : MonoBehaviour
{

    string username;
    public ushort id;

    public TMP_Text text;

    public GameObject kickButton;
    public GameObject readyIndicator;

    public void UpdateUI(string _username, ushort _id)
    {
        username = _username;
        id = _id;

        text.text = $"<b>{username}";

        if (NetworkManager.instance.Server.IsRunning)
        {
            kickButton.SetActive(true);
        }
    }

    public void ChangeReady(bool ready)
    {
        readyIndicator.SetActive(ready);
    }

    public void KickThisPlayer()
    {
        NetworkManager.instance.Server.DisconnectClient(id);
    }
}
