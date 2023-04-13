using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Riptide;
using Riptide.Utils;

public class LocalInventoryManager : MonoBehaviour
{
    private Controls controls;
    private LocalGunManager gm;
    private LocalPlayerController playerController;

    public int currentIndex;
    public InventoryItem[] inventoryItem = new InventoryItem[3];

    public bool canSwitch = true;
    public float whenCanSwitch;
    float sinceLastSwitch;

    public TMP_Text[] ammoCount;
    public TMP_Text[] totalAmmoCount;
    public TMP_Text[] gunName;
    public MeshRenderer gunMeshRenderer;
    public MeshFilter gunMeshFilter;
    public GameObject[] genericMarker;
    public GameObject[] rareMarker;
    public GameObject[] legendaryMarker;
    public Image[] slotsImage;
    public CanvasGroup[] slots;

    public Color legendaryColor;
    public Color rareColor;
    public Color genericColor;
    public Color noneColor;

    public enum AmmoType
    {
        Light, Medium, Heavy, Shells
    }

    public int lightAmmoCount;
    public int mediumAmmoCount;
    public int heavyAmmoCount;
    public int shellsAmmoCount;

    public GameObject soundEffect;
    public GameObject groundItem;

    public bool wantsToReload = false;
    public float reloadCooldown;
    public GameObject currentReloadSound;

    public int syringeCount = 0;
    public TMP_Text syringeCountText;
    public Slider syringeSlider;
    public float currentSyringeTime = 0f;
    public float syringeTime = 3.5f;
    bool wantsToUseSyringe = false;
    public int medkitCount = 0;
    public TMP_Text medkitCountText;
    public Slider medkitSlider;
    public float currentMedkitTime = 0f;
    public float medkitTime = 8f;
    bool wantsToUseMedkit = false;

    private void Awake()
    {
        controls = new Controls();
        gm = GetComponent<LocalGunManager>();
        playerController = GetComponent<LocalPlayerController>();

        LocalPlayerController.onDisablePlayerInput += controls.Disable;
        LocalPlayerController.onEnablePlayerInput += controls.Enable;

        controls.Player.Scroll.performed += ctx => Scroll(ctx.ReadValue<float>());
        controls.Player._0.performed += _ => Scroll(0);
        controls.Player._1.performed += _ => Scroll(1);
        controls.Player._2.performed += _ => Scroll(2);
        controls.Player._3.performed += _ => Scroll(3);
        controls.Player.Reload.performed += _ => Reload();
        controls.Player.Drop.performed += _ => DropWeapon(currentIndex);
        controls.Player.UseMedkit.performed += _ => UseMedkit(true);
        controls.Player.UseMedkit.canceled += _ => UseMedkit(false);
        controls.Player.UseSyringe.performed += _ => UseSyringe(true);
        controls.Player.UseSyringe.canceled += _ => UseSyringe(false);

        syringeCount = RulesManager.instance.startingSyringes;
        medkitCount = RulesManager.instance.startingMedkits;
        lightAmmoCount = RulesManager.instance.startingLightAmmo;
        mediumAmmoCount = RulesManager.instance.startingMediumAmmo;
        heavyAmmoCount = RulesManager.instance.startingHeavyAmmo;
        shellsAmmoCount = RulesManager.instance.startingShellsAmmo;

        UpdateWeaponVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ammo"))
        {
            PickupAmmo(other.GetComponent<Ammo>());
        } else if (other.CompareTag("Heal")) {
            PickupHeal(other.GetComponent<Healable>());
        }
    }

    void UseMedkit(bool pressed)
    {
        if (medkitCount > 0)
        {
            currentMedkitTime = 0f;
            currentSyringeTime = 0f;
            wantsToUseMedkit = pressed;
            wantsToUseSyringe = false;
        } else
        {
            currentMedkitTime = 0f;
            currentSyringeTime = 0f;
            wantsToUseMedkit = false;
            wantsToUseSyringe = false;
        }

        if (!pressed)
        {
            currentMedkitTime = 0f;
        }
    }

    void UseSyringe(bool pressed)
    {
        if (syringeCount > 0)
        {
            currentSyringeTime = 0f;
            currentMedkitTime = 0f;
            wantsToUseSyringe = pressed;
            wantsToUseMedkit = false;
        } else
        {
            currentMedkitTime = 0f;
            currentSyringeTime = 0f;
            wantsToUseSyringe = false;
            wantsToUseMedkit = false;
        }

        if (!pressed)
        {
            currentSyringeTime = 0f;
        }
    }

