using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public DungeonGenerator generator;
    public GameObject gen;
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

    public List<Transform> spawns;

    public List<NetworkManager.MultiplayerPlayer> playersInGame;

    public AudioClip[] clips;

    [Header("Game Settings")]
    public int lives = 3;
    public bool dropLootOnEveryDeath;
    public bool giveStartingStatsOnDropLoot;
    public Vector2 mapSize;
    public int maxHealth;
    public int startingSyringes;
    public int startingMedkits;
    public int startingLightAmmo;
    public int startingMediumAmmo;
    public int startingHeavyAmmo;
    public int startingShellsAmmo;

    private void Awake()
    {
        for (int i = 0; i < possibleWeapons.Length; i++)
        {
            possibleWeapons[i].id = i;
        }

        foreach (var player in NetworkManager.instance.connectedPlayers)
        {
            playersInGame.Add(player);
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
                remPlayer.GetComponent<RemotePlayer>()._name = player.name;
                remPlayer.GetComponent<HealthManager>().thisId = player.id;
                remotePlayers.Add(remPlayer.GetComponent<RemotePlayer>());
                player.playerObject = remPlayer;
            } else
            {
                player.playerObject = localPlayerObject;
            }
        }

        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerReady);
        msg.AddUShort(localPlayerId);

        networkManager.Client.Send(msg);
        networkManager.PlayerIsReady(localPlayerId);
    }

    bool endedGame = false;

    private void FixedUpdate()
    {
        if (!generationStarted && networkManager.isDoneLoading)
        {
            generationStarted = true;
            if (networkManager.Server.IsRunning)
                generator.StartGenerating();
        }

        if (playersInGame.Count == 1 && NetworkManager.instance.connectedPlayers.Count > 1 && !endedGame)
        {
            endedGame = true;
            Debug.Log("GAME END");
            LoadingScreen.instance.LoadLevel("MainMenu");
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

        foreach (var player in playersInGame)
        {
            if (player.id == id)
            {
                playersInGame.Remove(player);
                return;
            }
        }
    }

    public void ResetDungeonGen()
    {
        GameObject newGen = Instantiate(gen);
        generator = newGen.GetComponent<DungeonGenerator>();
        generator.ShouldGen = true;

        spawnedChests = new List<Chest>();
        spawns = new List<Transform>();
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerOutOfGame)]
    static void PlayerOutOfGame(Message msg)
    {
        ushort id = msg.GetUShort();

        foreach (var item in instance.playersInGame)
        {
            if (item.id == id)
            {
                instance.playersInGame.Remove(item);
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

    [MessageHandler((ushort)NetworkManager.MessageIds.playerHeal)]
    static void HealPlayer(Message msg)
    {
        ushort id = msg.GetUShort();

        foreach (var player in instance.remotePlayers)
        {
            if (player._id == id)
            {
                player.healthManager.Heal(msg.GetInt(), true);
            }
        }
    }

    public void AddSpawn(Transform t)
    {
        spawns.Add(t);
    }

    public void Respawn()
    {
        localPlayerObject.transform.position = spawns[Random.Range(0, spawns.Count)].position;
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.soundEffect)]
    static void Sfx(Message msg)
    {
        instance.PlaySoundEffectByID(msg.GetVector3(), msg.GetInt(), msg.GetFloat(), msg.GetFloat());
    }

    public void PlaySoundEffectByID(Vector3 position, int audioID, float volume, float maxDistance)
    {
        AudioClip cliptoplay = clips[audioID];

        Instantiate(soundEffect, position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(cliptoplay, maxDistance, volume);
    }

    [Header("Loot")]

    public Weapon[] possibleWeapons;

    public List<Chest> spawnedChests;
    public List<GroundItem> spawnedItems;
    public List<Healable> spawnedHealables;
    public List<Ammo> spawnedAmmo;

    public Weapon[] genericWeapons;

    public Weapon[] rareWeapons;
    public int rareChance;
    public Weapon[] legendaryWeapons;
    public int legendaryChance;
    public GameObject syringe;
    public int syringeChance;
    public GameObject medkit;
    public int medkitChance;

    public GameObject ammoPrefab;

    [System.Serializable]
    public class Loot
    {
        public Weapon weapon;
        public GameObject health = null;
        public LocalInventoryManager.AmmoType ammoForGun;
        public int ammoForGunCount;
        public LocalInventoryManager.AmmoType auxAmmo;
        public int auxAmmoCount;
    }

    public Loot GenerateLoot()
    {
        Loot loot = new Loot();

        if (Chance(rareChance))
        {
            //Debug.Log("Rare");
            loot.weapon = rareWeapons[Random.Range(0, rareWeapons.Length)];
        }
        else if (Chance(legendaryChance))
        {
            //Debug.Log("Legendary");
            loot.weapon = legendaryWeapons[Random.Range(0, legendaryWeapons.Length)];
        }
        else
        {
            //Debug.Log("Generic");
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

        loot.ammoForGun = loot.weapon.ammoType;
        loot.ammoForGunCount = loot.weapon.maxAmmoCount;

        switch (Random.Range(0, 4))
        {
            case 0:
                loot.auxAmmo = LocalInventoryManager.AmmoType.Light;
                break;
            case 1:
                loot.auxAmmo = LocalInventoryManager.AmmoType.Medium;
                break;
            case 2:
                loot.auxAmmo = LocalInventoryManager.AmmoType.Heavy;
                break;
            case 3:
                loot.auxAmmo = LocalInventoryManager.AmmoType.Shells;
                break;
        }

        loot.auxAmmoCount = Random.Range(4, 12);

        return loot;
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

    public void AddAmmo(Ammo item)
    {
        spawnedAmmo.Add(item);
    }

    public Weapon GetWeaponById(int gunId)
    {
        foreach (var gun in possibleWeapons)
        {
            if (gun.id == gunId)
            {
                return gun;
            }
        }

        return null;
    }

    bool Chance(int chance)
    {
        if (Random.Range(0, 100) < chance)
        {
            return true;
        }
        else { return false; }
    }

    public RemotePlayer GetRemotePlayer(ushort id)
    {
        foreach (var player in remotePlayers)
        {
            if (player._id == id)
            {
                return player;
            }
        }

        return null;
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
            obj.GetComponent<Healable>().id = id;
            obj.GetComponent<Healable>().count = msg.GetInt();
        } else
        {
            GameObject obj = Instantiate(instance.medkit, pos, Quaternion.identity);
            obj.GetComponent<Healable>().networkSpawned = true;
            obj.GetComponent<Healable>().id = id;
            obj.GetComponent<Healable>().count = msg.GetInt();
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

    [MessageHandler((ushort)NetworkManager.MessageIds.spawnAmmo)]
    static void SpawnAmmo(Message msg)
    {
        int id = msg.GetInt();
        Vector3 pos = msg.GetVector3();
        int typeId = msg.GetInt();
        int count = msg.GetInt();

        LocalInventoryManager.AmmoType type = LocalInventoryManager.AmmoType.Light;

        switch (typeId)
        {
            case 0:
                type = LocalInventoryManager.AmmoType.Light;
                break;
            case 1:
                type = LocalInventoryManager.AmmoType.Medium;
                break;
            case 2:
                type = LocalInventoryManager.AmmoType.Heavy;
                break;
            case 3:
                type = LocalInventoryManager.AmmoType.Shells;
                break;
        }

        GameObject ammoObj = Instantiate(instance.ammoPrefab, pos, Quaternion.identity);
        ammoObj.GetComponent<Ammo>().networkSpawned = true;
        ammoObj.GetComponent<Ammo>().id = id;
        ammoObj.GetComponent<Ammo>().type = type;
        ammoObj.GetComponent<Ammo>().count = count;
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.pickUpAmmo)]
    static void PickUpAmmo(Message msg)
    {
        int deltedItemId = msg.GetInt();

        Ammo itemGround = null;

        foreach (var item in instance.spawnedAmmo)
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
                Instantiate(instance.soundEffect, player.transform).GetComponent<SoundEffect>().PlaySound(instance.possibleWeapons[msg.GetInt()].shootSound, 60, 1);
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
                Instantiate(instance.soundEffect, player.transform).GetComponent<SoundEffect>().PlaySound(instance.possibleWeapons[msg.GetInt()].reloadSound, 35, 1);
            }
        }
    }
}
