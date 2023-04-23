using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSceneSelector : MonoBehaviour
{
    public GameObject[] rooms;

    private void Awake()
    {
        rooms[Random.Range(0, rooms.Length)].SetActive(true);
    }
}
