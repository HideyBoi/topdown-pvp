using Discord;
using UnityEngine;

public class DiscordManager : MonoBehaviour
{
    public long applicationID;
    [Space]
    public string details = "";
    public string state = "";
    [Space]
    public string largeImage = "";
    public string largeText = "";
    [Space]
    public string smallImage = "";
    public string smallText = "";

    public static DiscordManager instance;

    public Discord.Discord discord;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        UpdateStatus();
    }

    void Update()
    {
        try
        {
            discord.RunCallbacks();
        } catch {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        UpdateStatus();
    }

    void UpdateStatus()
    {
        try
        {
            string status = "";

            if (NetworkManager.instance.gameIsStarted)
            {
                status = "In a game.";
            }
            else if (MainUIManager.instance.currentLobby != null)
            {
                status = "In the lobby.";
            }
            else
            {
                status = "In the main menu.";
            }

            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = details,
                State = status,
                Assets =
            {
                LargeImage = largeImage,
                LargeText = largeText,
                SmallImage = smallImage,
                SmallText = smallText
            }
                /*,
                Timestamps =
                {
                    Start = time
                }
                */
            };

            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok) Debug.LogWarning("Failed connecting to Discord!");
            });
        } catch
        {
            Debug.Log("[Discord Manager] Couldn't update status.");
        }
    }
}