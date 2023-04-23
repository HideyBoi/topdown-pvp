using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFeedItem : MonoBehaviour
{
    CanvasGroup canvasGroup;

    float fadeTime = 5f;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (canvasGroup != null)
        {
            fadeTime -= Time.deltaTime;
            if (fadeTime < 1) {
                canvasGroup.alpha = fadeTime;
            }
            if (fadeTime < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
