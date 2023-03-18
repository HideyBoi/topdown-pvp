using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
    public ushort _id;
    public string _name;
    public Transform pivot;
    public RemoteInventoryManager invManager;
    public HealthManager healthManager;
    public bool beingSpectated;

    public Animator playerAnimator;
    public Vector3 desMoveDir;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    public void UpdatePosition(Vector3 pos, Quaternion pivotRot, Vector3 inputDir)
    {
        transform.position = pos;
        pivot.rotation = pivotRot;
    }

    private void FixedUpdate()
    {
        Vector3 localSpaceMoveDir = pivot.InverseTransformVector(new Vector3(desMoveDir.x, 0, desMoveDir.y));

        playerAnimator.SetFloat("MoveDirX", localSpaceMoveDir.x);
        playerAnimator.SetFloat("MoveDirY", localSpaceMoveDir.z);
        playerAnimator.SetFloat("MoveDirMag", localSpaceMoveDir.sqrMagnitude);
    }
}
