using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CosmeticsUI : MonoBehaviour
{
    int currentHat;
    int currentSkin;

    public CosmeticsHandler cosmeticsHandler;
    public TextMeshProUGUI hatTex;
    public Animator cosmeticsButton;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("DES_HAT"))
        {
            currentHat = PlayerPrefs.GetInt("DES_HAT");
        }
        else
        {
            PlayerPrefs.SetInt("DES_HAT", 0);
        }

        if (PlayerPrefs.HasKey("DES_SKIN"))
        {
            currentSkin = PlayerPrefs.GetInt("DES_SKIN");
        }
        else
        {
            currentSkin = 1;
            PlayerPrefs.SetInt("DES_SKIN", 1);
        }
    }

    public void LoadCosmetics()
    {
        cosmeticsButton.SetTrigger("Normal");

        if (PlayerPrefs.HasKey("DES_HAT"))
        {
            currentHat = PlayerPrefs.GetInt("DES_HAT");
        } else
        {
            PlayerPrefs.SetInt("DES_HAT", 0);
        }
        
        if (PlayerPrefs.HasKey("DES_SKIN"))
        {
            currentSkin = PlayerPrefs.GetInt("DES_SKIN");
        } else
        {
            currentSkin = 1;
            PlayerPrefs.SetInt("DES_SKIN", 1);
        }

        cosmeticsHandler.SetCosmetics(currentHat, currentSkin);
        hatTex.text = CosmeticsManager.i.hats[currentHat].name;
    }

    public void ChangeHat(int dir)
    {
        currentHat += dir;

        if (currentHat == CosmeticsManager.i.hats.Length)
        {
            currentHat = 0;
        } else if (currentHat < 0) {
            currentHat = CosmeticsManager.i.hats.Length - 1;
        }

        PlayerPrefs.SetInt("DES_HAT", currentHat);
        cosmeticsHandler.SetCosmetics(currentHat, currentSkin);
        hatTex.text = CosmeticsManager.i.hats[currentHat].name;
    }

    public void ChangeSkin(int dir)
    {
        currentSkin += dir;

        if (currentSkin == CosmeticsManager.i.skins.Length)
        {
            currentSkin = 0;
        }
        else if (currentSkin < 0)
        {
            currentSkin = CosmeticsManager.i.skins.Length - 1;
        }

        PlayerPrefs.SetInt("DES_SKIN", currentSkin);
        cosmeticsHandler.SetCosmetics(currentHat, currentSkin);
    }
}
