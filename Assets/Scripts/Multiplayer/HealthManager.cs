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
    public ushort id;
    public bool isLocalPlayer;
    int maxHealth;
    int currentHealth;
    public bool isDead;
    [SerializeField]
    float respawnTime;
    float timeUntilRespawn;

    private void Awake()
    {
        if (id == NetworkManager.instance.Client.Id)
            localHealthManager = this;

        maxHealth = RulesManager.instance.maxHealth;
        currentHealth = maxHealth;
    }

    public void Damage(int damage, int gunId, ushort fromId)
    {
        if (!isLocalPlayer || isDead)
            return;

        Debug.Log($"[Health Manager] Got hit! Damage:{damage} Gun ID:{gunId} From Player ID:{fromId}");

        currentHealth -= damage;
        if (currentHealth <= 0 )
        {
            isDead = true;
            timeUntilRespawn = respawnTime;

        }
    }
}
