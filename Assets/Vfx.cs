using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    private void FixedUpdate()
    {
        if (!_particleSystem.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
