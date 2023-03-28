using Discord;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    bool isDiscordEnabled = true;
    int fixedFramesTillRecheck = 120;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (PlayerPrefs.HasKey("DISCORD_RP"))
            {
                if (PlayerPrefs.GetInt("DISCORD_RP") == 1)
                {
                    isDiscordEnabled=true;
                } else
                {
                    isDiscordEnabled=false;
                }
            } else
            {
                PlayerPrefs.SetInt("DISCORD_RP", 1);
            }
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (isDiscordEnabled)
        {
            try
            {
                Debug.Log("[Discord Manager] Enabling Discord.");
                discord = new Discord.Discord(applicationID, (ulong)CreateFlags.NoRequireDiscord);
            }
            catch
            {
                Destroy(gameObject);
                Debug.Log("[Discord Manager] Couldn't connect to Discord.");
            }

            UpdateStatus();
        }
    }

    void FixedUpdate()
    {
        fixedFramesTillRecheck--;
        if (fixedFramesTillRecheck < 0)
        {
            fixedFramesTillRecheck = 120;

            if (PlayerPrefs.GetInt("DISCORD_RP") == 1)
            {
                if (!isDiscordEnabled)
                {
                    try
                    {
                        Debug.Log("[Discord Manager] Enabling Discord.");
                        discord = new Discord.Discord(applicationID, (ulong)CreateFlags.NoRequireDiscord);
                    }
                    catch
                    {
                        Destroy(gameObject);
                        Debug.Log("[Discord Manager] Couldn't connect to Discord.");
                    }

                    UpdateStatus();
                }

                isDiscordEnabled = true;
            }
            else
            {
                if (isDiscordEnabled)
                {
                    Debug.Log("[Discord Manager] Disabling Discord.");
                    if (discord != null)
                    {
                        discord.Dispose();
                        discord = null;
                    }
                        
                }

                isDiscordEnabled = false;
            }
        }

        if (!isDiscordEnabled)
            return;

        try
        {
            UpdateStatus();
            discord.RunCallbacks();
        } catch {
            Debug.Log("[Discord Manager] Callbacks failed.");
            Destroy(gameObject);
        }

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