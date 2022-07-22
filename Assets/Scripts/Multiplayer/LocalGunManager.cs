using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalGunManager : MonoBehaviour
{
    private Controls controls;

    private LocalPlayerController playerController;
    private LocalInventoryManager im;
    public LayerMask lm;
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
        if (!NetworkManager.instance.gameIsStarted)
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

            ToolTip tip = hit.collider.GetComponent<ToolTip>();

            if (tip != null && (hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Item")))
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
            Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out lookDir, Mathf.Infinity, lm);
            gunPivot.LookAt(lookDir.point);

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
                    Instantiate(muzzleFlash, gunPivot.position + gunPivot.transform.TransformDirection(im.inventoryItem[im.currentIndex].weapon.muzzleLocation), gunPivot.rotation);
                    shootCooldown = im.inventoryItem[im.currentIndex].weapon.timeBetweenShots;

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

                        Physics.Raycast(transform.position, dir, out shoot, Mathf.Infinity, lm);

                        if (shoot.collider != null)
                        {
                            Instantiate(impactEffect, shoot.point, Quaternion.LookRotation(Vector3.forward, shoot.normal));

                            if (shoot.collider.CompareTag("RemotePlayer"))
                            {
                                Damage(im.inventoryItem[im.currentIndex].weapon.damage);
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

    void Damage(int damage)
    {
        Debug.Log("Damage " + damage);
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
