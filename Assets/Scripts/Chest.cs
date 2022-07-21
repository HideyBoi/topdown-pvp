using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Chest : MonoBehaviour
{
    public Vector3 chestId;
    public GameObject groundItem;
    public GameObject sfx;
    public AudioClip chestOpenSound;

    GameManager gm;

    public Transform weaponSpawnPos;

    private void Awake()
    {
        chestId = transform.position;

        gm = GameManager.instance;
        gm.AddChest(this);
    }

    public void Open(bool isFromNetwork)
    {
        GetComponent<Animator>().Play("Open");
        Instantiate(sfx, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(chestOpenSound);
        if (!isFromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.openChest, shouldAutoRelay: true);
            msg.AddVector3(chestId);
            NetworkManager.instance.Client.Send(msg);

            GameManager.Loot loot = gm.GenerateLoot();

            InventoryItem inv = new InventoryItem();
            inv.weapon = loot.weapon;
            inv.ammoCount = loot.weapon.maxAmmoCount;

            GameObject item = Instantiate(groundItem, weaponSpawnPos.position, Quaternion.identity);
            item.GetComponent<GroundItem>().UpdateItem(inv);
            item.GetComponent<GroundItem>().networkSpawned = false;
        }  
    }
}
