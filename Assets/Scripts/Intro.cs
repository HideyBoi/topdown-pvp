using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Intro : MonoBehaviour
{
    public TextMeshProUGUI tex;

    public void Done()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void Play()
    {
        GetComponent<AudioSource>().Play();
    }

    private void Awake()
    {
        tex.text = "Version: " + Application.version;
    }
}
