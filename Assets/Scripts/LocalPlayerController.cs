using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class LocalPlayerController : MonoBehaviour
{

    private Controls controls;

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

    ushort id;

    private void Awake()
    {
        nm = NetworkManager.instance;
        id = nm.Client.Id;
        controls = new Controls();
        rb = GetComponent<Rigidbody>();

        controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled += ctx => Move(ctx.ReadValue<Vector2>());
        controls.Player.PointGamepad.performed += ctx => PointGamepad(ctx.ReadValue<Vector2>());
        controls.Player.PointMouse.performed += ctx => PointMouse(ctx.ReadValue<Vector2>());
    }

    private void FixedUpdate()
    {
        if (!nm.gameIsStarted)
            return;
        rb.AddForce(new Vector3(desMoveDir.x, 0, desMoveDir.y) * currentMovementSpeed, ForceMode.VelocityChange);

        RaycastHit hit;
        Physics.Raycast(transform.position, new Vector3(lookDir.x, 0, lookDir.y), out hit, Mathf.Infinity, lm);

        cursor.position = cam.WorldToScreenPoint(hit.point);

        Message playerPosRot = Message.Create(MessageSendMode.unreliable, NetworkManager.MessageIds.playerPos, shouldAutoRelay: true);
        playerPosRot.AddUShort(id);
        playerPosRot.AddVector3(transform.position);
        playerPosRot.AddQuaternion(pivot.rotation);
        nm.Client.Send(playerPosRot);
    }

    private void Update()
    {
        if (!nm.gameIsStarted)
            return;
        miniMapCam.transform.position = new Vector3(transform.position.x, 200, transform.position.z);
        miniMapCam.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
    }

    void Move(Vector2 rawdir)
    {
        desMoveDir = rawdir;
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
        controls.Disable();
    }
}
