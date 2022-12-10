using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowRot : MonoBehaviour
{
    public float speed;

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + Time.fixedDeltaTime * speed, transform.rotation.eulerAngles.z);    
    }
}
