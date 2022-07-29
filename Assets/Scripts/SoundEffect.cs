using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{

    public AudioSource source;

    public void PlaySound(AudioClip sound, float maxDist, float volume)
    {
        source.clip = sound;
        source.volume = volume;
        source.maxDistance = maxDist;
        source.Play();
    }

    private void FixedUpdate()
    {
        if (!source.isPlaying)
        {
            //Destroy(gameObject);
        }
    }
}
