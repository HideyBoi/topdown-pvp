using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LocalInventoryManager : MonoBehaviour
{
    private Controls controls;

    int currentIndex;
    public InventoryItem[] inventoryItem = new InventoryItem[3];

    bool canSwitch = true;

    public TMP_Text[] ammoCount;
    public TMP_Text[] totalAmmoCount;
    public TMP_Text[] gunName;
    public Image[] gunImageRender;
    public MeshRenderer gunMeshRenderer;
    public MeshFilter gunMeshFilter;
    public GameObject[] genericMarker;
    public GameObject[] rareMarker;
    public GameObject[] legendaryMarker;
    public CanvasGroup[] slots;

    public enum AmmoType
    {
        Light, Medium, Heavy, Shells
    }

    public int lightAmmoCount;
    public int mediumAmmoCount;
    public int heavyAmmoCount;
    public int shellsAmmoCount;

    public Sprite fist;

    public GameObject soundEffect;
    public GameObject groundItem;

    private void Awake()
    {
        controls = new Controls();

        controls.Player.Scroll.performed += ctx => Scroll(ctx.ReadValue<float>());
        UpdateWeaponVisual();
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

            if (inventoryItem[currentIndex].weapon != null)
            {
                SoundEffect sfx = Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>();
                sfx.PlaySound(inventoryItem[currentIndex].weapon.pickupSound);
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateWeaponVisual();
    }

    void UpdateWeaponVisual()
    {
        UpdateSlot(0);
        UpdateSlot(1);
        UpdateSlot(2);

        if (inventoryItem[currentIndex].weapon != null)
        {
            gunMeshFilter.mesh = inventoryItem[currentIndex].weapon.gunMesh;
            gunMeshRenderer.material = inventoryItem[currentIndex].weapon.gunMaterial;
        }

        slots[0].alpha = 0.7f;
        slots[1].alpha = 0.7f;
        slots[2].alpha = 0.7f;

        slots[currentIndex].alpha = 1f;
    }

    void UpdateSlot(int pos)
    {
        if (inventoryItem[pos].weapon != null)
        {
            ammoCount[pos].text = inventoryItem[pos].ammoCount.ToString();
            gunName[pos].text = inventoryItem[pos].weapon.gunName;
            gunImageRender[pos].sprite = inventoryItem[pos].weapon.gunImage;
            switch (inventoryItem[pos].weapon.ammoType)
            {
                case AmmoType.Light:
                    totalAmmoCount[pos].text = (lightAmmoCount + inventoryItem[pos].ammoCount).ToString();
                    break;
                case AmmoType.Medium:
                    totalAmmoCount[pos].text = (mediumAmmoCount + inventoryItem[pos].ammoCount).ToString();
                    break;
                case AmmoType.Heavy:
                    totalAmmoCount[pos].text = (heavyAmmoCount + inventoryItem[pos].ammoCount).ToString();
                    break;
                case AmmoType.Shells:
                    totalAmmoCount[pos].text = (shellsAmmoCount + inventoryItem[currentIndex].ammoCount).ToString();
                    break;
            }

            genericMarker[pos].SetActive(false);
            rareMarker[pos].SetActive(false);
            legendaryMarker[pos].SetActive(false);

            switch (inventoryItem[pos].weapon.rarity)
            {
                case Weapon.Rarity.generic:
                    genericMarker[pos].SetActive(true);
                    break;
                case Weapon.Rarity.rare:
                    rareMarker[pos].SetActive(true);
                    break;
                case Weapon.Rarity.legendary:
                    legendaryMarker[pos].SetActive(true);
                    break;
            }
        }
        else
        {
            ammoCount[pos].text = "--";
            gunName[pos].text = "Fist";
            gunMeshFilter.mesh = null;
            gunMeshRenderer.material = null;
            gunImageRender[pos].sprite = fist;
            totalAmmoCount[pos].text = "--";
        }
    }

    public void PickupWeapon(GroundItem groundItem)
    {
        InventoryItem pickedUpItem = groundItem.currentItem;

        bool foundSpot = false;

        for (int i = 0; i < inventoryItem.Length; i++)
        {
            if (inventoryItem[i].weapon == null && !foundSpot)
            {
                inventoryItem[i] = pickedUpItem;
                foundSpot = true;
            }
        }
        if (!foundSpot)
        {
            Instantiate(groundItem, transform.position, Quaternion.identity).GetComponent<GroundItem>().UpdateItem(inventoryItem[currentIndex]);

            inventoryItem[currentIndex] = pickedUpItem;
        }
        

        groundItem.Pickup();
    }

    public void DropWeapon(int index)
    {
        if (inventoryItem[index].weapon != null)
        {
            Instantiate(groundItem, transform.position, Quaternion.identity).GetComponent<GroundItem>().UpdateItem(inventoryItem[index]);

            inventoryItem[index].weapon = null;
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
