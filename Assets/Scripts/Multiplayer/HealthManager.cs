using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class HealthManager : MonoBehaviour
{
    int health = 150;
    int maxHealth = 150;
    public GameObject deathEffect;
    public ushort thisId;

    [Header("Local Player Specific")]
    [SerializeField] private Animator animator;
    float respawnTime = 6f;
    public bool isDead;
    float timeUntilRespawn;
    public Slider healthBar;
    [SerializeField] private TMP_Text healthBarText;
    [SerializeField] private CinemachineVirtualCamera cam;
    private LocalInventoryManager inv;
    public RemotePlayer spectating;
    bool isSpectating;

    public GameObject deathOverlay;
    public TMP_Text killerNameText;
    public TMP_Text respawningStatus;
    public TMP_Text currentlySpectatingText;
    public TMP_Text gunNameText;
    public TMP_Text killerHealthText;
    public GameObject[] rarityIndicator;
    public Slider killerHealth;
    bool canRespawn;

    public bool isLocalPlayer = false;

    public Transform[] lootLocs = new Transform[9];
    public GameObject item;
    public GameObject[] healing;
    public GameObject ammo;

    private void Awake()
    {
        if (isLocalPlayer)
        {
            maxHealth = GameManager.instance.maxHealth;
            health = maxHealth;
            healthBar.maxValue = maxHealth;
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            if (isSpectating)
            {
                cam.m_Follow = spectating.transform;
                currentlySpectatingText.text = "Spectating: " + spectating._name;
            }

            animator.SetInteger("Health", health);
            timeUntilRespawn -= Time.fixedDeltaTime;
            if (timeUntilRespawn < 0 && isDead && canRespawn)
            {
                isDead = false;
                canRespawn = false;
                Respawn();
            }
            healthBar.value = health;
            healthBarText.text = health.ToString();
        }
    }

    public void Damage(int damage, ushort attackingPlayer, int gunId, bool fromNetwork)
    {
        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Die(attackingPlayer, gunId); 
        }

        if (!fromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.playerDamage, shouldAutoRelay: true);
            msg.AddUShort(thisId);
            msg.AddInt(damage);
            msg.AddUShort(attackingPlayer);
            msg.Add(gunId);
            NetworkManager.instance.Client.Send(msg);
        }
    }

    public void Heal(int heal, bool fromNetwork)
    {
        health += heal;
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (!fromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.playerHeal, shouldAutoRelay: true);
            msg.AddUShort(thisId);
            msg.AddInt(heal);
            NetworkManager.instance.Client.Send(msg);
        }
    }

    public void Die(ushort killingPlayer, int gunId)
    {
        health = 150;
        transform.position = new Vector3(-30, 0, 30);
        if (isLocalPlayer)
        {
            if (GameManager.instance.lives > 0)
            {
                GameManager.instance.lives--;

                if (GameManager.instance.lives == 0)
                {
                    canRespawn = false;
                    respawningStatus.text = "You're out of the game, you've lost your last life!";
                    DropLoot();
                } else if (GameManager.instance.lives == 1)
                {
                    respawningStatus.text = $"You're on your last life, respawning, please wait...";
                    canRespawn = true;
                    if (GameManager.instance.dropLootOnEveryDeath)
                    {
                        DropLoot();
                    }
                } else
                {
                    respawningStatus.text = $"You have {GameManager.instance.lives} lives left, respawning, please wait...";
                    canRespawn = true;
                    if (GameManager.instance.dropLootOnEveryDeath)
                    {
                        DropLoot();
                    }
                }        
            } else if (GameManager.instance.lives == -1)
            {
                respawningStatus.text = "Respawning, please wait...";
                canRespawn = true;
            }

            timeUntilRespawn = respawnTime;
            isDead = true;
            isSpectating = true;

            deathOverlay.SetActive(true);

            RemotePlayer rm = GameManager.instance.GetRemotePlayer(killingPlayer);
            spectating = rm;
            rm.beingSpectated = true;
            killerHealth.value = rm.healthManager.health;
            killerHealthText.text = rm.healthManager.health.ToString();

            killerNameText.text = rm._name;
            gunNameText.text = GameManager.instance.GetWeaponById(gunId).gunName;

            rarityIndicator[0].SetActive(false);
            rarityIndicator[1].SetActive(false);
            rarityIndicator[2].SetActive(false);

            switch (GameManager.instance.GetWeaponById(gunId).rarity)
            {
                case Weapon.Rarity.generic:
                    rarityIndicator[0].SetActive(true);
                    break;
                case Weapon.Rarity.rare:
                    rarityIndicator[1].SetActive(true);
                    break;
                case Weapon.Rarity.legendary:
                    rarityIndicator[2].SetActive(true);
                    break;
            }
        } else
        {
            if (GetComponent<RemotePlayer>().beingSpectated)
            {
                GetComponent<RemotePlayer>().beingSpectated = false;
                GameManager.instance.localPlayerObject.GetComponent<HealthManager>().spectating = GameManager.instance.GetRemotePlayer(killingPlayer);
                GameManager.instance.GetRemotePlayer(killingPlayer).beingSpectated = true;
            }
        }
    }

    void Respawn()
    {
        deathOverlay.SetActive(false);
        GameManager.instance.Respawn();
        health = maxHealth;
        isSpectating = false;
        spectating.beingSpectated = false;
        cam.m_Follow = transform;
    }

    void DropLoot()
    {
        GameObject item1 = Instantiate(item, lootLocs[0].position, Quaternion.identity);
        item1.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[0]);

        GameObject item2 = Instantiate(item, lootLocs[1].position, Quaternion.identity);
        item2.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[1]);

        GameObject item3 = Instantiate(item, lootLocs[2].position, Quaternion.identity);
        item3.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[2]);

        GameObject lightAmmo = Instantiate(ammo, lootLocs[3].position, Quaternion.identity);
        lightAmmo.GetComponent<Ammo>().networkSpawned = false;
        lightAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Light;
        lightAmmo.GetComponent<Ammo>().count = inv.lightAmmoCount;
        
        GameObject mediumAmmo = Instantiate(ammo, lootLocs[4].position, Quaternion.identity);
        mediumAmmo.GetComponent<Ammo>().networkSpawned = false;
        mediumAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Medium;
        mediumAmmo.GetComponent<Ammo>().count = inv.mediumAmmoCount;

        GameObject heavyAmmo = Instantiate(ammo, lootLocs[5].position, Quaternion.identity);
        heavyAmmo.GetComponent<Ammo>().networkSpawned = false;
        heavyAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Heavy;
        heavyAmmo.GetComponent<Ammo>().count = inv.heavyAmmoCount;

        GameObject shellsAmmo = Instantiate(ammo, lootLocs[6].position, Quaternion.identity);
        shellsAmmo.GetComponent<Ammo>().networkSpawned = false;
        shellsAmmo.GetComponent<Ammo>().type = LocalInventoryManager.AmmoType.Shells;
        shellsAmmo.GetComponent<Ammo>().count = inv.shellsAmmoCount;

        GameObject medkit = Instantiate(healing[1], lootLocs[7].position, Quaternion.identity);
        medkit.GetComponent<Healable>().networkSpawned = false;
        medkit.GetComponent<Healable>().count = inv.medkitCount;

        GameObject syringe = Instantiate(healing[0], lootLocs[8].position, Quaternion.identity);
        syringe.GetComponent<Healable>().networkSpawned = false;
        syringe.GetComponent<Healable>().count = inv.syringeCount;

        if (GameManager.instance.giveStartingStatsOnDropLoot)
        {
            inv.medkitCount = GameManager.instance.startingMedkits;
            inv.syringeCount = GameManager.instance.startingSyringes;
            inv.lightAmmoCount = GameManager.instance.startingLightAmmo;
            inv.mediumAmmoCount = GameManager.instance.startingMediumAmmo;
            inv.heavyAmmoCount = GameManager.instance.startingHeavyAmmo;
            inv.shellsAmmoCount = GameManager.instance.startingShellsAmmo;
        } else
        {
            inv.medkitCount = 0;
            inv.syringeCount = 0;
            inv.lightAmmoCount = 0;
            inv.mediumAmmoCount = 0;
            inv.heavyAmmoCount = 0;
            inv.shellsAmmoCount = 0;
        }

        inv.inventoryItem[0].weapon = null;
        inv.inventoryItem[1].weapon = null;
        inv.inventoryItem[2].weapon = null;
    }
}
