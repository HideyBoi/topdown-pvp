using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Riptide;
using Riptide.Utils;

public class GroundItem : MonoBehaviour
{
    public GameObject sfx;

    float speed = 120;
    public Transform pivot;

    public int id;

    public InventoryItem currentItem;
    public int currentItemId;

    public MeshFilter gunFilter;
    public MeshRenderer gunRenderer;
    public MeshRenderer sheildRenderer;

    public Material generic;
    public Material rare;
    public Material legendary;

    public TMP_Text nameTex;
    public TMP_Text ammoTex;

    public bool networkSpawned = false;

    private void Awake()
    {    
        GameManager.instance.AddItem(this);
    }

    private void FixedUpdate()
    {
        pivot.rotation = Quaternion.Euler(0, pivot.rotation.eulerAngles.y + Time.fixedDeltaTime * speed, 0);
    }

    public void UpdateItem(InventoryItem item)
    {
        currentItem.weapon = item.weapon;
        nameTex.text = item.weapon.gunName;
        currentItem.ammoCount = item.ammoCount;
        ammoTex.text = item.ammoCount + " bullets";

        gunFilter.mesh = item.weapon.gunMesh;
        gunRenderer.material = item.weapon.gunMaterial;

        switch(item.weapon.rarity)
        {
            case Weapon.Rarity.generic:
                sheildRenderer.material = generic;
                break;
            case Weapon.Rarity.rare:
                sheildRenderer.material = rare;
                break;
            case Weapon.Rarity.legendary:
                sheildRenderer.material = legendary;
                break;
        }

        if (!networkSpawned)
        {
            id = (int)Random.Range(0, 2147483646);

            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.spawnItem);
            msg.AddInt(id);
            msg.AddVector3(transform.position);
            msg.AddInt(currentItem.weapon.id);
            NetworkManager.instance.Client.Send(msg);

            Debug.Log($"[Item] gun({currentItem.weapon.gunName}) with ID:{id} was spawned locally.");
        }
    }

    public void Pickup(bool isFromNetwork)
    {

        GameObject sfxOBJ = Instantiate(sfx, transform.position, Quaternion.identity);
        SoundEffect effect = sfxOBJ.GetComponent<SoundEffect>();
        effect.PlaySound(currentItem.weapon.pickupSound, 20, 0.7f);
        Destroy(GetComponent<BoxCollider>());

        if (!isFromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.pickUpItem);
            msg.AddInt(id);
            NetworkManager.instance.Client.Send(msg);
        }

        Debug.Log($"[Item] Gun({currentItem.weapon.gunName}) with ID:{id} was picked up locally.");
        GetComponent<Animator>().Play("Destroy");
    }
    public void Remove()
    {
        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.pickUpItem);
        msg.AddInt(id);

        NetworkManager.instance.Client.Send(msg);

        GetComponent<Animator>().Play("Destroy");
    }

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}
