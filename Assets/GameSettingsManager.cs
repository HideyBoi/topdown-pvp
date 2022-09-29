using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Riptide;
using Riptide.Utils;

public class GameSettingsManager : MonoBehaviour
{
    public TMP_InputField mapSize;
    public Toggle autoMapSize;
    public GameObject mapError;
    public int defaultMapSize = 5;
    public bool defaultAutoMapSize = true;

    public TMP_InputField lives;
    public Toggle infiniteLives;
    public GameObject livesError;
    public int defaultLivesCount = 3;
    public bool defaultInfiniteLives = false;

    public Toggle dropLootOnDeath;
    public bool dropLootOnDeathDefault = true;

    public Toggle giveStartingLootOnDeath;
    public bool giveStartingLootOnDeathDefault = true;

    public TMP_InputField maxHealth;
    public GameObject maxHealthError;
    public int defaulltMaxHealth = 150;

    public TMP_InputField startingSyringes;
    public GameObject startingSyringesError;
    public int defaultSyringeCount = 0;

    public TMP_InputField startingMedkits;
    public GameObject startingMedkitsError;
    public int defaultMedkitsCount = 0;

    public TMP_InputField startingLightAmmo;
    public GameObject startingLightAmmoError;
    public int defaultStartingLightAmmo = 35;

    public TMP_InputField startingMediumAmmo;
    public GameObject startingMediumAmmoError;
    public int defaultStartingMediumAmmo = 35;

    public TMP_InputField startingHeavyAmmo;
    public GameObject startingHeavyAmmoError;
    public int defaultStartingHeavyAmmo = 12;

    public TMP_InputField startingShellAmmo;
    public GameObject startingShellAmmoError;
    public int defaultStartingShell = 8;

    private void Awake()
    {
        LoadValues();
    }

