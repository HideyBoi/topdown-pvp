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
    public GameObject bulletTracer;

    public GameObject soundEffect;
    public AudioClip killSound;
    public GameObject deathEffect;

    bool generationStarted = false;

    public List<Transform> spawns;

    public List<NetworkManager.MultiplayerPlayer> playersInGame;

    public AudioClip[] clips;

    public GameObject sploosh;

    public float timeTillEndscreen;


    private void Awake()
    {
        Debug.Log("[Game Manager] Setting weapon IDs.");
        for (int i = 0; i < possibleWeapons.Length; i++)
        {
            possibleWeapons[i].id = i;
        }

        Debug.Log("[Game Manager] Registering connected players.");
        foreach (var player in NetworkManager.instance.connectedPlayers)
        {
            playersInGame.Add(player);
        }

        instance = this;

        networkManager = NetworkManager.instance;
        networkManager.gameManager = this;

        localPlayerId = networkManager.Client.Id;


        Debug.Log("[Game Manager] Creating local player.");

        localPlayerObject = Instantiate(playerPrefab);
        localPlayerObject.GetComponent<HealthManager>().isLocalPlayer = true;
        localPlayerObject.GetComponent<HealthManager>().id = networkManager.Client.Id;

        foreach (var player in networkManager.connectedPlayers)
        {
            if (player.id != localPlayerId)
            {
                Debug.Log("[Game Manager] Creating remote player for " + player.id + " " + player.name);
                GameObject remPlayer = Instantiate(remotePlayerPrefab);
                remPlayer.GetComponent<RemotePlayer>()._id = player.id;
                remPlayer.GetComponent<RemotePlayer>()._name = player.name;
                remPlayer.GetComponent<RemotePlayer>().HandleCosmetics(player.skinId, player.hatId);
                remPlayer.GetComponent<HealthManager>().id = player.id;
                remotePlayers.Add(remPlayer.GetComponent<RemotePlayer>());
                player.playerObject = remPlayer;
            } else
            {
                player.playerObject = localPlayerObject;
            }
        }

        Debug.Log("[Game Manager] Informing remote clients that local client is done initializing.");
        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerReady);
        msg.AddUShort(localPlayerId);

        networkManager.Client.Send(msg);
        networkManager.PlayerIsReady(localPlayerId);
    }

    public bool endedGame = false;

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
            Debug.Log("[Game Manager] Players in game == 1, game is over, loading Main Menu.");

            endedGame = true;
            ushort winningId = 0;

            foreach (var player in playersInGame)
            {
                winningId = player.id;
            }

            WinningRoom.instance.Win(winningId); 
        }
    }

    public void PlayerLeft(ushort id)
    {
        foreach (var player in remotePlayers)
        {
            if (id == player._id)
            {
                Debug.Log("[Game Manager] Player is disconnecting while in game, removing player object.");
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
        Debug.Log("[Game Manager] Reinitializing dungeon generator.");

        GameObject newGen = Instantiate(gen);
        generator = newGen.GetComponent<DungeonGenerator>();
        generator.ShouldGen = true;

        spawnedChests = new List<Chest>();
        spawns = new List<Transform>();
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerPos)]
    static void PlayerMoved(Message msg)
    {
        ushort fromId = msg.GetUShort();

        foreach (var player in GameManager.instance.remotePlayers)
        {
            if (player._id == fromId)
            {
                if (player != null)
                {
                    Vector3 pos =  msg.GetVector3();
                    Vector3 dir =  msg.GetVector3();
                    Quaternion rot = msg.GetQuaternion();

                    player.UpdatePosition(pos, rot, dir);
                }   
            }
        }
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.damage)]
    public static void DamagePlayer(Message msg)
    {
        int damage = msg.GetInt();
        int gunId = msg.GetInt();
        ushort toId = msg.GetUShort();
        ushort fromId = msg.GetUShort();

        if (toId == NetworkManager.instance.Client.Id)
        {
            instance.localPlayerObject.GetComponent<HealthManager>().Damage(damage, gunId, fromId);
        }
    }

    public static void DamagePlayerLocal(ushort id, int damage)
    {
        instance.GetRemotePlayer(id).healthManager.DamageCosmetics(damage);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerDied)]
    static void PlayerDied(Message msg)
    {
        ushort id = msg.GetUShort();
        ushort killer = msg.GetUShort();
        int gunId = msg.GetInt();
        

        if (killer == NetworkManager.instance.Client.Id)
        {
            HealthManager.localHealthManager.killCount++;
            Instantiate(instance.soundEffect, HealthManager.localHealthManager.transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(instance.killSound, 30, 0.8f);
            Instantiate(instance.deathEffect, HealthManager.localHealthManager.transform.position, Quaternion.identity);
        } else
        {
            instance.GetRemotePlayer(killer).healthManager.killCount++;
            Instantiate(instance.soundEffect, instance.GetRemotePlayer(killer).transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(instance.killSound, 30, 0.8f);
            Instantiate(instance.deathEffect, instance.GetRemotePlayer(killer).transform.position, Quaternion.identity);
        }

        if (id == NetworkManager.instance.Client.Id)
        {
            HealthManager.localHealthManager.transform.position = new Vector3(0, 0, 200);
        } else
        {
            instance.GetRemotePlayer(id).healthManager.lives--;
            instance.GetRemotePlayer(killer).transform.position = new Vector3(0, 0, 200);
        }

        if ((NetworkManager.instance.Client.Id == id) || (NetworkManager.instance.Client.Id == killer))
            HealthManager.localHealthManager.ShowLocalKillfeed(killer, id);

        KillFeed.i.OnKill(killer, id, instance.GetWeaponById(gunId));
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.playerOutOfGame)]
    static void PlayerOutOfGame(Message msg)
    {
        ushort id = msg.GetUShort();

        Debug.Log("[Game Manager] player " + id + " is out of the game.");

        KillFeed.i.OnPlayerOutOfGame(id);

        foreach (var item in instance.playersInGame)
        {
            if (item.id == id)
            {
                instance.playersInGame.Remove(item);
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

        if (audioID > 4 && audioID < 10)
        {
            Instantiate(sploosh, position, Quaternion.identity);
        }

        Instantiate(soundEffect, position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(cliptoplay, maxDistance, volume);
    }
    public void PlaySoundEffectByID(Transform parent, Vector3 position, int audioID, float volume, float maxDistance)
    {
        AudioClip cliptoplay = clips[audioID];

        if (audioID > 4 && audioID < 10)
        {
            Instantiate(sploosh, position, Quaternion.identity);
        }

        Instantiate(soundEffect, parent).GetComponent<SoundEffect>().PlaySound(cliptoplay, maxDistance, volume);
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
        loot.ammoForGunCount = Mathf.RoundToInt(loot.weapon.maxAmmoCount * RulesManager.instance.ammoMultiplier);

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

        loot.auxAmmoCount = Mathf.RoundToInt(Random.Range(4, 12) * RulesManager.instance.ammoMultiplier);

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

        Debug.Log($"[Game Manager] Item with ID:{id} was spawned remotely.");
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.pickUpItem)]
    static void PickUpItem(Message msg)
    {
        int deletedItemId = msg.GetInt();

        GroundItem itemGround = null;

        foreach (var item in instance.spawnedItems)
        {
            if (item.id == deletedItemId)
            {
                itemGround = item;
            }
        }

        itemGround.Pickup(true);

        Debug.Log($"[Game Manager] Item with ID:{deletedItemId} was picked up remotely.");
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

        Debug.Log($"[Game Manager] Healing item with ID:{id} was spawned remotely.");
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.pickUpHeal)]
    static void PickUpHeal(Message msg)
    {
        int deletedItemId = msg.GetInt();

        Healable itemGround = null;

        foreach (var item in instance.spawnedHealables)
        {
            if (item.id == deletedItemId)
            {
                itemGround = item;
            }
        }

        itemGround.Pickup(true);

        Debug.Log($"[Game Manager] Healing item with ID:{deletedItemId} was picked up remotely.");
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

        Debug.Log($"[Game Manager] Ammo({type}) with ID:{id} was spawned remotely");
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.pickUpAmmo)]
    static void PickUpAmmo(Message msg)
    {
        int deletedItemId = msg.GetInt();

        Ammo itemGround = null;

        foreach (var item in instance.spawnedAmmo)
        {
            if (item.id == deletedItemId)
            {
                itemGround = item;
            }
        }

        itemGround.Pickup(true);
        Debug.Log($"[Game Manager] Ammo with ID:{deletedItemId} was picked up remotely.");
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
                if (player.invManager != null)
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

    [MessageHandler((ushort)NetworkManager.MessageIds.punch)]
    static void Punch(Message msg) 
    { 
        bool player = msg.GetBool();
        Vector3 pos = msg.GetVector3();
        Quaternion rotation = msg.GetQuaternion();
        if (player)
        {
            Instantiate(instance.soundEffect, pos, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(instance.clips[13], 30, 0.6f);
            Instantiate(instance.particleObjects[1], pos, rotation);
        } else
        {
            Instantiate(instance.soundEffect, pos, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(instance.clips[12], 30, 0.6f);

        }
    }
    [MessageHandler((ushort)NetworkManager.MessageIds.playerShot)]
    static void MuzzleFlash(Message msg)
    {
        ushort id = msg.GetUShort();
        int weaponId = msg.GetInt();
        Vector3 flashPos = msg.GetVector3();
        Quaternion gunPiv = msg.GetQuaternion();
        Vector3 hitPoint = msg.GetVector3();
        bool playSound = msg.GetBool();

        foreach (var player in instance.remotePlayers)
        {
            if (player._id == id)
            {
                if (playSound) 
                    Instantiate(instance.soundEffect, player.transform).GetComponent<SoundEffect>().PlaySound(instance.possibleWeapons[weaponId].shootSound, 40, 0.7f);

                Instantiate(instance.muzzleFlash, flashPos, gunPiv, player.pivot);

                Instantiate(instance.bulletTracer, flashPos, Quaternion.identity).GetComponent<BulletTracer>().SetData(hitPoint);
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
                Instantiate(instance.soundEffect, player.transform).GetComponent<SoundEffect>().PlaySound(instance.possibleWeapons[msg.GetInt()].reloadSound, 35, 0.7f);
            }
        }
    }
}
