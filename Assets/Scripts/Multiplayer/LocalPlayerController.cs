using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Riptide;
using Riptide.Utils;
using System;

public class LocalPlayerController : MonoBehaviour
{

    private Controls controls;
    public static LocalPlayerController instance;

    Vector2 desMoveDir;
    public Vector2 lookDir;
    Rigidbody rb;
    public Transform pivot;

    public float currentMovementSpeed;
    public Camera cam;

    public RectTransform cursor;

    public LayerMask lm;

    public Camera miniMapCam;

    NetworkManager nm;

    public ushort id;

    HealthManager hm;
    LocalInventoryManager inventory;

    bool wantsToMove;
    float currentMoveTimer;
    public float moveTimer;

    public LayerMask groundLm;

    public GameObject waterSploosh;
    public Transform waterSplooshEmitter;

    public Animator playerAnimator;
    public Animator cosmeticsAnimator;
    public CosmeticsHandler cosmeticsHandler;

    public static event Action onEnablePlayerInput;
    public static event Action onDisablePlayerInput;

    public bool controlsEnabled = true;

    public static void EnablePlayerInput()
    {
        if (onEnablePlayerInput != null)
            onEnablePlayerInput();
    }

    public static void DisablePlayerInput()
    {
        if (onDisablePlayerInput != null)
            onDisablePlayerInput();
    }

    void Disable() { controls.Disable(); instance.controlsEnabled = false; }
    void Enable() { controls.Enable(); instance.controlsEnabled = true; }

    private void Awake()
    {
        instance = this;
        nm = NetworkManager.instance;
        id = nm.Client.Id;
        controls = new Controls();
        rb = GetComponent<Rigidbody>();
        hm = GetComponent<HealthManager>();
        inventory = GetComponent<LocalInventoryManager>();
        cosmeticsHandler.LoadCosmetics();

        onDisablePlayerInput += Disable;
        onEnablePlayerInput += Enable;

        controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>(), true);
        controls.Player.Move.canceled += ctx => Move(ctx.ReadValue<Vector2>(), false);
        controls.Player.PointGamepad.performed += ctx => PointGamepad(ctx.ReadValue<Vector2>());
        controls.Player.PointMouse.performed += ctx => PointMouse(ctx.ReadValue<Vector2>());
    }

    private void FixedUpdate()
    {
        if (!nm.gameIsStarted)
            return;

        if (!hm.isDead)
        {
            if (inventory.inventoryItem[inventory.currentIndex].weapon)
            {
                rb.AddForce(new Vector3(desMoveDir.x, 0, desMoveDir.y) * currentMovementSpeed * inventory.inventoryItem[inventory.currentIndex].weapon.speedModifier, ForceMode.VelocityChange);     
            } else
            {
                rb.AddForce(new Vector3(desMoveDir.x, 0, desMoveDir.y) * currentMovementSpeed, ForceMode.VelocityChange);
            }
            

            RaycastHit hit;
            Physics.Raycast(transform.position, new Vector3(lookDir.x, 0, lookDir.y), out hit, Mathf.Infinity, lm);

            cursor.position = cam.WorldToScreenPoint(hit.point);
        }

        Vector3 localSpaceMoveDir = pivot.InverseTransformVector(new Vector3(desMoveDir.x, 0, desMoveDir.y));

        playerAnimator.SetFloat("MoveDirX", localSpaceMoveDir.x);
        playerAnimator.SetFloat("MoveDirY", localSpaceMoveDir.z);
        playerAnimator.SetFloat("MoveDirMag", localSpaceMoveDir.sqrMagnitude);
        
        cosmeticsAnimator.SetFloat("MoveDirX", localSpaceMoveDir.x);
        cosmeticsAnimator.SetFloat("MoveDirY", localSpaceMoveDir.z);
        cosmeticsAnimator.SetFloat("MoveDirMag", localSpaceMoveDir.sqrMagnitude);

        Message playerPosRot = Message.Create(MessageSendMode.Unreliable, NetworkManager.MessageIds.playerPos);
        playerPosRot.AddUShort(id);
        playerPosRot.AddVector3(transform.position);
        playerPosRot.AddVector3(desMoveDir);
        playerPosRot.AddQuaternion(pivot.rotation);
        nm.Client.Send(playerPosRot);

        currentMoveTimer -= Time.fixedDeltaTime * ((rb.velocity.magnitude/8 - 0f) / (currentMovementSpeed - 0f));

        if (currentMoveTimer < 0 && wantsToMove)
        {
            currentMoveTimer = moveTimer;

            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 8f, groundLm);

            if (hit.collider != null)
            {
                int rng = 0;

                if (hit.collider.CompareTag("Water"))
                {
                    rng = UnityEngine.Random.Range(5, 10);
                } else
                {   
                    rng = UnityEngine.Random.Range(0, 5);
                }

                Message msg = Message.Create(MessageSendMode.Reliable, NetworkManager.MessageIds.soundEffect);
                msg.AddVector3(transform.position);
                msg.AddInt(rng);
                msg.AddFloat(1f);
                msg.AddFloat(90);
                NetworkManager.instance.Client.Send(msg);

                GameManager.instance.PlaySoundEffectByID(waterSplooshEmitter.position, rng, 1f, 90);
            }
        }
    }

    private void Update()
    {
        if (!nm.gameIsStarted || hm.isDead)
            return;

        miniMapCam.transform.position = new Vector3(transform.position.x, 200, transform.position.z);
        miniMapCam.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }

    void Move(Vector2 rawdir, bool moving)
    {
        desMoveDir = rawdir;
        wantsToMove = moving;
    }

    void PointMouse(Vector2 mousePos)
    {
        Vector3 mouseLookDir = transform.position - cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.transform.position.y));
        Point(new Vector2(-mouseLookDir.x, -mouseLookDir.z));
    }

    void PointGamepad(Vector2 pointDir)
    {
        if (pointDir.magnitude == 0)
            return;
        Point(pointDir);
    }

    void Point(Vector2 dir)
    {
        lookDir = dir;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        //transform.rotation = Quaternion.Euler(new Vector3(0, -angle + 90, 0));
        pivot.rotation = Quaternion.Euler(new Vector3(0, -angle + 90, 0));
    }

    private void OnEnable()
    {
        controls.Enable();
        controlsEnabled = true;
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }
}
