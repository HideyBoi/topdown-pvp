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
    public GameObject deathEffect;
    public ushort thisId;

    [Header("Local Player Specific")]
    [SerializeField] private Animator animator;
    float respawnTime = 6f;
    public bool isDead;
    float timeUntilRespawn;
    [SerializeField] private Slider heatlhBar;
    [SerializeField] private TMP_Text healthBarText;
    [SerializeField] private CinemachineVirtualCamera cam;
    public RemotePlayer spectating;
    bool isSpectating;

    public GameObject deathOverlay;
    public TMP_Text killerNameText;
    public TMP_Text gunNameText;
    public TMP_Text killerHealthText;
    public GameObject[] rarityIndicator;
    public Slider killerHealth;

    public bool isLocalPlayer = false;
    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            if (isSpectating)
            {
                cam.m_Follow = spectating.transform;
            }

            animator.SetInteger("Health", health);
            timeUntilRespawn -= Time.fixedDeltaTime;
            if (timeUntilRespawn < 0 && isDead)
            {
                isDead = false;
                Respawn();
            }
            heatlhBar.value = health;
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
        if (health > 150)
        {
            health = 150;
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

            rarityIndicator[0].SetActive(true);
            rarityIndicator[1].SetActive(true);
            rarityIndicator[2].SetActive(true);

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
        health = 150;
        isSpectating = false;
        spectating.beingSpectated = false;
        cam.m_Follow = transform;
    }
}
