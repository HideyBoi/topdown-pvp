using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalInventoryManager : MonoBehaviour
{
    private Controls controls;

    int currentIndex;
    public InventoryItem[] inventoryItem = new InventoryItem[3];

    bool canSwitch = true;

    public TMP_Text ammoCount;
    public TMP_Text totalAmmoCount;
    public TMP_Text gunName;

    public enum AmmoType
    {
        Light, Medium, Heavy, Shells
    }

    public int lightAmmoCount;
    public int mediumAmmoCount;
    public int heavyAmmoCount;
    public int shellsAmmoCount;

    public MeshRenderer gunMeshRenderer;
    public MeshFilter gunMeshFilter;

    public GameObject soundEffect;

    private void Awake()
    {
        controls = new Controls();

        controls.Player.Scroll.performed += ctx => Scroll(ctx.ReadValue<float>());
    }

    void Scroll(float amount)
    {
        if (canSwitch)
        {
            currentIndex += (int)amount;

            if (currentIndex >= inventoryItem.Length)
            {
                currentIndex = 0;
            }

            if (currentIndex < 0)
            {
                currentIndex = inventoryItem.Length - 1;
            }

            UpdateWeaponVisual();
        }
    }

    void UpdateWeaponVisual()
    {
        if (inventoryItem[currentIndex] != null)
        {
            ammoCount.text = inventoryItem[currentIndex].ammoCount.ToString();
            gunName.text = inventoryItem[currentIndex].weapon.gunName;
            gunMeshFilter.mesh = inventoryItem[currentIndex].weapon.gunMesh;
            gunMeshRenderer.material = inventoryItem[currentIndex].weapon.gunMaterial;
            switch (inventoryItem[currentIndex].weapon.ammoType)
            {
                case AmmoType.Light:
                    totalAmmoCount.text = (lightAmmoCount + inventoryItem[currentIndex].ammoCount).ToString();
                    break;
                case AmmoType.Medium:
                    totalAmmoCount.text = (mediumAmmoCount + inventoryItem[currentIndex].ammoCount).ToString();
                    break;
                case AmmoType.Heavy:
                    totalAmmoCount.text = (heavyAmmoCount + inventoryItem[currentIndex].ammoCount).ToString();
                    break;
                case AmmoType.Shells:
                    totalAmmoCount.text = (shellsAmmoCount + inventoryItem[currentIndex].ammoCount).ToString();
                    break;
            }

            SoundEffect sfx = Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>();
            sfx.PlaySound(inventoryItem[currentIndex].weapon.pickupSound);
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
