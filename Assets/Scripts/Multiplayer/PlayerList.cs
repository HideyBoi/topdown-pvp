using System.Collections.Generic;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
    NetworkManager networkManager;

    public List<PlayerListItem> items = new List<PlayerListItem>();

    public GameObject listItem;
    public Transform parentItem;

    private void FixedUpdate()
    {
        if (networkManager == null)
            networkManager = NetworkManager.instance;

        foreach (var player in networkManager.connectedPlayers)
        {
            bool exists = false;

            foreach (var item in items)
            {
                if (item.id == player.id)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                GameObject curItem = Instantiate(listItem, parentItem);
                curItem.GetComponent<PlayerListItem>().UpdateUI(player.name, player.id);
                items.Add(curItem.GetComponent<PlayerListItem>());
            }           
        }

        foreach (var item in items)
        {
            bool exists = false;

            foreach (var connectedPlayer in networkManager.connectedPlayers)
            {
                if (item.id == connectedPlayer.id)
                {
                    exists = true;
                }
            }

            if (!exists)
            {
                items.Remove(item);
                Destroy(item.gameObject);
                return;
            }
        }

        foreach (var item in items)
        {
            bool ready = false;

            foreach (var player in networkManager.playersReadyToStart)
            {
                if (player == item.id)
                {
                    ready = true;
                }
            }

            item.ChangeReady(ready);
        }
    }
}
