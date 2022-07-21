using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "Gun", order = 1)]
public class Weapon : ScriptableObject
{
    public enum Rarity
    {
        generic,
        rare,
        legendary
    }

    [Header("Inventory")]
    public string gunName;
    public Sprite gunImage;
    [Header("Gun Visual")]
    public Mesh gunMesh;
    public Material gunMaterial;
    public string idleAnimationName;
    public string attackAnimationName;
    public Vector3 muzzleLocation;
    public AudioClip shootSound;
    public AudioClip pickupSound;
    public AudioClip reloadSound;
    [Header("Gun Stats")]
    public Rarity rarity;
    public LocalInventoryManager.AmmoType ammoType;
    public bool automatic;
    public int damage;
    public int maxAmmoCount;
    public float timeBetweenShots;
    public float reloadTime;
}

