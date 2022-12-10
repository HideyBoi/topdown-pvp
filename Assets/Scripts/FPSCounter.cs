using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    TextMeshProUGUI text;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("SHOW_FRAMERATE"))
        {
            //Destroy(gameObject);
        }

        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        text.text = "FPS: " + (1f / Time.unscaledDeltaTime);
    }
}
