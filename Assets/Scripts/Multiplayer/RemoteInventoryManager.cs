using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using Riptide.Utils;

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

            Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(gun.pickupSound, 20, 0.7f);
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
