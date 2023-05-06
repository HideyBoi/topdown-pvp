using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathmatchCountdown : MonoBehaviour
{
    [SerializeField] DeathmatchRoom room;
    [SerializeField] AudioSource tick;
    [SerializeField] AudioSource done;

    public void Tickdown()
    {
        room.TickDown();
    }

    public void TickSound()
    {
        tick.Play();
    }

    public void StartMatch()
    {
        done.Play();
    }
}
