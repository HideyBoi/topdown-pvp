using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillPopup : MonoBehaviour
{

    public TextMeshProUGUI text;

    public void Remove()
    {
        Destroy(gameObject);
    }

    public void UpdateText(string tex)
    {
        text.text = tex;
    }
}
