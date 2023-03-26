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
            discord = new Discord.Discord(applicationID, (ulong)CreateFlags.NoRequireDiscord);
        } catch
        {
            Destroy(gameObject);
            Debug.Log("[Discord Manager] Couldn't connect to Discord.");
        }
        
        UpdateStatus();
    }

    void Update()
    {
        try
        {
            discord.RunCallbacks();
        } catch {
            Debug.Log("[Discord Manager] Callbacks failed.");
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        UpdateStatus();
    }

    private void OnDestroy()
    {   
        if (discord != null)
            discord.Dispose();
    }

    void UpdateStatus()
    {
        try
        {
            string status = "";
            Activity activity;


            if (NetworkManager.instance.gameIsStarted)
            {
                if (time == 0)
                {
                    time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }

                status = "In a game.";

                activity = new Activity
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

                activity = new Activity
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

                activity = new Activity
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
                if (res != Result.Ok) 
                    Debug.Log("[Discord Manager] Couldn't update status.");
            });
        } catch
        {
            Debug.Log("[Discord Manager] Couldn't update status.");
        }
    }
}