using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class LoadingScreen : MonoBehaviour
{

    public static LoadingScreen instance;
    public Animator animator;

    public TMP_Text text;

    public bool waitingForGeneration = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += SceneWasLoaded;
    }

    public void LoadLevel(string sceneName)
    {
        text.text = "Loading...";
        animator.Play("fadeToBlack");
        StartCoroutine("LoadAScene", sceneName);
    }

    IEnumerator LoadAScene(string name)
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadSceneAsync(name);
    }

    private void FixedUpdate()
    {
        if (waitingForGeneration)
        {
            text.text = "Generating map...";
            if (NetworkManager.instance.gameIsStarted)
            {
                animator.Play("fadeFromBlack");
                waitingForGeneration = false;
            }
        }
    }

    public void SceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            waitingForGeneration = true;
        }
        else
        {
            animator.Play("fadeFromBlack");
        }
    }
}
