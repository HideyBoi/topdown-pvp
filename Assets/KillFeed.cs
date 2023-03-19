using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillFeed : MonoBehaviour
{
    public static KillFeed i;

    public GameObject killFeedItem;

    private void Awake()
    {
        i = this;
    }

    public void OnKill(ushort killer, ushort victim, Weapon weapon)
    {
        GameObject newItem = Instantiate(killFeedItem);
        newItem.GetComponent<TextMeshProUGUI>().text = $"{GetPlayerName(killer)} killed {GetPlayerName(killer)} using {weapon.gunName} [{GetWeaponRarity(weapon)}]";
    }

    string GetPlayerName(ushort id)
    {
        foreach (NetworkManager.MultiplayerPlayer player in NetworkManager.instance.connectedPlayers)
        {
            if (player.id == id)
            {
                return player.name;
            }
          
        }
        return "!!PLAYER MISSING!!";
    }

    string GetWeaponRarity(Weapon weapon)
    {
        switch (weapon.rarity)
        {
            case Weapon.Rarity.generic:
                return "Generic";
            case Weapon.Rarity.rare:
                return "Rare";
            case Weapon.Rarity.legendary:
                return "Legendary";
            default:
                return "!!MISSING WEAPON!!";

        }
    }
}
