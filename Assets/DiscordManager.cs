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

    private long time;

    

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        try {
            discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        } catch
        {
            Destroy(gameObject);
        }
        
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

    private void OnDestroy()
    {
        discord.Dispose();
    }

    void UpdateStatus()
    {
        try
        {
            string status = "";
            Discord.Activity activity;


            if (NetworkManager.instance.gameIsStarted)
            {
                if (time == 0)
                {
                    time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                status = "In a game.";

                activity = new Discord.Activity
                {
                    Details = details,
                    State = status,
                    Assets =
                    {
                        LargeImage = largeImage,
                        LargeText = largeText,
                        SmallImage = smallImage,
                        SmallText = smallText
                    },
                    
                    Timestamps =
                    {
                        Start = time
                    }
                    
                };
            }
            else if (MainUIManager.instance.currentLobby != null)
            {
                if (time != 0)
                {
                    time = 0;
                }

                status = "In the lobby.";

                activity = new Discord.Activity
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
                };
            }
            else
            {
                if (time != 0)
                {
                    time = 0;
                }

                status = "In the main menu.";

                activity = new Discord.Activity
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
                };
            }

            var activityManager = discord.GetActivityManager();
            

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