    void Scroll(float amount)
    {
        if (canSwitch && sinceLastSwitch > whenCanSwitch)
        {
            Destroy(currentReloadSound);
            wantsToReload = false;

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
                sfx.PlaySound(inventoryItem[currentIndex].weapon.pickupSound, 35, 1);
                PlayerHoldChanged();
            } else {
                Scroll(amount);
            }
            
        }
    }

    public void Scroll(int indexToSwapTo)
    {
        if (canSwitch && sinceLastSwitch > whenCanSwitch && indexToSwapTo != currentIndex && inventoryItem[indexToSwapTo].weapon != null)
        {
            Destroy(currentReloadSound);
            wantsToReload = false;

            currentIndex = indexToSwapTo;
            
            if (inventoryItem[currentIndex].weapon != null)
            {
                SoundEffect sfx = Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>();
                sfx.PlaySound(inventoryItem[currentIndex].weapon.pickupSound, 35, 1);
            }

            PlayerHoldChanged();
        }
    }

    void PlayerHoldChanged()
    {
        Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerHoldItem);

        msg.AddUShort(NetworkManager.instance.Client.Id);

        if (inventoryItem[currentIndex].weapon != null)
        {
            msg.AddInt(inventoryItem[currentIndex].weapon.id);
        } else
        {
            msg.AddInt(-1);
        }
        
        NetworkManager.instance.Client.Send(msg);
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.instance.gameIsStarted)
            return;

        sinceLastSwitch += Time.fixedDeltaTime;

        if (wantsToUseMedkit)
        {
            playerController.healingMove = .25f;
            currentMedkitTime += Time.fixedDeltaTime;
            if (currentMedkitTime > medkitTime)
            {
                currentMedkitTime = 0f;
                wantsToUseMedkit = false;
                medkitCount--;
                GetComponent<HealthManager>().Heal(150, false);

                Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.soundEffect);
                msg.AddVector3(transform.position);
                msg.AddInt(11);
                msg.AddFloat(0.7f);
                msg.AddFloat(30);
                NetworkManager.instance.Client.Send(msg);

                GameManager.instance.PlaySoundEffectByID(transform.position, 11, 0.7f, 30);
            }
        } 
        else if (wantsToUseSyringe)
        {
            playerController.healingMove = .55f;
            currentSyringeTime += Time.fixedDeltaTime;
            if (currentSyringeTime > syringeTime)
            {
                currentSyringeTime = 0;
                wantsToUseSyringe = false;
                syringeCount--;
                GetComponent<HealthManager>().Heal(30, false);

                Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.soundEffect);
                msg.AddVector3(transform.position);
                msg.AddInt(10);
                msg.AddFloat(0.7f);
                msg.AddFloat(30);
                NetworkManager.instance.Client.Send(msg);

                GameManager.instance.PlaySoundEffectByID(transform.position, 10, 0.7f, 30);
            }
        } else
        {
            playerController.healingMove = 1;
        }

        UpdateWeaponVisual();

        reloadCooldown -= Time.fixedDeltaTime;

        if (wantsToReload && reloadCooldown < 0)
        {
            int currentAmmo = inventoryItem[currentIndex].ammoCount;

            int diff = inventoryItem[currentIndex].weapon.maxAmmoCount - currentAmmo;

            switch (inventoryItem[currentIndex].weapon.ammoType)
            {
                case AmmoType.Light:
                    if (lightAmmoCount > 0)
                    {
                        if (diff < lightAmmoCount)
                        {
                            lightAmmoCount -= diff;
                            inventoryItem[currentIndex].ammoCount += diff;
                        }
                        else
                        {
                            inventoryItem[currentIndex].ammoCount += lightAmmoCount;
                            lightAmmoCount = 0;
                        }
                    }
                    break;
                case AmmoType.Medium:
                    if (mediumAmmoCount > 0)
                    {
                        if (diff < mediumAmmoCount)
                        {
                            mediumAmmoCount -= diff;
                            inventoryItem[currentIndex].ammoCount += diff;
                        }
                        else
                        {
                            inventoryItem[currentIndex].ammoCount += mediumAmmoCount;
                            mediumAmmoCount = 0;
                        }
                    }
                    break;
                case AmmoType.Heavy:
                    if (heavyAmmoCount > 0)
                    {
                        if (diff < heavyAmmoCount)
                        {
                            heavyAmmoCount -= diff;
                            inventoryItem[currentIndex].ammoCount += diff;
                        }
                        else
                        {
                            inventoryItem[currentIndex].ammoCount += heavyAmmoCount;
                            heavyAmmoCount = 0;
                        }
                    }
                    break;
                case AmmoType.Shells:
                    if (shellsAmmoCount > 0)
                    {
                        if (diff < shellsAmmoCount)
                        {
                            shellsAmmoCount -= diff;
                            inventoryItem[currentIndex].ammoCount += diff;
                        }
                        else
                        {
                            inventoryItem[currentIndex].ammoCount += shellsAmmoCount;
                            shellsAmmoCount = 0;
                        }
                    }
                    break;
            }

            wantsToReload = false;
        }
    }

    void UpdateWeaponVisual()
    {
        UpdateSlot(1);
        UpdateSlot(2);
        UpdateSlot(3);

        if (inventoryItem[currentIndex].weapon != null)
        {
            gunMeshFilter.mesh = inventoryItem[currentIndex].weapon.gunMesh;
            gunMeshRenderer.material = inventoryItem[currentIndex].weapon.gunMaterial;
        }

        slots[0].alpha = 0.45f;
        slots[1].alpha = 0.45f;
        slots[2].alpha = 0.45f;
        slots[3].alpha = 0.45f;

        slots[currentIndex].alpha = 1f;

        medkitCountText.text = medkitCount.ToString();
        medkitSlider.value = currentMedkitTime;
        syringeCountText.text = syringeCount.ToString();
        syringeSlider.value = currentSyringeTime;
    }

    void UpdateSlot(int pos)
    {
        if (inventoryItem[pos].weapon != null)
        {
            ammoCount[pos - 1].text = inventoryItem[pos].ammoCount.ToString();
            gunName[pos - 1].text = inventoryItem[pos].weapon.gunName;
            switch (inventoryItem[pos].weapon.ammoType)
            {
                case AmmoType.Light:
                    totalAmmoCount[pos - 1].text = (lightAmmoCount).ToString();
                    break;
                case AmmoType.Medium:
                    totalAmmoCount[pos - 1].text = (mediumAmmoCount).ToString();
                    break;
                case AmmoType.Heavy:
                    totalAmmoCount[pos - 1].text = (heavyAmmoCount).ToString();
                    break;
                case AmmoType.Shells:
                    totalAmmoCount[pos - 1].text = (shellsAmmoCount).ToString();
                    break;
            }

            genericMarker[pos - 1].SetActive(false);
            rareMarker[pos - 1].SetActive(false);
            legendaryMarker[pos - 1].SetActive(false);

            switch (inventoryItem[pos].weapon.rarity)
            {
                case Weapon.Rarity.generic:
                    slotsImage[pos - 1].color = genericColor;
                    break;
                case Weapon.Rarity.rare:
                    slotsImage[pos - 1].color = rareColor;
                    break;
                case Weapon.Rarity.legendary:
                    slotsImage[pos - 1].color = legendaryColor;
                    break;
            }
        }
        else
        {
            genericMarker[pos - 1].SetActive(false);
            rareMarker[pos - 1].SetActive(false);
            legendaryMarker[pos - 1].SetActive(false);
            ammoCount[pos - 1].text = "- -";
            gunName[pos - 1].text = "- - - - -";
            gunMeshFilter.mesh = null;
            gunMeshRenderer.material = null;
            totalAmmoCount[pos - 1].text = "- -";
            slotsImage[pos - 1].color = noneColor;
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
        if (!foundSpot && currentIndex != 0)
        {
            Instantiate(groundItem, transform.position, Quaternion.identity).GetComponent<GroundItem>().UpdateItem(inventoryItem[currentIndex]);

            inventoryItem[currentIndex] = pickedUpItem;

            foundSpot = true;
        }
        
        if (foundSpot)
            groundItem.Pickup(false);

        PlayerHoldChanged();
    }

    public void PickupHeal(Healable heal)
    {
        if (heal.type == Healable.HealType.Medkit)
        {
            medkitCount += heal.count;
        } else
        {
            syringeCount += heal.count;
        }

        heal.Pickup(false);
    }

    public void PickupAmmo(Ammo ammo) { 
        switch (ammo.type)
        {
            case AmmoType.Light:
                lightAmmoCount += ammo.count;
                break;
            case AmmoType.Medium:
                mediumAmmoCount += ammo.count;
                break;
            case AmmoType.Heavy:
                heavyAmmoCount += ammo.count;
                break;
            case AmmoType.Shells:
                shellsAmmoCount += ammo.count;
                break;
        }
        ammo.Pickup(false);
    }

    public void DropWeapon(int index)
    {
        if (inventoryItem[index].weapon != null && currentIndex != 0)
        {
            Instantiate(groundItem, transform.position, Quaternion.identity).GetComponent<GroundItem>().UpdateItem(inventoryItem[index]);

            inventoryItem[index].weapon = null;

            Scroll(1.0f);
        }

        PlayerHoldChanged();
    }

    public void Reload()
    {
        if (inventoryItem[currentIndex].weapon == null || wantsToReload || currentIndex == 0)
            return;

        switch (inventoryItem[currentIndex].weapon.ammoType)
        {
            case AmmoType.Light:
                if (lightAmmoCount > 0)
                {
                    wantsToReload = true;
                    reloadCooldown = inventoryItem[currentIndex].weapon.reloadTime;

                    currentReloadSound = Instantiate(soundEffect, transform.position, Quaternion.identity, transform);
                    currentReloadSound.GetComponent<SoundEffect>().PlaySound(inventoryItem[currentIndex].weapon.reloadSound, 35, 1);

                    Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerReloadSound);
                    msg.AddUShort(NetworkManager.instance.Client.Id);
                    msg.AddInt(inventoryItem[currentIndex].weapon.id);
                    NetworkManager.instance.Client.Send(msg);
                }
                break;
            case AmmoType.Medium:
                if (mediumAmmoCount > 0)
                {
                    wantsToReload = true;
                    reloadCooldown = inventoryItem[currentIndex].weapon.reloadTime;

                    currentReloadSound = Instantiate(soundEffect, transform.position, Quaternion.identity, transform);
                    currentReloadSound.GetComponent<SoundEffect>().PlaySound(inventoryItem[currentIndex].weapon.reloadSound, 35, 1);

                    Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerReloadSound);
                    msg.AddUShort(NetworkManager.instance.Client.Id);
                    msg.AddInt(inventoryItem[currentIndex].weapon.id);
                    NetworkManager.instance.Client.Send(msg);
                }
                break;
            case AmmoType.Heavy:
                if (heavyAmmoCount > 0)
                {
                    wantsToReload = true;
                    reloadCooldown = inventoryItem[currentIndex].weapon.reloadTime;

                    currentReloadSound = Instantiate(soundEffect, transform.position, Quaternion.identity, transform);
                    currentReloadSound.GetComponent<SoundEffect>().PlaySound(inventoryItem[currentIndex].weapon.reloadSound, 35, 1);

                    Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerReloadSound);
                    msg.AddUShort(NetworkManager.instance.Client.Id);
                    msg.AddInt(inventoryItem[currentIndex].weapon.id);
                    NetworkManager.instance.Client.Send(msg);
                }
                break;
            case AmmoType.Shells:
                if (shellsAmmoCount > 0)
                {
                    wantsToReload = true;
                    reloadCooldown = inventoryItem[currentIndex].weapon.reloadTime;

                    currentReloadSound = Instantiate(soundEffect, transform.position, Quaternion.identity, transform);
                    currentReloadSound.GetComponent<SoundEffect>().PlaySound(inventoryItem[currentIndex].weapon.reloadSound, 35, 1);

                    Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerReloadSound);
                    msg.AddUShort(NetworkManager.instance.Client.Id);
                    msg.AddInt(inventoryItem[currentIndex].weapon.id);
                    NetworkManager.instance.Client.Send(msg);
                }
                break;
        }
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        LocalPlayerController.onDisablePlayerInput -= controls.Disable;
        LocalPlayerController.onEnablePlayerInput -= controls.Enable;

        if (controls != null)
            controls.Disable();
    }
}
