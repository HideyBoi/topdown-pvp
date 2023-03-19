using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticsManager : MonoBehaviour
{
    public static CosmeticsManager i;

    public Hat[] hats;
    public Material[] skins;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        } else
        {
            Destroy(this);
        }
    }

    [System.Serializable]
    public class Hat
    {
        public string name;
        public Mesh mesh;
        public Material[] materials;
    }
}
