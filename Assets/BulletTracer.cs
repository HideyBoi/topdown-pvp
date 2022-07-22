using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private float speed;

    public void SetStartEnd(Vector3 end)
    {
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
        line.SetPosition(2, transform.InverseTransformPoint(end));
    }

    private void Update()
    {
        float mag = (line.GetPosition(2) - line.GetPosition(0)).sqrMagnitude;
        float currMag = (line.GetPosition(1) - line.GetPosition(0)).sqrMagnitude;
        Vector3 dir = (line.GetPosition(2) - line.GetPosition(0)).normalized;
        line.SetPosition(1, line.GetPosition(1) + dir * mag * Time.deltaTime);
        if (mag < currMag)
        {
            line.SetPosition(1, Vector3.zero);
        }
    }
}
