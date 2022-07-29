using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Healable : MonoBehaviour
{
    public enum HealType
    {
        Medkit,
        Syringe
    }

    public HealType type;

    public int count = 1;

    public TMP_Text nameTex;
    public TMP_Text subTex;

    public bool networkSpawned = false;

    public Transform pivot;

    public int id;

    public float speed = 120;

    public GameObject sfx;
    public AudioClip pickupSfx;

    private void Awake()
    {
        GameManager.instance.AddHealItem(this);
    }

    private void Start()
    {
        if (!networkSpawned)
        {
            id = Random.Range(0, 2147483646);

            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.spawnHeal, shouldAutoRelay: true);
            msg.AddInt(id);
            msg.AddVector3(transform.position);
            if (type == HealType.Medkit)
            {
                msg.AddInt(1);
            } else
            {
                msg.AddInt(0);
            }
            msg.AddInt(count);
            NetworkManager.instance.Client.Send(msg);
        }

        if (type == HealType.Syringe)
        {
            nameTex.text = "Syringe";
            subTex.text = $"{count}x + 30 HP";
        } else
        {
            nameTex.text = "Medkit";
            subTex.text = $"{count}x +150hp";
        }
    }

    private void FixedUpdate()
    {
        pivot.rotation = Quaternion.Euler(0, pivot.rotation.eulerAngles.y + Time.fixedDeltaTime * speed, 0);
    }

    public void Pickup(bool fromNetwork)
    {
        GameObject sfxOBJ = Instantiate(sfx, transform.position, Quaternion.identity);
        SoundEffect effect = sfxOBJ.GetComponent<SoundEffect>();
        effect.PlaySound(pickupSfx, 1, 30);
        Destroy(GetComponent<BoxCollider>());

        if (!fromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.pickUpHeal, shouldAutoRelay: true);
            msg.AddInt(id);
            NetworkManager.instance.Client.Send(msg);
        }

        GetComponent<Animator>().Play("Destroy");
    }
    public void DestroyThis()
    {
        Destroy(gameObject);
    }

}
