using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class RulesManager : MonoBehaviour
{

    public static RulesManager instance;

    public int lives = 3;
    public bool dropLootOnEveryDeath;
    public bool giveStartingStatsOnDropLoot;
    public Vector2 mapSize;
    public int maxHealth;
    public int startingSyringes;
    public int startingMedkits;
    public int startingLightAmmo;
    public int startingMediumAmmo;
    public int startingHeavyAmmo;
    public int startingShellsAmmo;

    private void Awake()
    {
        instance = this;
    }

    public void RulesUpdated()
    {
        Debug.Log("[Rules Manager] Rules updated!");

        if (PlayerPrefs.HasKey("MAP_SIZE"))
        {
            mapSize = new Vector2(PlayerPrefs.GetInt("MAP_SIZE"), PlayerPrefs.GetInt("MAP_SIZE"));
        }

        if (PlayerPrefs.HasKey("LIFE_COUNT"))
        {
            lives = PlayerPrefs.GetInt("LIFE_COUNT");
            if (PlayerPrefs.GetInt("INFINITE_LIVES") == 1)
            {
                lives = -1;
            }
        }

        if (PlayerPrefs.HasKey("DROP_LOOT_ON_DEATH"))
        {
            if (PlayerPrefs.GetInt("DROP_LOOT_ON_DEATH") == 1)
            {
                dropLootOnEveryDeath = true;
            }
            else
            {
                dropLootOnEveryDeath = false;
            }
        }

        if (PlayerPrefs.HasKey("GIVE_STARTING_LOOT_ON_DROP_WEAPONS"))
        {
            if (PlayerPrefs.GetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS") == 1)
            {
                giveStartingStatsOnDropLoot = true;
            }
            else
            {
                giveStartingStatsOnDropLoot = false;
            }
        }

        if (PlayerPrefs.HasKey("MAX_HEALTH"))
        {
            maxHealth = PlayerPrefs.GetInt("MAX_HEALTH");
        }

        if (PlayerPrefs.HasKey("STARTING_SYRINGES"))
        {
            startingSyringes = PlayerPrefs.GetInt("STARTING_SYRINGES");
        }

        if (PlayerPrefs.HasKey("STARTING_MEDKITS"))
        {
            startingMedkits = PlayerPrefs.GetInt("STARTING_MEDKITS");
        }

        if (PlayerPrefs.HasKey("STARTING_LIGHTAMMO"))
        {
            startingLightAmmo = PlayerPrefs.GetInt("STARTING_LIGHTAMMO");
        }

        if (PlayerPrefs.HasKey("STARTING_MEDIUMAMMO"))
        {
            startingMediumAmmo = PlayerPrefs.GetInt("STARTING_MEDIUMAMMO");
        }

        if (PlayerPrefs.HasKey("STARTING_HEAVYAMMO"))
        {
            startingHeavyAmmo = PlayerPrefs.GetInt("STARTING_HEAVYAMMO");
        }

        if (PlayerPrefs.HasKey("STARTING_SHELLS"))
        {
            startingShellsAmmo = PlayerPrefs.GetInt("STARTING_SHELLS");
        }

        if (NetworkManager.instance.Client != null)
        {
            SendRuleChangesToPlayers();
        } else
        {
            //Debug.Log("[Rules Manager] Local client has not connected to local server yet, rules have not been sent to remote clients.");
        }
        
    }

    public void SendRuleChangesToPlayers()
    {
        //Debug.Log("[Rules Manager] Sending rules to players.");
        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.rules);

        msg.AddInt(lives);
        msg.AddBool(dropLootOnEveryDeath);
        msg.AddBool(giveStartingStatsOnDropLoot);
        msg.AddVector2(mapSize);
        msg.AddInt(maxHealth);
        msg.AddInt(startingSyringes);
        msg.AddInt(startingMedkits);
        msg.AddInt(startingLightAmmo);
        msg.AddInt(startingMediumAmmo);
        msg.AddInt(startingHeavyAmmo);
        msg.AddInt(startingShellsAmmo);

        NetworkManager.instance.Client.Send(msg);
        Debug.Log("[Rules Manager] Sent rules.");
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.rules)]
    static void NewRules(Message msg)
    {
        //Debug.Log("[Rules Manager] Recieved new rules.");

        instance.lives = msg.GetInt();
        PlayerPrefs.SetInt("LIFE_COUNT", instance.lives);
        instance.dropLootOnEveryDeath = msg.GetBool();
        if (instance.dropLootOnEveryDeath) { PlayerPrefs.SetInt("DROP_LOOT_ON_DEATH", 1); }
        else { PlayerPrefs.SetInt("DROP_LOOT_ON_DEATH", 0); }
        instance.giveStartingStatsOnDropLoot = msg.GetBool();
        if (instance.giveStartingStatsOnDropLoot) { PlayerPrefs.SetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS", 1); }
        else { PlayerPrefs.SetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS", 0); }
        instance.mapSize = msg.GetVector2();
        PlayerPrefs.SetInt("MAP_SIZE", (int)instance.mapSize.x);
        instance.maxHealth = msg.GetInt();
        PlayerPrefs.SetInt("MAX_HEALTH", instance.maxHealth);
        instance.startingSyringes = msg.GetInt();
        PlayerPrefs.SetInt("STARTING_SYRINGES", instance.startingSyringes);
        instance.startingMedkits = msg.GetInt();
        PlayerPrefs.SetInt("STARTING_MEDKITS", instance.startingMedkits);
        instance.startingLightAmmo = msg.GetInt();
        PlayerPrefs.SetInt("STARTING_LIGHTAMMO", instance.startingLightAmmo);
        instance.startingMediumAmmo = msg.GetInt();
        PlayerPrefs.SetInt("STARTING_MEDIUMAMMO", instance.startingMediumAmmo);
        instance.startingHeavyAmmo = msg.GetInt();
        PlayerPrefs.SetInt("STARTING_HEAVYAMMO", instance.startingHeavyAmmo);
        instance.startingShellsAmmo = msg.GetInt();
        PlayerPrefs.SetInt("STARTING_SHELLS", instance.startingShellsAmmo);

        GameSettingsManager.instance.LoadValues();
    }
}
