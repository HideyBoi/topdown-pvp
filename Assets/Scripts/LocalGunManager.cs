using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalGunManager : MonoBehaviour
{
    private Controls controls;

    private LocalPlayerController playerController;
    private LocalInventoryManager inventoryManager;
    public LayerMask lm;
    public Animator interactUiAnimation;
    public Slider interactProgressbar;

    bool pressingInteract = false;
    float timeHeld = 0f;
    float interactCooldown = 0.7f;

    void Awake()
    {
        controls = new Controls();
        controls.Player.Interact.performed += _ => InteractButton(true);
        controls.Player.Interact.canceled += _ => InteractButton(false);

        playerController = GetComponent<LocalPlayerController>();
        inventoryManager = GetComponent<LocalInventoryManager>();
    }

    private void InteractButton(bool press)
    {
        pressingInteract = press;
    }

    private void FixedUpdate()
    {
        if (!NetworkManager.instance.gameIsStarted)
            return;

        RaycastHit hit;

        Physics.Raycast(transform.position, new Vector3(playerController.lookDir.x, 0, playerController.lookDir.y), out hit, 2f, lm);

        interactCooldown -= Time.fixedDeltaTime;

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
                    chest.Open();

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
                inventoryManager.PickupWeapon(hit.collider.GetComponent<GroundItem>());
            }
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
