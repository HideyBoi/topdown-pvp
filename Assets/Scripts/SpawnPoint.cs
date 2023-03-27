using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.instance)
            GameManager.instance.AddSpawn(transform);
    }
}
