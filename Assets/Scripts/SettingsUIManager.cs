using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIManager : MonoBehaviour
{
    public GameObject visRoot;
    public static SettingsUIManager instance;

    [Header("Screen Res")]
    public TMP_Dropdown resolutionDropdown;
    public Resolution[] resolutions;
    public List<string> options = new List<string>();
    public int currentResIndex = 0;
    public Slider refreshRateSlider;
    public TextMeshProUGUI refreshRateText;
    [Header("Rendering")]
    public TMP_Dropdown qualityDropdown;

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

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResIndex;
        resolutionDropdown.RefreshShownValue();

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

    public void OnRefreshRateChange()
    {
        PlayerPrefs.SetInt("DES_FRAMERATE", (int)refreshRateSlider.value);
        Application.targetFrameRate = PlayerPrefs.GetInt("DES_FRAMERATE");
        refreshRateText.text = refreshRateSlider.value.ToString();
    }

    public void OnQualityChange(int value)
    {
        QualitySettings.SetQualityLevel(value);
        PlayerPrefs.SetInt("QUALITY", value);
    }
}
