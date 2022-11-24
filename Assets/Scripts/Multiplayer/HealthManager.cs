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

    public int health = 150;
    public int maxHealth = 150;
    public ushort thisId;

    public GameObject healEffect;
    public GameObject deathEffect;

    [Header("Local Player Specific")]
    [SerializeField] private Animator animator;
    public Collider coll;
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

    public GameObject hitPopup;
    DamageNumber currentHitPopup;

    public GameObject killPopup;
    public Transform killPopupParent;

    private void Awake()
    {
        if (isLocalPlayer)
        {
            localHealthManager = this;
            inv = GetComponent<LocalInventoryManager>();
            maxHealth = RulesManager.instance.maxHealth;
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

            Message msg = Message.Create(MessageSendMode.Unreliable, NetworkManager.MessageIds.playerHealth);
            msg.AddInt(health);
            msg.AddUShort(thisId);
            NetworkManager.instance.Client.Send(msg);
        }
    }

    public void Damage(int damage, ushort attackingPlayer, int gunId, bool fromNetwork)
    {
        if (!fromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerDamage);
            msg.AddUShort(thisId);
            msg.AddInt(damage);
            msg.AddUShort(attackingPlayer);
            msg.Add(gunId);
            NetworkManager.instance.Client.Send(msg);

            if (currentHitPopup == null)
            {
                currentHitPopup = Instantiate(hitPopup, transform.position, Quaternion.identity, GameManager.instance.transform).GetComponent<DamageNumber>();
            }

            currentHitPopup.AddNumber(damage, transform.position);
        }

        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Die(attackingPlayer, gunId);
        }
    }

    public void Heal(int heal, bool fromNetwork)
    {
        health += heal;
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        Instantiate(healEffect, transform.position, Quaternion.identity);

        if (!fromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerHeal);
            msg.AddUShort(thisId);
            msg.AddInt(heal);
            NetworkManager.instance.Client.Send(msg);
        }
    }

    public void KilledPlayer(ushort id)
    {
        RemotePlayer killed = GameManager.instance.GetRemotePlayer(id);

        Instantiate(killPopup, killPopupParent).GetComponent<KillPopup>().UpdateName(killed._name);
    }

    public void Health(int newHealth)
    {
        health = newHealth;
    }

    public void Die(ushort killingPlayer, int gunId)
    {
        coll.enabled = false;
        transform.position = new Vector3(-30, 0, 30);

        Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (killingPlayer == NetworkManager.instance.Client.Id)
        {
            if (health <= 0)
            {
                Debug.Log("[Health Manager] Detected that local client has killed a remote client: " + thisId);
                HealthManager.localHealthManager.KilledPlayer(thisId);
            }
        }

        if (isLocalPlayer)
        {
            if (RulesManager.instance.lives > 0)
            {
                RulesManager.instance.lives--;

                if (RulesManager.instance.lives == 0)
                {
                    canRespawn = false;
                    respawningStatus.text = "You're out of the game, you've lost your last life!";
                    DropLoot();
                    OutOfGame();
                } else if (RulesManager.instance.lives == 1)
                {
                    respawningStatus.text = $"You're on your last life, respawning, please wait...";
                    canRespawn = true;
                    if (RulesManager.instance.dropLootOnEveryDeath)
                    {
                        DropLoot();
                    }
                } else
                {
                    respawningStatus.text = $"You have {RulesManager.instance.lives} lives left, respawning, please wait...";
                    canRespawn = true;
                    if (RulesManager.instance.dropLootOnEveryDeath)
                    {
                        DropLoot();
                    }
                }        
            } else if (RulesManager.instance.lives == -1)
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
        coll.enabled = true;
        deathOverlay.SetActive(false);
        GameManager.instance.Respawn();
        health = maxHealth;
        isSpectating = false;
        spectating.beingSpectated = false;
        cam.m_Follow = transform;
    }

    void DropLoot()
    {
        if (inv.inventoryItem[0].weapon != null)
        {
            GameObject item1 = Instantiate(item, lootLocs[0].position, Quaternion.identity);
            item1.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[0]);
        }      

        if (inv.inventoryItem[1].weapon != null)
        {
            GameObject item2 = Instantiate(item, lootLocs[1].position, Quaternion.identity);
            item2.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[1]);
        }      

        if (inv.inventoryItem[2].weapon != null)
        {
            GameObject item3 = Instantiate(item, lootLocs[2].position, Quaternion.identity);
            item3.GetComponent<GroundItem>().UpdateItem(inv.inventoryItem[2]);
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

    void OutOfGame()
    {
        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerOutOfGame);
        msg.AddUShort(thisId);
        NetworkManager.instance.Client.Send(msg);

        foreach (var player in GameManager.instance.playersInGame)
        {
            if (player.id == thisId)
            {
                GameManager.instance.playersInGame.Remove(player);
                return;
            }
        }
    }

    public void GotKill(ushort killedPlayerID)
    {

    }
}
