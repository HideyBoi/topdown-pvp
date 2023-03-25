using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Riptide;
using Riptide.Utils;
using EZCameraShake;
using Cinemachine;

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

    public Transform camOffsetTransform;
    public float camOffset;
    public float zoomAmount;
    public float currentFOV;
    public float normalFOV;
    public float adsFOV;
    public CinemachineVirtualCamera cam;
    public CinemachineFramingTransposer transposor;
    public Transform gunPivot;
    public Transform pivot;

    public GameObject muzzleFlash;
    public GameObject bulletTracer;
    public GameObject impactEffect;
    public GameObject bloodEffect;
    public GameObject soundEffect;


    void Awake()
    {
        controls = new Controls();
        controls.Player.Interact.performed += _ => InteractButton(true);
        controls.Player.Interact.canceled += _ => InteractButton(false);
        controls.Player.Shoot.performed += _ => Shoot(true);
        controls.Player.Shoot.canceled += _ => Shoot(false);
        //controls.Player.Aim.performed += _ => Aim(true);
        //controls.Player.Aim.canceled += _ => Aim(false);


        LocalPlayerController.onDisablePlayerInput += controls.Disable;
        LocalPlayerController.onEnablePlayerInput += controls.Enable;

        playerController = GetComponent<LocalPlayerController>();
        im = GetComponent<LocalInventoryManager>();
        hm = GetComponent<HealthManager>();
    }

    private void InteractButton(bool press)
    {
        pressingInteract = press;
    }

    void Aim(bool isAiming)
    {
        if (isAiming)
        {
            camOffset = zoomAmount;
            currentFOV = adsFOV;
        } else
        {
            camOffset = 0;
            currentFOV = normalFOV;
        }
    }

    void Shoot(bool isPressing)
    {
        wantsToShoot = isPressing;
    }

    private void Update()
    {
        if (!NetworkManager.instance.gameIsStarted || hm.isDead)
            return;

        /*
        cam.m_Lens.FieldOfView = Mathf.Lerp(cam.m_Lens.FieldOfView, currentFOV, Time.deltaTime*4);

        if (transposor != null)
        {
            Vector2 V2lookDir = playerController.lookDir.normalized;
            Vector3 V3lookDir = new Vector3(V2lookDir.x * camOffset, 8, V2lookDir.y * camOffset); 
            transposor.m_TrackedObjectOffset = Vector3.Lerp(transposor.m_TrackedObjectOffset, V3lookDir, Time.deltaTime * 4);
            transposor.m_TrackedObjectOffset.y = 8;
        } else
        {
            transposor = cam.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        */


        RaycastHit hit;

        Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out hit, 3.5f, lm);

        interactCooldown -= Time.deltaTime;
        shootCooldown -= Time.deltaTime;

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Interactable") && pressingInteract)
            {
                interactUiAnimation.SetBool("interacting", true);

                timeHeld += Time.deltaTime;

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
                interactCooldown = 0.1f;
                im.PickupWeapon(hit.collider.GetComponent<GroundItem>());
            }

            if (hit.collider.CompareTag("Heal") && interactCooldown < 0 && pressingInteract)
            {
                interactCooldown = 0.1f;
                im.PickupHeal(hit.collider.GetComponent<Healable>());
            }

            if (hit.collider.CompareTag("Ammo") && interactCooldown < 0 && pressingInteract)
            {
                interactCooldown = 0.1f;
                im.PickupAmmo(hit.collider.GetComponent<Ammo>());
            }

            ToolTip tip = hit.collider.GetComponent<ToolTip>();

            if (tip != null && (hit.collider.CompareTag("Interactable") || hit.collider.CompareTag("Item") || hit.collider.CompareTag("Heal") || hit.collider.CompareTag("Ammo")))
            {
                tip.IsAimedAt(true);
            }
        } else
        {
            interactUiAnimation.SetBool("interacting", false);
        }

        if (im.inventoryItem[im.currentIndex].weapon != null)
        {
            if (im.currentIndex == 0)
            {

                RaycastHit lookDir;
                Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out lookDir, Mathf.Infinity, gunLm);
                gunPivot.LookAt(lookDir.point);

                Message gunRot = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerGunRot);
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
                    wantsToShoot = false;
                    shootCooldown = im.inventoryItem[im.currentIndex].weapon.timeBetweenShots;

                    RaycastHit shoot;

                    Vector3 dir = new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y);
                    dir.Normalize();
                    dir = pivot.InverseTransformDirection(dir);
                    dir.x += Random.Range(-im.inventoryItem[im.currentIndex].weapon.spread, im.inventoryItem[im.currentIndex].weapon.spread);
                    dir = pivot.TransformDirection(dir);

                    Physics.Raycast(transform.position, dir, out shoot, 5f, gunLm);

                    if (shoot.collider != null)
                    {
                        CameraShaker.Instance.ShakeOnce(im.inventoryItem[im.currentIndex].weapon.magnitude, im.inventoryItem[im.currentIndex].weapon.roughness, im.inventoryItem[im.currentIndex].weapon.shakeFadeIn, im.inventoryItem[im.currentIndex].weapon.shakeFadeOut);

                        Message punch = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.punch);
                        punch.AddBool(shoot.collider.CompareTag("RemotePlayer"));
                        punch.AddVector3(shoot.point);
                        punch.AddQuaternion(Quaternion.LookRotation(Vector3.forward, shoot.normal));
                        NetworkManager.instance.Client.Send(punch);

                        if (shoot.collider.CompareTag("RemotePlayer"))
                        {
                            Instantiate(bloodEffect, shoot.point, Quaternion.LookRotation(shoot.normal, Vector3.up));
                            Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(im.inventoryItem[im.currentIndex].weapon.shootSound, 45, 1);

                            HealthManager hm = shoot.collider.GetComponent<HealthManager>();
                            Damage(im.inventoryItem[im.currentIndex].weapon.damage, im.inventoryItem[im.currentIndex].weapon.id, hm);
                        } else
                        {
                            Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(im.inventoryItem[im.currentIndex].weapon.reloadSound, 30, 1);
                        }
                    }
                }

            } 
            else
            {
                RaycastHit lookDir;
                Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out lookDir, Mathf.Infinity, gunLm);
                gunPivot.LookAt(lookDir.point);

                Message gunRot = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerGunRot);
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
                        Instantiate(soundEffect, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(im.inventoryItem[im.currentIndex].weapon.shootSound, 60, 1);
                        Instantiate(muzzleFlash, gunPivot.position + gunPivot.transform.TransformDirection(im.inventoryItem[im.currentIndex].weapon.muzzleLocation), gunPivot.rotation, gunPivot);
                        shootCooldown = im.inventoryItem[im.currentIndex].weapon.timeBetweenShots;


                        if (!im.inventoryItem[im.currentIndex].weapon.automatic)
                        {
                            wantsToShoot = false;
                        }

                        for (int i = 0; i <= im.inventoryItem[im.currentIndex].weapon.shotCount; i++)
                        {
                            RaycastHit shoot;

                            Vector3 dir = new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y);
                            dir.Normalize();
                            dir = pivot.InverseTransformDirection(dir);
                            dir.x += Random.Range(-im.inventoryItem[im.currentIndex].weapon.spread, im.inventoryItem[im.currentIndex].weapon.spread);
                            dir = pivot.TransformDirection(dir);

                            Physics.Raycast(transform.position, dir, out shoot, Mathf.Infinity, gunLm);

                            if (shoot.collider != null)
                            {
                                Instantiate(impactEffect, shoot.point, Quaternion.LookRotation(Vector3.forward, shoot.normal));

                                CameraShaker.Instance.ShakeOnce(im.inventoryItem[im.currentIndex].weapon.magnitude, im.inventoryItem[im.currentIndex].weapon.roughness, im.inventoryItem[im.currentIndex].weapon.shakeFadeIn, im.inventoryItem[im.currentIndex].weapon.shakeFadeOut);

                                Message muzzleFlashMsg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.playerShot);
                                muzzleFlashMsg.AddUShort(playerController.id);
                                muzzleFlashMsg.AddInt(im.inventoryItem[im.currentIndex].weapon.id);
                                muzzleFlashMsg.AddVector3(gunPivot.position + gunPivot.transform.TransformDirection(im.inventoryItem[im.currentIndex].weapon.muzzleLocation));
                                muzzleFlashMsg.AddQuaternion(gunPivot.rotation);
                                muzzleFlashMsg.AddVector3(shoot.point);
                                NetworkManager.instance.Client.Send(muzzleFlashMsg);

                                Instantiate(bulletTracer, gunPivot.position + gunPivot.transform.TransformDirection(im.inventoryItem[im.currentIndex].weapon.muzzleLocation), Quaternion.identity).GetComponent<BulletTracer>().SetData(shoot.point);

                                int damage = im.inventoryItem[im.currentIndex].weapon.damage;
                                
                                if (RulesManager.instance.doWeaponDropoff)
                                {
                                    float dist = Vector3.Distance(transform.position, shoot.point);
                                    int distanceDiminish = (int)(dist * im.inventoryItem[im.currentIndex].weapon.damageFalloff);

                                    if (distanceDiminish > damage)
                                    {
                                        damage = 0;
                                    }
                                    else
                                    {
                                        damage -= distanceDiminish;
                                    }

                                    Debug.Log($"{damage} : {distanceDiminish} : {im.inventoryItem[im.currentIndex].weapon.damage} | {dist}");
                                }

                                if (shoot.collider.CompareTag("RemotePlayer"))
                                {
                                    Instantiate(bloodEffect, shoot.point, Quaternion.LookRotation(shoot.normal, Vector3.up));

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
