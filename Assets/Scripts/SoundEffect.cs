using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{

    public AudioSource source;

    public void PlaySound(AudioClip sound)
    {
        source.clip = sound;
        source.Play();
    }

    private void FixedUpdate()
    {
        if (!source.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
