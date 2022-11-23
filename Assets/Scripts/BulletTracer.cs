using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [SerializeField] private TrailRenderer line;
    [SerializeField] private float speed;
    [SerializeField] private Transform endPoint;
    private bool running = false;

    public void SetData(Vector3 endPos)
    {
        endPoint.position = endPos;
    }

    private void FixedUpdate()
    {
        Vector3 dir = endPoint.position - line.transform.position;

        line.transform.position += dir * speed * Time.fixedDeltaTime;

        if (!running)
            StartCoroutine("TimeOut");
    }

    IEnumerator TimeOut()
    {
        running = true;
        yield return new WaitForSeconds(0.6f);
        Destroy(gameObject);
    }
}
