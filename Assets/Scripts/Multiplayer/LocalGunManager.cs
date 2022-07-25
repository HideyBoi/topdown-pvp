using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class LocalGunManager : MonoBehaviour
{
    private Controls controls;

    private LocalPlayerController playerController;
    private LocalInventoryManager im;
    private HealthManager hm;
    public LayerMask lm;
    public LayerMask gunLm;
    public Animator interactUiAnimation;
    public Slider interactProgressbar;

    bool pressingInteract = false;
    float timeHeld = 0f;
    float interactCooldown = 0.7f;

    bool wantsToShoot = false;
    public float shootCooldown;

    public Transform gunPivot;
    public Transform pivot;

    public GameObject muzzleFlash;
    public GameObject impactEffect;
    public GameObject soundEffect;


    void Awake()
    {
        controls = new Controls();
        controls.Player.Interact.performed += _ => InteractButton(true);
        controls.Player.Interact.canceled += _ => InteractButton(false);
        controls.Player.Shoot.performed += _ => Shoot(true);
        controls.Player.Shoot.canceled += _ => Shoot(false);

        playerController = GetComponent<LocalPlayerController>();
        im = GetComponent<LocalInventoryManager>();
        hm = GetComponent<HealthManager>();
    }

    private void InteractButton(bool press)
    {
        pressingInteract = press;
    }

    public ToolTip lastTip;
    public ToolTip currentTip;

    void Shoot(bool isPressing)
    {
        wantsToShoot = isPressing;
    }

    private void Update()
    {
        if (!NetworkManager.instance.gameIsStarted || hm.isDead)
            return;

        RaycastHit hit;

        Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out hit, 3.5f, lm);

        interactCooldown -= Time.deltaTime;
        shootCooldown -= Time.deltaTime;

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Interactable") && pressingInteract)
            {
                interactUiAnimation.SetBool("interacting", true);

                timeHeld += Time.fixedDeltaTime;

                interactProgressbar.value = timeHeld;

                if (timeHeld > 1.5f)
                {
                    hit.collider.tag = "NonInteractable";
                    Chest chest = hit.collider.GetComponent<Chest>();
                    chest.Open(false);

                    interactCooldown = 0.78f;
                }
            }
            else
            {
                interactUiAnimation.SetBool("interacting", false);
                timeHeld = 0f;
                interactProgressbar.value = timeHeld;
            }

            if (hit.collider.CompareTag("Item") && interactCooldown < 0 && pressingInteract)
            {
                interactCooldown = 0.7f;
                im.PickupWeapon(hit.collider.GetComponent<GroundItem>());
            }

            if (hit.collider.CompareTag("Heal") && interactCooldown < 0 && pressingInteract)
            {
                interactCooldown = 0.4f;
                im.PickupHeal(hit.collider.GetComponent<Healable>());
            }

            ToolTip tip = hit.collider.GetComponent<ToolTip>();

            if (tip != null && (hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Item") || hit.collider.CompareTag("Heal") || hit.collider.CompareTag("Ammo")))
            {
                if (lastTip == null)
                {
                    currentTip = tip;
                    lastTip = tip;
                    tip.IsAimedAt(true);
                }
                else {
                    if (currentTip.GetInstanceID() != lastTip.GetInstanceID())
                    {
                        lastTip.IsAimedAt(false);
                        lastTip = null;
                    } else
                    {
                        lastTip = currentTip;
                        currentTip.IsAimedAt(true);
                    }
                }
            }  else
            {
                if (lastTip != null)
                {
                    lastTip.IsAimedAt(false);
                    lastTip = null;
                }
            }
            
        } else
        {
            interactUiAnimation.SetBool("interacting", false);

            if (lastTip != null)
            {

                lastTip.IsAimedAt(false);
                lastTip = null;
            }
        }

        if (im.inventoryItem[im.currentIndex].weapon != null)
        {
            RaycastHit lookDir;
            Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out lookDir, Mathf.Infinity, gunLm);
            gunPivot.LookAt(lookDir.point);

            Message gunRot = Message.Create(MessageSendMode.unreliable, NetworkManager.MessageIds.playerGunRot, shouldAutoRelay: true);
            gunRot.AddUShort(playerController.id);
            gunRot.AddQuaternion(gunPivot.rotation);
            NetworkManager.instance.Client.Send(gunRot);

            if (shootCooldown < 0)
            {
                im.canSwitch = true;
            }
            else
            {
                im.canSwitch = false;
            }

            if (wantsToShoot && im.reloadCooldown < 0 && shootCooldown < 0)
            {
                if (im.inventoryItem[im.currentIndex].ammoCount > 0)
                {
                    im.inventoryItem[im.currentIndex].ammoCount--;
                    Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(im.inventoryItem[im.currentIndex].weapon.shootSound);
                    Instantiate(muzzleFlash, gunPivot.position + gunPivot.transform.TransformDirection(im.inventoryItem[im.currentIndex].weapon.muzzleLocation), gunPivot.rotation, gunPivot);
                    shootCooldown = im.inventoryItem[im.currentIndex].weapon.timeBetweenShots;

                    Message muzzleFlashMsg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.playerShot, shouldAutoRelay: true);
                    muzzleFlashMsg.AddUShort(playerController.id);
                    muzzleFlashMsg.AddInt(im.inventoryItem[im.currentIndex].weapon.id);
                    muzzleFlashMsg.AddVector3(gunPivot.position + gunPivot.transform.TransformDirection(im.inventoryItem[im.currentIndex].weapon.muzzleLocation));
                    muzzleFlashMsg.AddQuaternion(gunPivot.rotation);
                    NetworkManager.instance.Client.Send(muzzleFlashMsg);

                    if (!im.inventoryItem[im.currentIndex].weapon.automatic)
                    {
                        wantsToShoot = false;
                    }

                    for (int i = 0; i <= im.inventoryItem[im.currentIndex].weapon.shotCount; i++)
                    {
                        RaycastHit shoot;

                        Vector3 dir = new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y);
                        dir = pivot.InverseTransformDirection(dir);
                        dir.x += Random.Range(-im.inventoryItem[im.currentIndex].weapon.spread, im.inventoryItem[im.currentIndex].weapon.spread);
                        dir = pivot.TransformDirection(dir);

                        Physics.Raycast(transform.position, dir, out shoot, Mathf.Infinity, gunLm);

                        if (shoot.collider != null)
                        {
                            Instantiate(impactEffect, shoot.point, Quaternion.LookRotation(Vector3.forward, shoot.normal));

                            if (shoot.collider.CompareTag("RemotePlayer"))
                            {
                                HealthManager hm = shoot.collider.GetComponent<HealthManager>();
                                Damage(im.inventoryItem[im.currentIndex].weapon.damage, im.inventoryItem[im.currentIndex].weapon.id, hm);
                            }
                        }
                    }
                }
                else
                {
                    if (!im.wantsToReload)
                        im.Reload();
                }
            }
        }
    }

    void Damage(int damage, int gunId, HealthManager hm)
    {
        hm.Damage(damage, playerController.id, gunId, false);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }
}