    public void ValuesUpdated()
    {
        try
        {
            PlayerPrefs.SetInt("MAP_SIZE", int.Parse(mapSize.text));
            mapError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("MAP SIZE is invalid!");
            mapError.SetActive(true);
        }

        if (autoMapSize.isOn)
        {
            PlayerPrefs.SetInt("AUTO_MAP_SIZE", 1);
        } else
        {
            PlayerPrefs.SetInt("AUTO_MAP_SIZE", 0);
        }

        try
        {
            PlayerPrefs.SetInt("LIFE_COUNT", int.Parse(lives.text));
            livesError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("LIFE COUNT is invalid");
            livesError.SetActive(true);
        }

        if (infiniteLives.isOn)
        {
            PlayerPrefs.SetInt("INFINITE_LIVES", 1);
        }
        else
        {
            PlayerPrefs.SetInt("INFINITE_LIVES", 0);
        }

        if (dropLootOnDeath.isOn)
        {
            PlayerPrefs.SetInt("DROP_LOOT_ON_DEATH", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DROP_LOOT_ON_DEATH", 0);
        }

        if (giveStartingLootOnDeath.isOn)
        {
            PlayerPrefs.SetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS", 1);
        }
        else
        {
            PlayerPrefs.SetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS", 0);
        }

        try
        {
            PlayerPrefs.SetInt("MAX_HEALTH", int.Parse(maxHealth.text));
            maxHealthError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("MAX HEALTH is invalid!");
            maxHealthError.SetActive(true);
        }

        try
        {
            PlayerPrefs.SetInt("STARTING_SYRINGES", int.Parse(startingSyringes.text));
            startingSyringesError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("STARTING SYRINGES is invalid!");
            startingSyringesError.SetActive(true);
        }

        try
        {
            PlayerPrefs.SetInt("STARTING_MEDKITS", int.Parse(startingMedkits.text));
            startingMedkitsError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("STARTING MEDKITS is invalid!");
            startingMedkitsError.SetActive(true);
        }

        try
        {
            PlayerPrefs.SetInt("STARTING_LIGHTAMMO", int.Parse(startingLightAmmo.text));
            startingLightAmmoError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("STARTING LIGHTAMMO is invalid!");
            startingLightAmmoError.SetActive(true);
        }

        try
        {
            PlayerPrefs.SetInt("STARTING_MEDIUMAMMO", int.Parse(startingMediumAmmo.text));
            startingMediumAmmoError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("STARTING MEDIUMAMMO is invalid!");
            startingMediumAmmoError.SetActive(true);
        }

        try
        {
            PlayerPrefs.SetInt("STARTING_HEAVYAMMO", int.Parse(startingHeavyAmmo.text));
            startingHeavyAmmoError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("STARTING HEAVY is invalid!");
            startingHeavyAmmoError.SetActive(true);
        }

        try
        {
            PlayerPrefs.SetInt("STARTING_SHELLS", int.Parse(startingShellAmmo.text));
            startingShellAmmoError.SetActive(false);
        }
        catch (FormatException)
        {
            Debug.Log("STARTING SHELLAMMO is invalid!");
            startingShellAmmoError.SetActive(true);
        }
    }

    public void LoadValues()
    {
        if (PlayerPrefs.HasKey("MAP_SIZE"))
        {
            mapError.SetActive(false);
            mapSize.text = PlayerPrefs.GetInt("MAP_SIZE").ToString();
            Debug.Log("GMAING " + PlayerPrefs.GetInt("MAP_SIZE"));
            if (PlayerPrefs.GetInt("AUTO_MAP_SIZE") == 1)
            {
                autoMapSize.isOn = true;
            }
            else
            {
                autoMapSize.isOn = false;
            }
        }

        if (PlayerPrefs.HasKey("LIFE_COUNT"))
        {
            livesError.SetActive(false);
            lives.text = PlayerPrefs.GetInt("LIFE_COUNT").ToString();

            if (PlayerPrefs.GetInt("INFINITE_LIVES") == 1)
            {
                infiniteLives.isOn = true;
            }
            else
            {
                infiniteLives.isOn = false;
            }
        }

        if (PlayerPrefs.HasKey("DROP_LOOT_ON_DEATH"))
        {
            if (PlayerPrefs.GetInt("DROP_LOOT_ON_DEATH") == 1)
            {
                dropLootOnDeath.isOn = true;
            } else
            {
                dropLootOnDeath.isOn = false;
            }
        }

        if (PlayerPrefs.HasKey("GIVE_STARTING_LOOT_ON_DROP_WEAPONS"))
        {
            if (PlayerPrefs.GetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS") == 1)
            {
                giveStartingLootOnDeath.isOn = true;
            } else
            {
                giveStartingLootOnDeath.isOn = false;
            }
        }

        if (PlayerPrefs.HasKey("MAX_HEALTH"))
        {
            maxHealthError.SetActive(false);
            maxHealth.text = PlayerPrefs.GetInt("MAX_HEALTH").ToString();
        }

        if (PlayerPrefs.HasKey("STARTING_SYRINGES"))
        {
            startingSyringes.text = PlayerPrefs.GetInt("STARTING_SYRINGES").ToString();
        }

        if (PlayerPrefs.HasKey("STARTING_MEDKITS"))
        {
            startingMedkits.text = PlayerPrefs.GetInt("STARTING_MEDKITS").ToString();
        }

        if (PlayerPrefs.HasKey("STARTING_LIGHTAMMO"))
        {
            startingLightAmmo.text = PlayerPrefs.GetInt("STARTING_LIGHTAMMO").ToString();
        }

        if (PlayerPrefs.HasKey("STARTING_MEDIUMAMMO"))
        {
            startingMediumAmmo.text = PlayerPrefs.GetInt("STARTING_MEDIUMAMMO").ToString();
        }

        if (PlayerPrefs.HasKey("STARTING_HEAVYAMMO"))
        {
            startingHeavyAmmo.text = PlayerPrefs.GetInt("STARTING_HEAVYAMMO").ToString();
        }

        if (PlayerPrefs.HasKey("STARTING_SHELLAMMO"))
        {
            startingShellAmmo.text = PlayerPrefs.GetInt("STARTING_SHELLAMMO").ToString();
        }

        ValuesUpdated();
    }
}