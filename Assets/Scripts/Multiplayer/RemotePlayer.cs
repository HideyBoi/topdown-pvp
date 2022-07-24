using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public ushort _id;
    public Transform pivot;
    public RemoteInventoryManager invManager;
    public HealthManager healthManager;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    public void UpdatePosition(Vector3 pos, Quaternion pivotRot)
    {
        transform.position = pos;
        pivot.rotation = pivotRot;
    }
}
