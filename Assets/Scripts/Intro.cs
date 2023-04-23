using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class Intro : MonoBehaviour
{
    public TextMeshProUGUI tex;
    public bool enableBetaDRM = false;

    public void Done()
    {
        LoadingScreen.instance.LoadLevel("MainMenu");
    }

    public void Play()
    {
        GetComponent<AudioSource>().Play();
    }

    private void Awake()
    {
        tex.text = "Version: " + Application.version;
        if (enableBetaDRM)
        {
            Debug.Log("[DRM] DRM started!");
            StartCoroutine(GetRequest("https://hideyboi.pages.dev/dungeon-of-guns-beta-ver"));
        }
    }


    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("[DRM] Error: " + webRequest.error);
                    ErrorPrompt.ShowError("[DRM] Unable to connect to the server to verify game version.");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("[DRM] HTTP Error: " + webRequest.error);
                    ErrorPrompt.ShowError("[DRM] Unable to connect to the server to verify game version due to a HTTP error.");
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("[DRM] Got version information successfully.");
                    if (webRequest.downloadHandler.text != Application.version)
                    {
                        Debug.Log("[DRM] Versions do not match, throwing error.");
                        ErrorPrompt.ShowError("[DRM] The playtesting period for this version has ended. Please contact the developer for a new playtesting version and <b>permanently</b> delete this copy.");
                    } else
                    {
                        Debug.Log("[DRM] Version confirmed successfully!");
                    }
                    break;
            }
        }
    }
}
