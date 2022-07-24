using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public DungeonGenerator generator;
    NetworkManager networkManager;
    public ushort localPlayerId;

    public GameObject localPlayer;
    public GameObject playerPrefab;
    public GameObject localPlayerObject;

    public List<RemotePlayer> remotePlayers = new List<RemotePlayer>();
    public GameObject remotePlayerPrefab;
    public GameObject groundItem;

    public GameObject[] particleObjects;
    public GameObject muzzleFlash;

    public GameObject soundEffect;

    bool generationStarted = false;

    private void Awake()
    {
        for (int i = 0; i < possibleWeapons.Length; i++)
        {
            possibleWeapons[i].id = i;
        }

        instance = this;

        networkManager = NetworkManager.instance;
        networkManager.gameManager = this;

        localPlayerId = networkManager.Client.Id;

        localPlayerObject = Instantiate(playerPrefab);
        localPlayerObject.GetComponent<HealthManager>().isLocalPlayer = true;
        localPlayerObject.GetComponent<HealthManager>().thisId = networkManager.Client.Id;

        foreach (var player in networkManager.connectedPlayers)
        {
            if (player.id != localPlayerId)
            {
                GameObject remPlayer = Instantiate(remotePlayerPrefab);
                remPlayer.GetComponent<RemotePlayer>()._id = player.id;
                remPlayer.GetComponent<HealthManager>().thisId = player.id;
                remotePlayers.Add(remPlayer.GetComponent<RemotePlayer>());
                player.playerObject = remPlayer;
            } else
            {
                player.playerObject = localPlayerObject;
            }
        }

        Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.playerReady, shouldAutoRelay: true);
        msg.AddUShort(localPlayerId);

        networkManager.Client.Send(msg);
        networkManager.PlayerIsReady(localPlayerId);
    }

    private void FixedUpdate()
    {
        if (!generationStarted && networkManager.isDoneLoading)
        {
            generationStarted = true;
            if (networkManager.Server.IsRunning)
                generator.StartGenerating();
        }
    }

    public void PlayerLeft(ushort id)
    {
        foreach (var player in remotePlayers)
        {
            if (id == player._id)
            {
                Debug.Log("Player is disconnecting while in game.");
                Destroy(player.gameObject);
                remotePlayers.Remove(player);
                return;
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerPos)]
    static void PlayerMoved(Message msg)
    {
        ushort fromId = msg.GetUShort();

        foreach (var player in GameManager.instance.remotePlayers)
        {
            if (player._id == fromId)
            {
                player.transform.position = msg.GetVector3();
                player.transform.rotation = msg.GetQuaternion();
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerDamage)]
    static void DamagePlayer(Message msg)
    {
        ushort id = msg.GetUShort();
        int damage = msg.GetInt();
        ushort fromId = msg.GetUShort();
        int gunId = msg.GetInt();

        if (id == NetworkManager.instance.Client.Id)
        {
            instance.localPlayerObject.GetComponent<HealthManager>().Damage(damage, fromId, gunId, true);
        }

        foreach (var player in instance.remotePlayers)
        {
            if (id == player._id)
            {
                player.healthManager.Damage(damage, fromId, gunId, true);
            }
        }
    }

    [Header("Loot")]

    public Weapon[] possibleWeapons;

    public List<Chest> spawnedChests;
    public List<GroundItem> spawnedItems;
    public List<Healable> spawnedHealables;

    public Weapon[] genericWeapons;

    public Weapon[] rareWeapons;
    public int rareChance;
    public Weapon[] legendaryWeapons;
    public int legendaryChance;
    public GameObject syringe;
    public int syringeChance;
    public GameObject medkit;
    public int medkitChance;

    [System.Serializable]
    public class Loot
    {
        public Weapon weapon;
        //ammmo
        public GameObject health = null;
    }

    public void AddChest(Chest chest)
    {
        spawnedChests.Add(chest);
    }

    public void AddItem(GroundItem item)
    {
        spawnedItems.Add(item);
    }

    public void AddHealItem(Healable item)
    {
        spawnedHealables.Add(item);
    }

    public Loot GenerateLoot()
    {
        Loot loot = new Loot();

        if (Chance(rareChance))
        {
            Debug.Log("Rare");
            loot.weapon = rareWeapons[Random.Range(0, rareWeapons.Length)];
        }
        else if (Chance(legendaryChance))
        {
            Debug.Log("Legendary");
            loot.weapon = legendaryWeapons[Random.Range(0, legendaryWeapons.Length)];
        }
        else
        {
            Debug.Log("Generic");
            loot.weapon = genericWeapons[Random.Range(0, genericWeapons.Length)];
        }

        if (Chance(syringeChance))
        {
            loot.health = syringe;
        }

        if (Chance(medkitChance))
        {
            loot.health = medkit;
        }

        //correct and aux ammo

        return loot;
    }

    bool Chance(int chance)
    {
        if (Random.Range(0, 100) < chance)
        {
            return true;
        }
        else { return false; }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.openChest)]
    static void ChestOpened(Message msg)
    {
        Vector3 id = msg.GetVector3();

        foreach (var Chest in GameManager.instance.spawnedChests)
        {
            if (Chest.chestId == id)
            {
                Chest.Open(true);
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.spawnItem)]
    static void SpawnItem(Message msg)
    {
        int id = msg.GetInt();
        GameObject item = Instantiate(instance.groundItem, msg.GetVector3(), Quaternion.identity);
        item.GetComponent<GroundItem>().networkSpawned = true;
        item.GetComponent<GroundItem>().id = id;

        InventoryItem inv = new InventoryItem();
        inv.weapon = instance.possibleWeapons[msg.GetInt()];
        inv.ammoCount = inv.weapon.maxAmmoCount;

        item.GetComponent<GroundItem>().UpdateItem(inv);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.pickUpItem)]
    static void PickUpItem(Message msg)
    {
        int deltedItemId = msg.GetInt();

        GroundItem itemGround = null;

        foreach (var item in instance.spawnedItems)
        {
            if (item.id == deltedItemId)
            {
                itemGround = item;
            }
        }

        itemGround.Pickup(true);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.spawnHeal)]
    static void SpawnHeal(Message msg)
    {
        int id = msg.GetInt();
        Vector3 pos = msg.GetVector3();
        if (msg.GetInt() == 0)
        {
            GameObject obj = Instantiate(instance.syringe, pos, Quaternion.identity);
            obj.GetComponent<Healable>().networkSpawned = true;
        } else
        {
            GameObject obj = Instantiate(instance.medkit, pos, Quaternion.identity);
            obj.GetComponent<Healable>().networkSpawned = true;
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.pickUpHeal)]
    static void PickUpHeal(Message msg)
    {
        int deltedItemId = msg.GetInt();

        Healable itemGround = null;

        foreach (var item in instance.spawnedHealables)
        {
            if (item.id == deltedItemId)
            {
                itemGround = item;
            }
        }

        itemGround.Pickup(true);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerHoldItem)]
    static void PlayerHoldItem(Message msg)
    {
        ushort id = msg.GetUShort();

        foreach (var player in instance.remotePlayers)
        {
            if (player._id == id)
            {
                player.invManager.UpdateItem(msg.GetInt());
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerGunRot)]
    static void PlayerGunRot(Message msg)
    {
        ushort id = msg.GetUShort();

        foreach (var player in instance.remotePlayers)
        {
            if (player._id == id)
            {
                player.invManager.UpdateGunRot(msg.GetQuaternion());
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.particleEffect)]
    static void SpawnParticle(Message msg)
    {
        GameObject particle = Instantiate(instance.particleObjects[msg.GetInt()], msg.GetVector3(), msg.GetQuaternion());
        particle.GetComponent<Vfx>().networkSpawned = true;
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerShot)]
    static void MuzzleFlash(Message msg)
    {
        ushort id = msg.GetUShort();

        foreach (var player in instance.remotePlayers)
        {
            if (player._id == id)
            {
                Instantiate(instance.soundEffect, player.transform).GetComponent<SoundEffect>().PlaySound(instance.possibleWeapons[msg.GetInt()].shootSound);
                Instantiate(instance.muzzleFlash, msg.GetVector3(), msg.GetQuaternion(), player.pivot);
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerReloadSound)]
    static void PlayerReload(Message msg)
    {
        ushort id = msg.GetUShort();

        foreach (var player in instance.remotePlayers)
        {
            if (player._id == id)
            {
                Instantiate(instance.soundEffect, player.transform).GetComponent<SoundEffect>().PlaySound(instance.possibleWeapons[msg.GetInt()].reloadSound);
            }
        }
    }
}
