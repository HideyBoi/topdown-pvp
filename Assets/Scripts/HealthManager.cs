using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine.UI;
using TMPro;

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

    public bool isLocalPlayer = false;
    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
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

    public void Heal(int heal)
    {
        health += heal;
        if (health > 150)
        {
            health = 150;
        }
    }

    public void Die(ushort killingPlayer, int gunId)
    {
        Debug.Log(killingPlayer + " killed " + thisId + " with " + GameManager.instance.possibleWeapons[gunId]);
        transform.position = new Vector3(-30, 0, 30);
        if (isLocalPlayer)
        {
            timeUntilRespawn = respawnTime;
            isDead = true;
        }
    }

    void Respawn()
    {
        //get spawnpoint
        transform.position = new Vector3(0, 0.5f, 0);
        health = 150;
    }
}
