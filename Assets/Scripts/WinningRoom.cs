using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinningRoom : MonoBehaviour
{
    public static WinningRoom instance;

    public GameObject winCamOrbit;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else { Destroy(gameObject); }
    }

    public void Win()
    {
        Debug.Log("Win sequence started.");
        StartCoroutine("Sequence");
    }

    IEnumerator Sequence()
    {
        winCamOrbit.SetActive(true);
        HealthManager.localHealthManager.playingHUD.SetActive(false);
        HealthManager.localHealthManager.deathUI.SetActive(false);
        LocalPlayerController.instance.cam.gameObject.SetActive(false);
        yield return new WaitForSeconds(3f);
        LoadingScreen.instance.LoadLevel("MainMenu");
    }
}
