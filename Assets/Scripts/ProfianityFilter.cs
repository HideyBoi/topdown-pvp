using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ProfianityFilter
{
    public static List<string> blockedWords = new List<string>();
    public static bool isEnabled = true;

    public static void Init()
    {
        int counter = 0;

        if (File.Exists(@"./Game/PROF_FILTER.txt"))
        { 
            try
            {
                foreach (string line in File.ReadLines(@"./Game/PROF_FILTER.txt"))
                {
                    blockedWords.Add(line);
                    counter++;
                }

                Debug.Log("Successfully loaded " + counter + " words from ./Game/PROF_FILTER.txt");
            } catch (DirectoryNotFoundException)
            {
                Debug.LogWarning("Couldn't find './Game/PROF_FILTER.txt'");
            } catch (FileNotFoundException)
            {
                Debug.LogWarning("Couldn't find './Game/PROF_FILTER.txt'");
            } catch (PathTooLongException)
            {
                Debug.LogWarning("Path is too long! Couldn't load the profanity filter.");
            } catch(UnauthorizedAccessException)
            {
                Debug.LogWarning("Game doesn't have permission to access './Game/PROF_FILTER.txt'");
            }
        }

        /*/tests
        Debug.Log(Filter("ur a cunt CUNT"));
        Debug.Log(Filter("ur cool"));
        Debug.Log(Filter("fuck fucking asshole you squiggkly diggly"));
        */
    }

    public static string Filter(string s)
    {
        string text = s.ToLower();
        if (isEnabled)
        {
            foreach (var word in blockedWords)
            {
                if (text.Contains(word)) Debug.Log("filtering \"" + word + "\"");
                text = text.Replace(word, GenDashes(word.ToCharArray().Length));
            }

            return text;
        } else
        {
            return text;

        }
    }

    static string GenDashes(int count)
    {
        string dashStr = "";

        for (int i = 0; i < count; i++)
        {
            dashStr += "-";
        }

        return dashStr;
    }
}
