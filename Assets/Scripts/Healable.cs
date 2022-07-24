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

    public TMP_Text nameTex;
    public TMP_Text subTex;

    public bool networkSpawned = false;

    public Transform pivot;

    public int id;

    public float speed = 120;

    private void Awake()
    {
        GameManager.instance.AddHealItem(this);
    }

    private void Start()
    {
        if (!networkSpawned)
        {
            id = (int)Random.Range(0, 696969696969);

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
            NetworkManager.instance.Client.Send(msg);
        }

        if (type == HealType.Syringe)
        {
            nameTex.text = "Syringe";
            subTex.text = "+ 30 HP";
        } else
        {
            nameTex.text = "Medkit";
            subTex.text = "+150hp";
        }
    }

    private void FixedUpdate()
    {
        pivot.rotation = Quaternion.Euler(0, pivot.rotation.eulerAngles.y + Time.fixedDeltaTime * speed, 0);
    }

    public void Pickup(bool fromNetwork)
    {
        //GameObject sfxOBJ = Instantiate(sfx, transform.position, Quaternion.identity);
        //SoundEffect effect = sfxOBJ.GetComponent<SoundEffect>();
        //effect.PlaySound(currentItem.weapon.pickupSound);
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
