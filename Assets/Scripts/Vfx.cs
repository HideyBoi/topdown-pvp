using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class Vfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    [SerializeField] private int particleId;
    public bool networkSpawned = false;

    private void Start()
    {
        if (!networkSpawned && particleId != -1)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.particleEffect);
            msg.AddInt(particleId);
            msg.AddVector3(transform.position);
            msg.AddQuaternion(transform.rotation);
            NetworkManager.instance.Client.Send(msg);
        }
    }

    private void FixedUpdate()
    {
        if (!_particleSystem.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
