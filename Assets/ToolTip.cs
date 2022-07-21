using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    public Animator tooltip;

    private void Awake()
    {
        tooltip.transform.rotation = Quaternion.identity;
    }

    public void IsAimedAt(bool yes)
    {
        tooltip.SetBool("AimedAt", yes);
    }
}
