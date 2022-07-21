using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void Open()
    {
        GetComponent<Animator>().Play("Open");
        Instantiate(sfx, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(chestOpenSound);
        GameManager.Loot loot = gm.GenerateLoot();

        InventoryItem inv = new InventoryItem();
        inv.weapon = loot.weapon;
        inv.ammoCount = loot.weapon.maxAmmoCount;

        Instantiate(groundItem, weaponSpawnPos.position, weaponSpawnPos.rotation).GetComponent<GroundItem>().UpdateItem(inv);
    }
}
