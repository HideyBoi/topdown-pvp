using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class RemoteInventoryManager : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public GameObject soundEffect;
    public Transform gunPivot;

    public void UpdateItem(int gunId)
    {
        if (gunId != -1)
        {
            Weapon gun = GameManager.instance.possibleWeapons[gunId];
            meshFilter.mesh = gun.gunMesh;
            meshRenderer.material = gun.gunMaterial;
        } else
        {
            meshFilter.mesh = null;
            meshRenderer.material = null;
        }
    }

    public void UpdateGunRot(Quaternion rot)
    {
        gunPivot.rotation = rot;
    }
}
