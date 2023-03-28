using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.IO;
using System;

public class SettingsUIManager : MonoBehaviour
{
    public GameObject visRoot;
    public static SettingsUIManager instance;
    [Header("General")]
    public Toggle discord;
    [Header("Screen Res")]
    public TMP_Dropdown resolutionDropdown;
    public Resolution[] resolutions;
    public TMP_Dropdown monitorDropdown;
    public GameObject restartPrompt;
    public List<string> options = new List<string>();
    public int currentResIndex = 0;
    public Slider refreshRateSlider;
    public TextMeshProUGUI refreshRateText;
    [Header("Rendering")]
    public TMP_Dropdown qualityDropdown;
    [Header("Audio")]
    public AudioMixer Master;
    public Slider MasterSlider;
    public Slider AmbientSlider;
    public Slider SoundEffectsSlider;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        resolutions = Screen.resolutions;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string setting = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(setting);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResIndex = i;
            }
        }

        if (PlayerPrefs.HasKey("DISCORD_RP"))
        {
            if (PlayerPrefs.GetInt("DISCORD_RP") == 1)
            {
                discord.isOn = true;
            } else
            {
                discord.isOn = false;
            }
        }
        if (PlayerPrefs.HasKey("DES_MASTERSOUND"))
        {
            MasterSlider.value = PlayerPrefs.GetFloat("DES_MASTERSOUND");
            Master.SetFloat("MasterVol", PlayerPrefs.GetFloat("DES_MASTERSOUND"));
        }
        if (PlayerPrefs.HasKey("DES_AMBSOUND"))
        {
            AmbientSlider.value = PlayerPrefs.GetFloat("DES_AMBSOUND");
            Master.SetFloat("AmbientVol", PlayerPrefs.GetFloat("DES_AMBSOUND"));
        }
        if (PlayerPrefs.HasKey("DES_SFXSOUND"))
        {
            SoundEffectsSlider.value = PlayerPrefs.GetFloat("DES_SFXSOUND");
            Master.SetFloat("SoundEffectsVol", PlayerPrefs.GetFloat("DES_SFXSOUND"));
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

        List<string> monitors = new List<string>();

        for (int i = 0; i < Display.displays.Length; i++)
        {
            monitors.Add(i.ToString());
        }

        monitorDropdown.ClearOptions();
        monitorDropdown.AddOptions(monitors);
        monitorDropdown.value = PlayerPrefs.GetInt("UnitySelectMonitor");
        monitorDropdown.RefreshShownValue();

        if (Screen.currentResolution.refreshRateRatio.value > refreshRateSlider.maxValue)
        {
            refreshRateSlider.maxValue = (float)Screen.currentResolution.refreshRateRatio.value;
        }

        if (PlayerPrefs.HasKey("DES_FRAMERATE"))
        {
            refreshRateSlider.value = PlayerPrefs.GetInt("DES_FRAMERATE");
            refreshRateText.text = refreshRateSlider.value.ToString();
            Application.targetFrameRate = PlayerPrefs.GetInt("DES_FRAMERATE");
        }
        else
        {
            refreshRateSlider.value = (float)Screen.currentResolution.refreshRateRatio.value;
        }

        if (PlayerPrefs.HasKey("QUALITY"))
        {
            qualityDropdown.value = PlayerPrefs.GetInt("QUALITY");
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QUALITY"));
        }
    }

    public void ShowSettings()
    {
        visRoot.SetActive(true);
    }

    public void OnResChange(int value)
    {
        Screen.SetResolution(resolutions[value].width, resolutions[value].height, FullScreenMode.FullScreenWindow, Screen.currentResolution.refreshRateRatio);
    }

    public void OnMonitorChange()
    {
        int value = monitorDropdown.value;

        if (value != PlayerPrefs.GetInt("UnitySelectMonitor"))
        {
            PlayerPrefs.SetInt("UnitySelectMonitor", value);
            restartPrompt.SetActive(true);
        }
    }

    public void OnRefreshRateChange()
    {
        PlayerPrefs.SetInt("DES_FRAMERATE", (int)refreshRateSlider.value);
        Application.targetFrameRate = PlayerPrefs.GetInt("DES_FRAMERATE");
        refreshRateText.text = refreshRateSlider.value.ToString();
    }

    public void OnDiscordToggle()
    {
        if (discord.isOn)
        {
            PlayerPrefs.SetInt("DISCORD_RP", 1);
        } else
        {
            PlayerPrefs.SetInt("DISCORD_RP", 0);
        }
    }

    public void OnQualityChange(int value)
    {
        QualitySettings.SetQualityLevel(value);
        PlayerPrefs.SetInt("QUALITY", value);
    }

    public void OnMasterVolChanged(float value)
    {
        Master.SetFloat("MasterVol", value);
        PlayerPrefs.SetFloat("DES_MASTERSOUND", value);
    }
    public void OnAmbientVolChanged(float value)
    {
        Master.SetFloat("AmbientVol", value);
        PlayerPrefs.SetFloat("DES_AMBSOUND", value);
    }
    public void OnSfxVolChanged(float value)
    {
        Master.SetFloat("SoundEffectsVol", value);
        PlayerPrefs.SetFloat("DES_SFXSOUND", value);
    }

    public void RestartGame()
    {
#if UNITY_STANDALONE
        try
        {
            /*
            ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(Directory.GetCurrentDirectory(), "Dungeon of Guns.exe"));
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            startInfo.UseShellExecute = false;

            Debug.Log(startInfo.WorkingDirectory);
            Debug.Log(Path.Combine(Directory.GetCurrentDirectory(), "Dungeon of Guns.exe"));

            Process.Start(startInfo);
            */

            //dont ask why the above didn't work but this did, i couldn't tell ya
            //(it's because unity didn't move the Process class to il2cpp
            Application.OpenURL("file://" + Path.Combine(Directory.GetCurrentDirectory(), "Dungeon of Guns.exe"));

            Application.Quit();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
#endif
    }
}
