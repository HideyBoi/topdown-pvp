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

    private void Awake()
    {
        tex.text = "This game is in active development.\n\nGame Version: " + Application.version;
    }
}
