using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager localHealthManager;
    public GameObject deadUI;
    public GameObject normalUI;
    public GameObject persistantUI;
    public ushort id;
    public bool isLocalPlayer;
    public int killCount = 0;
    public int lives = 0;
    bool canRespawn = true;
    LocalInventoryManager inv;
    Rigidbody rb;
    int maxHealth;
    int currentHealth;
    public bool isDead;
    [SerializeField]
    float respawnTime;
    float timeUntilRespawn;
    [SerializeField]
    Transform localKillfeed;
    [SerializeField]
    GameObject localKillfeedItem;
    DamageNumber currentDamageNumber;
    [SerializeField]
    GameObject hitPopup;
    [SerializeField]
    Transform[] lootLocs;
    [SerializeField]
    GameObject ammo;
    [SerializeField]
    GameObject[] healing;
    [SerializeField]
    GameObject item;
    [SerializeField]
    TextMeshProUGUI respawningStatus;
    [SerializeField]
    TextMeshProUGUI livesText;
    [SerializeField]
    TextMeshProUGUI killsText;
    [SerializeField]
    Slider healthBar;
    [SerializeField]
    Animator healthFlash;
    [SerializeField]
    GameObject soundEffect;
    [SerializeField]
    GameObject deathEffect;
    [SerializeField]
    AudioClip deathSound;
    [SerializeField]
    AudioClip[] hitSounds;

    private void Awake()
    {
        lives = RulesManager.instance.lives;
    }

    public void Init()
    {
        localHealthManager = this;
        inv = GetComponent<LocalInventoryManager>();
        rb = GetComponent<Rigidbody>();
        maxHealth = RulesManager.instance.maxHealth;
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = maxHealth;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;


        healthBar.value = currentHealth;
        healthFlash.SetInteger("Health", currentHealth);

        killsText.text = "x " + killCount;
        livesText.text = "x " + lives;

        if (isDead && canRespawn)
        {
            timeUntilRespawn -= Time.fixedDeltaTime;
            if (timeUntilRespawn <= 0)
            {
                int rng = Random.Range(0, GameManager.instance.spawns.Count);
                transform.position = GameManager.instance.spawns[rng].position;

                inv.currentIndex = 0;
                inv.Scroll(0);
                inv.canSwitch = true;     
                deadUI.SetActive(false);
                normalUI.SetActive(true);              
                currentHealth = maxHealth; 
                isDead = false;
                GetComponent<Collider>().enabled = true;
                rb.isKinematic = false;
            }
        }
    }

    public void DamageCosmetics(int damage)
    {
        if (currentDamageNumber == null)
        {
            currentDamageNumber = Instantiate(hitPopup, transform.position, Quaternion.identity).GetComponent<DamageNumber>();
            currentDamageNumber.AddNumber(damage, transform.position);
        }
        else
        {
            currentDamageNumber.AddNumber(damage, transform.position);
        }
    }

    public void Damage(int damage, int gunId, ushort fromId)
    { 
        if (!isLocalPlayer || isDead)
            return;

        Debug.Log($"[Health Manager] Got hit! Damage:{damage} Gun ID:{gunId} From Player ID:{fromId}");
        if (gunId != 0)
            Instantiate(soundEffect, transform).GetComponent<SoundEffect>().PlaySound(hitSounds[Random.Range(0, hitSounds.Length)], 20, 0.7f);

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die(gunId, fromId);
        }
    }

    void Die(int gunId, ushort fromId)
    {
        if (RulesManager.instance.dropLootOnEveryDeath)
            DropLoot();

        Debug.Log("[Health Manager] local player has died.");

        isDead = true;
        timeUntilRespawn = respawnTime;

        deadUI.SetActive(true);
        normalUI.SetActive(false);

        Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(deathSound, 30, 0.8f);
        Instantiate(deathEffect, transform.position, Quaternion.identity);

        GetComponent<Collider>().enabled = false;

        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerDied);
        msg.AddUShort(id);
        msg.AddUShort(fromId);
        msg.AddInt(gunId);

        NetworkManager.instance.Client.Send(msg);

        ShowLocalKillfeed(fromId, id);

        KillFeed.i.OnKill(fromId, id, GameManager.instance.GetWeaponById(gunId));

        if (lives > 0)
        {
            lives--;

            if (lives == 0)
            {
                canRespawn = false;
                respawningStatus.text = "You're out of the game, you've lost your last life!";
                DropLoot();

                Message msg2 = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerOutOfGame);
                msg2.AddUShort(id);
                NetworkManager.instance.Client.Send(msg2);

                transform.position = new Vector3(0, 0, 200);
                rb.isKinematic = true;

                ushort localId = NetworkManager.instance.Client.Id;

                foreach (var item in GameManager.instance.playersInGame)
                {
                    if (item.id == localId)
                    {
                        GameManager.instance.playersInGame.Remove(item);
                    }
                }

                KillFeed.i.OnPlayerOutOfGame(id);
            }
            else if (lives == 1)
            {
                respawningStatus.text = $"You're on your last life, respawning, please wait...";
                canRespawn = true;
                if (RulesManager.instance.dropLootOnEveryDeath)
                {
                    DropLoot();
                }
            }
            else
            {
                respawningStatus.text = $"You have {lives} lives left, respawning, please wait...";
                canRespawn = true;
                if (RulesManager.instance.dropLootOnEveryDeath)
                {
                    DropLoot();
                }
            }

            livesText.text = "x " + lives;
        }
        else if (lives == -1)
        {
            respawningStatus.text = "Respawning, please wait...";
            canRespawn = true;
            livesText.text = "Infinite";
        }

        transform.position = new Vector3(0, 0, 200);
        rb.isKinematic = true;
    }

    void DropLoot()
    {
        if (inv.inventoryItem[1].weapon != null)
        {
            GameObject item1 = Instantiate(item, lootLocs[0].position, Quaternion.identity);
            item1.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[1]);
        }

        if (inv.inventoryItem[2].weapon != null)
        {
            GameObject item2 = Instantiate(item, lootLocs[1].position, Quaternion.identity);
            item2.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[2]);
        }

        if (inv.inventoryItem[3].weapon != null)
        {
            GameObject item3 = Instantiate(item, lootLocs[2].position, Quaternion.identity);
            item3.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[3]);
        }

        if (inv.lightAmmoCount > 0)
        {
            GameObject lightAmmo = Instantiate(ammo, lootLocs[3].position, Quaternion.identity);
            lightAmmo.GetComponent<Ammo>().networkSpawned = false;
            lightAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Light;
            lightAmmo.GetComponent<Ammo>().count = inv.lightAmmoCount;
        }

        if (inv.mediumAmmoCount > 0)
        {
            GameObject mediumAmmo = Instantiate(ammo, lootLocs[4].position, Quaternion.identity);
            mediumAmmo.GetComponent<Ammo>().networkSpawned = false;
            mediumAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Medium;
            mediumAmmo.GetComponent<Ammo>().count = inv.mediumAmmoCount;
        }

        if (inv.heavyAmmoCount > 0)
        {
            GameObject heavyAmmo = Instantiate(ammo, lootLocs[5].position, Quaternion.identity);
            heavyAmmo.GetComponent<Ammo>().networkSpawned = false;
            heavyAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Heavy;
            heavyAmmo.GetComponent<Ammo>().count = inv.heavyAmmoCount;
        }

        if (inv.shellsAmmoCount > 0)
        {
            GameObject shellsAmmo = Instantiate(ammo, lootLocs[6].position, Quaternion.identity);
            shellsAmmo.GetComponent<Ammo>().networkSpawned = false;
            shellsAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Shells;
            shellsAmmo.GetComponent<Ammo>().count = inv.shellsAmmoCount;
        }

        if (inv.medkitCount > 0)
        {
            GameObject medkit = Instantiate(healing[1], lootLocs[7].position, Quaternion.identity);
            medkit.GetComponent<Healable>().networkSpawned = false;
            medkit.GetComponent<Healable>().count = inv.medkitCount;
        }

        if (inv.syringeCount > 0)
        {
            GameObject syringe = Instantiate(healing[0], lootLocs[8].position, Quaternion.identity);
            syringe.GetComponent<Healable>().networkSpawned = false;
            syringe.GetComponent<Healable>().count = inv.syringeCount;
        }

        if (RulesManager.instance.giveStartingStatsOnDropLoot)
        {
            inv.medkitCount = RulesManager.instance.startingMedkits;
            inv.syringeCount = RulesManager.instance.startingSyringes;
            inv.lightAmmoCount = RulesManager.instance.startingLightAmmo;
            inv.mediumAmmoCount = RulesManager.instance.startingMediumAmmo;
            inv.heavyAmmoCount = RulesManager.instance.startingHeavyAmmo;
            inv.shellsAmmoCount = RulesManager.instance.startingShellsAmmo;
        }
        else
        {
            inv.medkitCount = 0;
            inv.syringeCount = 0;
            inv.lightAmmoCount = 0;
            inv.mediumAmmoCount = 0;
            inv.heavyAmmoCount = 0;
            inv.shellsAmmoCount = 0;
        }

        inv.inventoryItem[1].weapon = null;
        inv.inventoryItem[2].weapon = null;
        inv.inventoryItem[3].weapon = null;
    }

    public void ShowLocalKillfeed(ushort killerId, ushort victimId)
    {
        if (victimId != id)
        {
            Instantiate(localKillfeedItem, localKillfeed).GetComponent<KillPopup>().UpdateText("<color=red>Killed</color> " + GameManager.instance.GetRemotePlayer(victimId)._name);
        } else
        {
            Instantiate(localKillfeedItem, localKillfeed).GetComponent<KillPopup>().UpdateText($"Got killed by <color=red>{GameManager.instance.GetRemotePlayer(killerId)._name}</color>");
        }
    }
}
