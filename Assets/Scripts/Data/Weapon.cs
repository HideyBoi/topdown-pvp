using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

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
    public int id;
    [Header("Gun Visual")]
    public Mesh gunMesh;
    public Material gunMaterial;
    public Vector3 muzzleLocation;
    public AudioClip shootSound;
    public AudioClip pickupSound;
    public AudioClip reloadSound;
    public float magnitude;
    public float roughness;
    public float shakeFadeIn;
    public float shakeFadeOut;
    [Header("Gun Stats")]
    public Rarity rarity;
    public LocalInventoryManager.AmmoType ammoType;
    public bool automatic;
    public int damage;
    public float damageFalloff = 0;
    public int maxAmmoCount;
    public float timeBetweenShots;
    public float reloadTime;
    public int shotCount;
    public float spread;
    public float speedModifier = 1;
}

