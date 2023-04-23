using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ErrorPrompt : MonoBehaviour
{

    public TextMeshProUGUI errorBody;
    public GameObject visualRoot;

    public static ErrorPrompt instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(transform.parent.gameObject);
    }

    public static void ShowError(string errorBodyText)
    {
        Debug.LogError(errorBodyText);
        instance.errorBody.text = errorBodyText;
        instance.visualRoot.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
