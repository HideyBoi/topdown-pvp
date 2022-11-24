using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUIManager : MonoBehaviour
{


    [Header("Screen Res")]
    public TMP_Dropdown resolutionDropdown;
    public Resolution[] resolutions;
    public List<string> options = new List<string>();
    public int currentResIndex = 0;
    public Slider refreshRateSlider;
    public TextMeshProUGUI refreshRateText;

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

        if (Screen.currentResolution.refreshRate > refreshRateSlider.maxValue)
        {
            refreshRateSlider.maxValue = Screen.currentResolution.refreshRate;
        }

        refreshRateSlider.value = Screen.currentResolution.refreshRate;
    }

    public void OnResChange(int value)
    {
        Screen.SetResolution(resolutions[value].width, resolutions[value].height, true, Screen.currentResolution.refreshRate);
    }

    public void OnRefreshRateChange()
    {
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen, (int)refreshRateSlider.value);
        refreshRateText.text = refreshRateSlider.value.ToString();
    }
}
