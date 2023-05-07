using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

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

    GameObject item;
    GameObject gunAmmo;
    GameObject auxAmmo;
    GameObject heal;

    bool beenOpened = false;
    float timeTillRefil = 90.0f;

    private void Awake()
    {
        chestId = transform.position;

        gm = GameManager.instance;
        if (gm)
            gm.AddChest(this);
    }

    private void FixedUpdate()
    {
        if (beenOpened)
        {
            timeTillRefil -= Time.fixedDeltaTime;
            if (timeTillRefil < 0)
            {
                if (item != null)
                {
                    item.tag = "NonInteractable";
                    item.GetComponent<GroundItem>().Remove();
                }
                if (gunAmmo != null)
                {
                    gunAmmo.tag = "NonInteractable";
                    gunAmmo.GetComponent<Ammo>().Remove();
                }
                if (auxAmmo != null)
                {
                    auxAmmo.tag = "NonInteractable";
                    gunAmmo.GetComponent<Ammo>().Remove();
                }
                if (heal != null)
                {
                    heal.tag = "NonInteractable";
                    heal.GetComponent<Healable>().Remove();
                }

                beenOpened = false;
                GetComponent<Animator>().Play("Close");
                gameObject.tag = "Interactable";
            }
        }
    }

    public void Open(bool isFromNetwork)
    {
        gameObject.tag = "NonInteractable";
        beenOpened = true;
        timeTillRefil = 90.0f;
        GetComponent<Animator>().Play("Open");
        Instantiate(sfx, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(chestOpenSound, 25, 0.7f);
        if (!isFromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.openChest);
            msg.AddVector3(chestId);
            NetworkManager.instance.Client.Send(msg);

            GameManager.Loot loot = gm.GenerateLoot();

            InventoryItem inv = new InventoryItem();
            inv.weapon = loot.weapon;
            inv.ammoCount = loot.weapon.maxAmmoCount;

            item = Instantiate(groundItem, weaponSpawnPos.position, Quaternion.identity);
            item.GetComponent<GroundItem>().UpdateItem(inv);
            item.GetComponent<GroundItem>().networkSpawned = false;

            gunAmmo = Instantiate(ammoObject, gunAmmoSpawnPos.position, Quaternion.identity);
            gunAmmo.GetComponent<Ammo>().type = loot.ammoForGun;
            gunAmmo.GetComponent<Ammo>().count = loot.ammoForGunCount;
            gunAmmo.GetComponent<Ammo>().networkSpawned = false;

            auxAmmo = Instantiate(ammoObject, auxAmmoSpawnPos.position, Quaternion.identity);
            auxAmmo.GetComponent<Ammo>().type = loot.auxAmmo;
            auxAmmo.GetComponent<Ammo>().count = loot.auxAmmoCount;
            auxAmmo.GetComponent<Ammo>().networkSpawned = false;

            if (loot.health != null)
            {
                heal = Instantiate(loot.health, healSpawnPos.position, Quaternion.identity);
                heal.GetComponent<Healable>().networkSpawned = false;
                heal.GetComponent<Healable>().count = 1;
            }
        }  
    }
}
