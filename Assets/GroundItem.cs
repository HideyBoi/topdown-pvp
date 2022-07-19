using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundItem : MonoBehaviour
{
    float speed = 4000;
    [SerializeField] private Transform pivot;

    private void FixedUpdate()
    {
        pivot.rotation = Quaternion.Euler(0, pivot.rotation.eulerAngles.y + Time.fixedDeltaTime * speed, 0);
    }
}
