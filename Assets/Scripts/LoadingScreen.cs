using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{

    public static LoadingScreen instance;
    public Animator animator;

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
        animator.Play("fadeToBlack");
        StartCoroutine("LoadAScene", sceneName);
    }

    IEnumerator LoadAScene(string name)
    {
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadSceneAsync(name);
    }

    public void SceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
        animator.Play("fadeFromBlack");
    }

}
