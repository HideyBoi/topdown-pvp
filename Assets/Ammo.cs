using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;
using TMPro;

public class Ammo : MonoBehaviour
{
    public int id;

    public bool networkSpawned;

    public LocalInventoryManager.AmmoType type;
    public int count;

    public AudioClip ammoPickUpSound;
    public GameObject sfx;

    public TMP_Text ammoName;
    public TMP_Text ammoCount;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public Mesh lightMesh;
    public Material[] lightMat;
    public Mesh mediumMesh;
    public Material[] mediumMat;
    public Mesh heavyMesh;
    public Material[] heavyMat;
    public Mesh shellsMesh;
    public Material[] shellsMat;

    private void Awake()
    {
        GameManager.instance.AddAmmo(this);
    }

    void Start()
    {
        switch (type)
        {
            case LocalInventoryManager.AmmoType.Light:
                ammoName.text = "Light Ammo";
                meshFilter.mesh = lightMesh;
                meshRenderer.materials = lightMat;
                break;
            case LocalInventoryManager.AmmoType.Medium:
                ammoName.text = "Medium Ammo";
                meshFilter.mesh = mediumMesh;
                meshRenderer.materials = mediumMat;
                break;
            case LocalInventoryManager.AmmoType.Heavy:
                ammoName.text = "Heavy Ammo";
                meshFilter.mesh = heavyMesh;
                meshRenderer.materials = heavyMat;
                break;
            case LocalInventoryManager.AmmoType.Shells:
                ammoName.text = "Shell Ammo";
                meshFilter.mesh = shellsMesh;
                meshRenderer.materials = shellsMat;
                break;
        }

        ammoCount.text = $"{count} bullets";

        if (!networkSpawned)
        {
            id = Random.Range(0, 2147483646);

            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.spawnAmmo, shouldAutoRelay: true);
            msg.AddInt(id);
            msg.AddVector3(transform.position);

            int typeId = 0;

            switch (type)
            {
                case LocalInventoryManager.AmmoType.Light:
                    typeId = 0;
                    break;
                case LocalInventoryManager.AmmoType.Medium:
                    typeId = 1;
                    break;
                case LocalInventoryManager.AmmoType.Heavy:
                    typeId = 2;
                    break;
                case LocalInventoryManager.AmmoType.Shells:
                    typeId = 3;
                    break;
            }

            msg.AddInt(typeId);
            msg.AddInt(count);

            NetworkManager.instance.Client.Send(msg);
        }
    }

    public void Pickup(bool fromNetwork)
    {
        GameObject sfxOBJ = Instantiate(sfx, transform.position, Quaternion.identity);
        SoundEffect effect = sfxOBJ.GetComponent<SoundEffect>();
        effect.PlaySound(ammoPickUpSound, 30, 1);
        Destroy(GetComponent<BoxCollider>());

        if (!fromNetwork)
        {
            Message msg = Message.Create(MessageSendMode.reliable, NetworkManager.MessageIds.pickUpAmmo, shouldAutoRelay: true);
            msg.AddInt(id);

            NetworkManager.instance.Client.Send(msg);
        }

        GetComponent<Animator>().Play("Destroy");
    }

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}
