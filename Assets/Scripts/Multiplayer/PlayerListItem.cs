using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerListItem : MonoBehaviour
{

    string username;
    public ushort id;

    public TMP_Text text;

    public void UpdateUI(string _username, ushort _id)
    {
        username = _username;
        id = _id;

        text.text = $"<b>{username}";
    }
}
