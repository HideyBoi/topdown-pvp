using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        GameManager.instance.AddSpawn(transform);
    }
}
