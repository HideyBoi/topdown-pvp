using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Riptide;
using Riptide.Utils;

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

    private void Awake()
    {
        instance = this;
        nm = NetworkManager.instance;
        id = nm.Client.Id;
        controls = new Controls();
        rb = GetComponent<Rigidbody>();
        hm = GetComponent<HealthManager>();
        inventory = GetComponent<LocalInventoryManager>();

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
            rb.AddForce(new Vector3(desMoveDir.x, 0, desMoveDir.y) * currentMovementSpeed * inventory.inventoryItem[inventory.currentIndex].weapon.speedModifier, ForceMode.VelocityChange);
            //rb.AddForce(new Vector3(desMoveDir.x, 0, desMoveDir.y) * currentMovementSpeed, ForceMode.VelocityChange);

            RaycastHit hit;
            Physics.Raycast(transform.position, new Vector3(lookDir.x, 0, lookDir.y), out hit, Mathf.Infinity, lm);

            cursor.position = cam.WorldToScreenPoint(hit.point);
        }  

        Message playerPosRot = Message.Create(MessageSendMode.Unreliable, NetworkManager.MessageIds.playerPos);
        playerPosRot.AddUShort(id);
        playerPosRot.AddVector3(transform.position);
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
                    rng = Random.Range(5, 10);
                } else
                {   
                    rng = Random.Range(0, 5);
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
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }
}
