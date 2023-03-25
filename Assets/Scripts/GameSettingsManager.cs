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
    public static GameSettingsManager instance;

    public LobbyManager lobby;

    public GameObject readyButton;
    public GameObject rulesCover;

    public GameObject resetPrompt;

    public TMP_InputField mapSize;
    public GameObject mapError;
    public int defaultMapSize = 5;

    public TMP_InputField lives;
    public Toggle infiniteLives;
    public GameObject livesError;
    public int defaultLivesCount = 3;
    public bool defaultInfiniteLives = false;

    public Toggle dropLootOnDeath;
    public bool dropLootOnDeathDefault = true;

    public Toggle giveStartingLootOnDeath;
    public bool giveStartingLootOnDeathDefault = true;

    public Toggle doWeaponSlowdown;
    public bool doWeaponSlowdownDefault = false;

    public Toggle doWeaponDropoff;
    public bool doWeaponDropoffDefault = true;

    public TMP_InputField maxHealth;
    public GameObject maxHealthError;
    public int defaulltMaxHealth = 150;

    public TMP_InputField startingSyringes;
    public GameObject startingSyringesError;
    public int defaultSyringeCount = 0;

    public TMP_InputField startingMedkits;
    public GameObject startingMedkitsError;
    public int defaultMedkitsCount = 0;

    public TMP_InputField ammoMultiplier;
    public GameObject ammoMultiplierError;
    public float defaultAmmoMultiplier = 1;

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

    bool loading = false;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("MAX_HEALTH"))
        {
            LoadValues();
        } else
        {
            Reset();
        }
        RulesManager.instance.RulesUpdated();
        instance = this;
    }

    private void FixedUpdate()
    {
        if (NetworkManager.instance.Server.IsRunning)
        {
            rulesCover.SetActive(lobby.isReady);
        } else
        {
            rulesCover.SetActive(true);
        }
    }

    public void ValuesUpdated()
    {
        //Debug.Log("[Game Settings Manager] Values changed, saving game settings.");

        if (lobby.isReady || !NetworkManager.instance.Server.IsRunning)
        {
            LoadValues();
            return;
        }

        if (loading)
        {
            return;
        }

        bool failure = false;

        try
        {
            if (int.Parse(mapSize.text) > 2)
            {
                PlayerPrefs.SetInt("MAP_SIZE", int.Parse(mapSize.text));
                mapError.SetActive(false);
            } else
            {
                failure = true;
                Debug.Log("MAP SIZE is invalid!");
                mapError.SetActive(true);
            }           
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("MAP SIZE is invalid!");
            mapError.SetActive(true);
        }

        try
        {
            if (int.Parse(lives.text) > 0)
            {
                PlayerPrefs.SetInt("LIFE_COUNT", int.Parse(lives.text));
                livesError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("LIFE COUNT is invalid!");
                livesError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
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

        if (doWeaponSlowdown.isOn)
        {
            PlayerPrefs.SetInt("DO_WEAPON_SLOWDOWN", 1);
        } else
        {
            PlayerPrefs.SetInt("DO_WEAPON_SLOWDOWN", 0);
        }

        if (doWeaponDropoff.isOn)
        {
            PlayerPrefs.SetInt("DO_WEAPON_DROPOFF", 1);
        } else
        {

            PlayerPrefs.SetInt("DO_WEAPON_DROPOFF", 0);
        }

        try
        {
            if (int.Parse(maxHealth.text) > 0)
            {
                PlayerPrefs.SetInt("MAX_HEALTH", int.Parse(maxHealth.text));
                maxHealthError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("MAX HEALTH is invalid!");
                maxHealthError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("MAX HEALTH is invalid!");
            maxHealthError.SetActive(true);
        }

        try
        {
            if (float.Parse(ammoMultiplier.text) >= 1 && float.Parse(ammoMultiplier.text) <= 500)
            {
                PlayerPrefs.SetFloat("AMMO_MULTIPLIER", float.Parse(ammoMultiplier.text));
                ammoMultiplierError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("AMMO MULTIPLIER is invalid!");
                ammoMultiplierError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING SYRINGES is invalid!");
            startingSyringesError.SetActive(true);
        }

        try
        {
            if (int.Parse(startingSyringes.text) >= 0)
            {
                PlayerPrefs.SetInt("STARTING_SYRINGES", int.Parse(startingSyringes.text));
                startingSyringesError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("STARTING SYRINGES is invalid!");
                startingSyringesError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING SYRINGES is invalid!");
            startingSyringesError.SetActive(true);
        }

        try
        {
            if (int.Parse(startingMedkits.text) >= 0)
            {
                PlayerPrefs.SetInt("STARTING_MEDKITS", int.Parse(startingMedkits.text));
                startingMedkitsError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("STARTING MEDKITS is invalid!");
                startingMedkitsError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING MEDKITS is invalid!");
            startingMedkitsError.SetActive(true);
        }

        try
        {
            if (int.Parse(startingLightAmmo.text) >= 0)
            {
                PlayerPrefs.SetInt("STARTING_LIGHTAMMO", int.Parse(startingLightAmmo.text));
                startingLightAmmoError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("STARTING LIGHTAMMO is invalid!");
                startingLightAmmoError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING LIGHTAMMO is invalid!");
            startingLightAmmoError.SetActive(true);
        }

        try
        {
            if (int.Parse(startingMediumAmmo.text) >= 0)
            {
                PlayerPrefs.SetInt("STARTING_MEDIUMAMMO", int.Parse(startingMediumAmmo.text));
                startingMediumAmmoError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("STARTING MEDIUMAMMO is invalid!");
                startingMediumAmmoError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING MEDIUMAMMO is invalid!");
            startingMediumAmmoError.SetActive(true);
        }

        try
        {
            if (int.Parse(startingHeavyAmmo.text) >= 0)
            {
                PlayerPrefs.SetInt("STARTING_HEAVYAMMO", int.Parse(startingHeavyAmmo.text));
                startingHeavyAmmoError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("STARTING HEAVYAMMO is invalid!");
                startingHeavyAmmoError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING HEAVYAMMO is invalid!");
            startingHeavyAmmoError.SetActive(true);
        }

        try
        {
            if (int.Parse(startingShellAmmo.text) >= 0)
            {
                PlayerPrefs.SetInt("STARTING_SHELLS", int.Parse(startingShellAmmo.text));
                startingShellAmmoError.SetActive(false);
            }
            else
            {
                failure = true;
                Debug.Log("STARTING SHELLS is invalid!");
                startingShellAmmoError.SetActive(true);
            }
        }
        catch (FormatException)
        {
            failure = true;
            Debug.Log("STARTING SHELLS is invalid!");
            startingShellAmmoError.SetActive(true);
        }

        if (failure)
        {
            readyButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            readyButton.GetComponent<Button>().interactable = true;
            RulesManager.instance.RulesUpdated();
        }
    }

    public void LoadValues()
    {
        //Debug.Log("[Game Settings Manager] Loading game settings.");

        loading = true;

        if (PlayerPrefs.HasKey("MAP_SIZE"))
        {
            mapError.SetActive(false);
            mapSize.text = PlayerPrefs.GetInt("MAP_SIZE").ToString();
        } else
        {
            Reset();
            return;
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

        if (PlayerPrefs.HasKey("DO_WEAPON_SLOWDOWN"))
        {
            if (PlayerPrefs.GetInt("DO_WEAPON_SLOWDOWN") == 1)
            {
                doWeaponSlowdown.isOn = true;
            } else
            {

                doWeaponSlowdown.isOn = false;
            }
        }

        if (PlayerPrefs.HasKey("DO_WEAPON_DROPOFF"))
        {
            if (PlayerPrefs.GetInt("DO_WEAPON_DROPOFF") == 1)
            {
                doWeaponDropoff.isOn = true;
            }
            else
            {
                doWeaponDropoff.isOn = false;
            }
        }

        if (PlayerPrefs.HasKey("MAX_HEALTH"))
        {
            maxHealthError.SetActive(false);
            maxHealth.text = PlayerPrefs.GetInt("MAX_HEALTH").ToString();
        }

        if (PlayerPrefs.HasKey("AMMO_MULTIPLIER"))
        {
            ammoMultiplier.text = PlayerPrefs.GetFloat("AMMO_MULTIPLIER").ToString();
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

        if (PlayerPrefs.HasKey("STARTING_SHELLS"))
        {
            startingShellAmmo.text = PlayerPrefs.GetInt("STARTING_SHELLS").ToString();
        }

        if (NetworkManager.instance.Server.IsRunning)
        {
            RulesManager.instance.RulesUpdated();
        }

        loading = false;
    }

    public void Reset()
    {

        //loading || lobby.isReady || 

        //if (!NetworkManager.instance.Server.IsRunning)
       // {
            //LoadValues();
            //return;
        //}
        //Debug.Log("[Game Settings Manager] Saving default values.");

        PlayerPrefs.SetInt("MAP_SIZE", defaultMapSize);

        PlayerPrefs.SetInt("LIFE_COUNT", defaultLivesCount);

        if (defaultInfiniteLives)
        {
            PlayerPrefs.SetInt("INFINITE_LIVES", 1);
        }
        else
        {
            PlayerPrefs.SetInt("INFINITE_LIVES", 0);
        }

        if (dropLootOnDeathDefault)
        {
            PlayerPrefs.SetInt("DROP_LOOT_ON_DEATH", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DROP_LOOT_ON_DEATH", 0);
        }

        if (giveStartingLootOnDeathDefault)
        {
            PlayerPrefs.SetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS", 1);
        }
        else
        {
            PlayerPrefs.SetInt("GIVE_STARTING_LOOT_ON_DROP_WEAPONS", 0);
        }

        if (doWeaponSlowdownDefault)
        {
            PlayerPrefs.SetInt("DO_WEAPON_SLOWDOWN", 1);
        } else
        {
            PlayerPrefs.SetInt("DO_WEAPON_SLOWDOWN", 0);
        }

        if (doWeaponDropoffDefault)
        {
            PlayerPrefs.SetInt("DO_WEAPON_DROPOFF", 1);
        }
        else
        {
            PlayerPrefs.SetInt("DO_WEAPON_DROPOFF", 0);
        }

        PlayerPrefs.SetInt("MAX_HEALTH", defaulltMaxHealth);
        PlayerPrefs.SetFloat("AMMO_MULTIPLIER", defaultAmmoMultiplier);
        PlayerPrefs.SetInt("STARTING_SYRINGES", defaultSyringeCount);
        PlayerPrefs.SetInt("STARTING_MEDKITS", defaultMedkitsCount);
        PlayerPrefs.SetInt("STARTING_LIGHTAMMO", defaultStartingLightAmmo);
        PlayerPrefs.SetInt("STARTING_MEDIUMAMMO", defaultStartingMediumAmmo);
        PlayerPrefs.SetInt("STARTING_HEAVYAMMO", defaultStartingHeavyAmmo);
        PlayerPrefs.SetInt("STARTING_SHELLS", defaultStartingShell);

        LoadValues();
    }
}