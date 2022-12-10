using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIButton : MonoBehaviour
{

    public AudioSource click;
    public AudioSource highlight;

    public void PlayClick()
    {
        click.Play();
    }
    
    public void PlayHighlight()
    {
        highlight.Play();
    }
}
