using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeathmatchRoom : MonoBehaviour
{
    [SerializeField] bool doDeathmatch = false;
    bool deathmatchStarted = false;

    public static DeathmatchRoom instance;
    [SerializeField] GameObject[] rooms;
    GameObject selectedRoom;
    public GameObject userInterface;
    [SerializeField] TextMeshProUGUI timeRemaining;
    [SerializeField] long timeToStartDeathmatch;

    int ticks = 3;
    [SerializeField] TextMeshProUGUI countdown;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource wind;
    public Vector3 deathmatchSpawn;

    private void Awake()
    {
        instance = this;
        doDeathmatch = RulesManager.instance.doDeathmatch;

        if (doDeathmatch)
            timeRemaining.gameObject.SetActive(true);
    }

    public void TickDown()
    {
        ticks--;
        countdown.text = ticks.ToString();
    }

    private void FixedUpdate()
    {
        if (!doDeathmatch || GameManager.instance.endedGame || timeToStartDeathmatch == 0)
            return;

        DateTime currentTimestamp = DateTime.UtcNow;
        long currentTime = ((DateTimeOffset)currentTimestamp).ToUnixTimeSeconds();

        long seconds = timeToStartDeathmatch - currentTime;
        long minutesDiff = seconds / 60;
        long secondsDiff = seconds - (minutesDiff * 60);

        string secondsString = secondsDiff.ToString();
        if (secondsString.Length == 1)
            secondsString = "0" + secondsString;

        timeRemaining.text = $"Time Until Deathmatch: {minutesDiff}:{secondsString}";

#if DEBUG
        timeRemaining.text += " | DEBUG RawSeconds:" + seconds;
#endif

        if (currentTime >= timeToStartDeathmatch && !deathmatchStarted)
        {
            deathmatchStarted = true;
            Debug.Log("[Deathmatch Room] Starting deathmatch.");
            StartCoroutine(StartDeathmatch());
        }
    }

    public static void HideUI()
    {
        instance.userInterface.SetActive(false);
    }

    [Serializable]
    public class DeathmatchSpawn : IMessageSerializable
    {
        public Vector3 pos;
        public ushort pId;

        public void Serialize(Message msg)
        {
            msg.AddVector3(pos);
            msg.AddUShort(pId);
        }

        public void Deserialize(Message msg)
        {
            pos = msg.GetVector3();
            pId = msg.GetUShort();
        }
    }

    public void PrepareDeathmatch()
    {
        int chosenRoom = Random.Range(0, rooms.Length);
        PrepareRoom(chosenRoom);

        DateTime currentTime = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

        unixTime += (long)RulesManager.instance.timeTillDeathmatch;
        timeToStartDeathmatch = unixTime;

        List<ushort> playerIDs = new List<ushort>();

        foreach (var player in GameManager.instance.playersInGame)
        {
            playerIDs.Add(player.id);
        }

        List<Vector3> possibleSpawns = new List<Vector3>();

        for (int i = 0; i < selectedRoom.transform.childCount; i++)
        {
            Transform t = selectedRoom.transform.GetChild(i);
            if (t.gameObject.CompareTag("DeathmatchSpawn"))
            {
                possibleSpawns.Add(t.position);
            }
        }

        bool allPlayersHaveSpawns = false;
        int currentPlayerIndex = 0;
        int currentSpawnIndex = 0;
        List<DeathmatchSpawn> dSpawns = new List<DeathmatchSpawn>();


        while (!allPlayersHaveSpawns)
        {
            DeathmatchSpawn spawn = new DeathmatchSpawn();

            spawn.pos = possibleSpawns[currentSpawnIndex];
            spawn.pId = playerIDs[currentPlayerIndex];

            dSpawns.Add(spawn);

            currentPlayerIndex++;
            currentSpawnIndex++;

            if (currentPlayerIndex == playerIDs.Count)
                allPlayersHaveSpawns = true;
            
            if (currentSpawnIndex == possibleSpawns.Count)
                currentSpawnIndex = 0;
        }

        ushort localid = NetworkManager.instance.Client.Id;
        foreach (var spawn in dSpawns)
        {
            if (spawn.pId == localid)
            {
                instance.deathmatchSpawn = spawn.pos;
                Debug.Log("[Deathmatch Room] Got spawn location " + instance.deathmatchSpawn);
                break;
            }
        }

        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.deathmatchData);
        msg.AddInt(chosenRoom);
        msg.AddLong(unixTime);
        msg.AddSerializables(dSpawns.ToArray());

        NetworkManager.instance.Client.Send(msg);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.deathmatchData)]
    static void HandleDeathmatchData(Message msg)
    {
        instance.PrepareRoom(msg.GetInt());
        instance.timeToStartDeathmatch = msg.GetLong();
        DeathmatchSpawn[] dSpawns = msg.GetSerializables<DeathmatchSpawn>();

        ushort localid = NetworkManager.instance.Client.Id;
        foreach (var spawn in dSpawns)
        {
            if (spawn.pId == localid)
            {
                instance.deathmatchSpawn = spawn.pos;
                Debug.Log("[Deathmatch Room] Got spawn location " + instance.deathmatchSpawn);
                break;
            }
        }

        Debug.Log($"[Deathmatch Room] Got deathmatch data. Time:{instance.timeToStartDeathmatch}");
    }

    IEnumerator StartDeathmatch()
    {
        animator.Play("enter");
        timeRemaining.gameObject.SetActive(false);

        HealthManager player = HealthManager.localHealthManager;
        bool isInDeathmatch = !player.isDead;

        if (isInDeathmatch)
        {
            
            player.isDead = true;
            player.goingIntoDeathmatch = true;
            player.GetComponent<Collider>().enabled = false;
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.canRespawn = false;         
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (isInDeathmatch)
        {
            player.Heal(RulesManager.instance.maxHealth);
            player.transform.position = deathmatchSpawn;
            player.lives = 1;
        }        

        yield return new WaitForSecondsRealtime(15.2f - 0.5f);

        if (isInDeathmatch)
        {
            player.isDead = false;
            player.goingIntoDeathmatch = false;
            player.canRespawn = true;
            player.GetComponent<Collider>().enabled = true;
            player.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    void PrepareRoom(int chosenRoom)
    {
        rooms[chosenRoom].SetActive(true);
        selectedRoom = rooms[chosenRoom];
    }
}
