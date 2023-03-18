using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinningRoom : MonoBehaviour
{
    public static WinningRoom instance;

    public GameObject winCamOrbit;

    [Space]
    [Header("UI")]
    public GameObject UI;
    public Animator uiFade;
    public TextMeshPro namePlate;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI livesCount;
    public CosmeticsHandler cosmeticsHandler;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else { Destroy(gameObject); }
    }

    public void Win(ushort winnerId)
    {
        Debug.Log("Win sequence started.");
        StartCoroutine("Sequence", winnerId);
    }

    IEnumerator Sequence(ushort id)
    {
        yield return new WaitForSeconds(1f);
        uiFade.Play("Win");
        yield return new WaitForSeconds(1f);
        winCamOrbit.SetActive(true);
        HealthManager.localHealthManager.playingHUD.SetActive(false);
        HealthManager.localHealthManager.deathUI.SetActive(false);
        LocalPlayerController.instance.cam.gameObject.SetActive(false);
        gameObject.GetComponent<Animator>().Play("Win");

        SetWinUI(id);

        yield return new WaitForSeconds(14f);
        LoadingScreen.instance.LoadLevel("MainMenu");
    }

    void SetWinUI(ushort id)
    {
        UI.SetActive(true);

        int hatId = 0;
        int skinId = 0;

        if (id == NetworkManager.instance.Client.Id)
        {
            foreach (var networkPlayer in NetworkManager.instance.connectedPlayers)
            {
                if (networkPlayer.id == id)
                {
                    namePlate.text = networkPlayer.name;
                    hatId = networkPlayer.hatId;
                    skinId = networkPlayer.skinId;
                }
            }

            killCount.text = HealthManager.localHealthManager.killCount.ToString();

            livesCount.gameObject.SetActive(HealthManager.localHealthManager.lives != -1);
            livesCount.text = HealthManager.localHealthManager.lives.ToString();

            cosmeticsHandler.SetCosmetics(hatId, skinId);

        } else
        {
            foreach (var remotePlayer in GameManager.instance.remotePlayers)
            {
                namePlate.text = remotePlayer._name;
                killCount.text = remotePlayer.healthManager.killCount.ToString();
                livesCount.gameObject.SetActive(remotePlayer.healthManager.lives != -1);
                livesCount.text = remotePlayer.healthManager.lives.ToString();
                cosmeticsHandler.SetCosmetics(remotePlayer.cosmeticsHandler.currHatId, remotePlayer.cosmeticsHandler.currSkinId);
            }
        }
    }
}
