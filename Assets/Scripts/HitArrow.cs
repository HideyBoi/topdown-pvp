using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HitArrow : MonoBehaviour
{
    Camera main;

    private void Awake()
    {
        main = Camera.main;
    }

    private void Update()
    {
        transform.position = main.WorldToScreenPoint(LocalPlayerController.instance.transform.position);
    }

    public void SetRot(Vector3 targetPos)
    {
        targetPos = main.WorldToScreenPoint(targetPos);
        targetPos.x -= transform.position.x;
        targetPos.y -= transform.position.y;
        var angle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void DonePlaying()
    {
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
