using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class Chest : MonoBehaviour
{
    public Vector3 chestId;
    public GameObject groundItem;
    public GameObject ammoObject;
    public GameObject sfx;
    public AudioClip chestOpenSound;

    GameManager gm;

    public Transform weaponSpawnPos;
    public Transform healSpawnPos;
    public Transform gunAmmoSpawnPos;
    public Transform auxAmmoSpawnPos;

    private void Awake()
    {
        chestId = transform.position;

        gm = GameManager.instance;
        gm.AddChest(this);
    }

    public void Open(bool isFromNetwork)
    {
        gameObject.tag = "NonInteractable";
        GetComponent<Animator>().Play("Open");
        Instantiate(sfx, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(chestOpenSound, 45, 1);
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

            GameObject gunAmmo = Instantiate(ammoObject, gunAmmoSpawnPos.position, Quaternion.identity);
            gunAmmo.GetComponent<Ammo>().type = loot.ammoForGun;
            gunAmmo.GetComponent<Ammo>().count = loot.ammoForGunCount;
            gunAmmo.GetComponent<Ammo>().networkSpawned = false;

            GameObject auxAmmo = Instantiate(ammoObject, auxAmmoSpawnPos.position, Quaternion.identity);
            auxAmmo.GetComponent<Ammo>().type = loot.auxAmmo;
            auxAmmo.GetComponent<Ammo>().count = loot.auxAmmoCount;
            auxAmmo.GetComponent<Ammo>().networkSpawned = false;

            if (loot.health != null)
            {
                GameObject heal = Instantiate(loot.health, healSpawnPos.position, Quaternion.identity);
                heal.GetComponent<Healable>().networkSpawned = false;
                heal.GetComponent<Healable>().count = 1;
            }
        }  
    }
}
