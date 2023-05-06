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
    [SerializeField] TextMeshProUGUI timeRemaining;
    [SerializeField] long timeToStartDeathmatch;

    private void Awake()
    {
        instance = this;
        doDeathmatch = RulesManager.instance.doDeathmatch;

        if (doDeathmatch)
            timeRemaining.gameObject.SetActive(true);

        if (NetworkManager.instance.Server.IsRunning && doDeathmatch)
        {
            PrepareDeathmatch();
        }
    }

    private void FixedUpdate()
    {
        if (!doDeathmatch)
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

    void PrepareDeathmatch()
    {
        int chosenRoom = Random.Range(0, rooms.Length);
        PrepareRoom(chosenRoom);

        DateTime currentTime = DateTime.UtcNow;
        long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

        unixTime += (long)RulesManager.instance.timeTillDeathmatch;
        timeToStartDeathmatch = unixTime;

        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.deathmatchData);
        msg.AddInt(chosenRoom);
        msg.AddLong(unixTime);

        NetworkManager.instance.Client.Send(msg);
    }

    [MessageHandler((ushort)NetworkManager.MessageIds.deathmatchData)]
    static void HandleDeathmatchData(Message msg)
    {
        instance.PrepareRoom(msg.GetInt());
        instance.timeToStartDeathmatch = msg.GetLong();
    }

    IEnumerator StartDeathmatch()
    {
        yield return new WaitForSecondsRealtime(1f);
    }

    void PrepareRoom(int chosenRoom)
    {
        rooms[chosenRoom].SetActive(true);
    }
}
