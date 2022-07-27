using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    public Animator tooltip;

    public float timeLeftUntilDisappear = 0;

    private void Awake()
    {
        tooltip.transform.rotation = Quaternion.identity;
    }

    private void FixedUpdate()
    {
        timeLeftUntilDisappear -= Time.fixedDeltaTime;
        if (timeLeftUntilDisappear > 0)
        {
            tooltip.SetBool("AimedAt", true);
        } else 
        {
            tooltip.SetBool("AimedAt", false);
        }
    }
    public void IsAimedAt(bool yes)
    {
        if (yes)
            timeLeftUntilDisappear = 0.1f;
    }
}
