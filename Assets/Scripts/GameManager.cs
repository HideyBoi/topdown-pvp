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

    public List<RemotePlayer> remotePlayers = new List<RemotePlayer>();
    public GameObject remotePlayerPrefab;

    bool generationStarted = false;

    private void Awake()
    {
        instance = this;

        networkManager = NetworkManager.instance;
        networkManager.gameManager = this;

        localPlayerId = networkManager.Client.Id;

        Instantiate(playerPrefab);

        foreach (var player in networkManager.connectedPlayers)
        {
            if (player.id != localPlayerId)
            {
                GameObject remPlayer = Instantiate(remotePlayerPrefab);
                remPlayer.GetComponent<RemotePlayer>()._id = player.id;
                remotePlayers.Add(remPlayer.GetComponent<RemotePlayer>());
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


    [Header("Loot")]

    public Weapon[] possibleWeapons;

    public List<Chest> spawnedChests;
    public List<GroundItem> spawnedItems;

    public Weapon[] genericWeapons;

    public Weapon[] rareWeapons;
    public int rareChance;
    public Weapon[] legendaryWeapons;
    public int legendaryChance;

    [System.Serializable]
    public class Loot
    {
        public Weapon weapon;
        //ammmo
        //heals
    }

    public void AddChest(Chest chest)
    {
        spawnedChests.Add(chest);
    }

    public void AddItem(GroundItem item)
    {
        spawnedItems.Add(item);
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

        //health
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
}
