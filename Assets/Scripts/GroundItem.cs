using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GroundItem : MonoBehaviour
{
    public GameObject sfx;

    float speed = 120;
    public Transform pivot;

    public Vector3 id;

    public InventoryItem currentItem;
    public int currentItemId;

    public MeshFilter gunFilter;
    public MeshRenderer gunRenderer;
    public MeshRenderer sheildRenderer;

    public Material generic;
    public Material rare;
    public Material legendary;

    private void Awake()
    {
        id = transform.position;
        GameManager.instance.AddItem(this);
    }

    private void FixedUpdate()
    {
        pivot.rotation = Quaternion.Euler(0, pivot.rotation.eulerAngles.y + Time.fixedDeltaTime * speed, 0);
    }

    public void UpdateItem(InventoryItem item)
    {
        currentItem.weapon = item.weapon;
        currentItem.ammoCount = item.ammoCount;

        gunFilter.mesh = item.weapon.gunMesh;
        gunRenderer.material = item.weapon.gunMaterial;

        switch(item.weapon.rarity)
        {
            case Weapon.Rarity.generic:
                sheildRenderer.material = generic;
                break;
            case Weapon.Rarity.rare:
                sheildRenderer.material = rare;
                break;
            case Weapon.Rarity.legendary:
                sheildRenderer.material = legendary;
                break;
        }
    }

    public void Pickup()
    {
        Instantiate(sfx, transform.position, Quaternion.identity).GetComponent<SoundEffect>().PlaySound(currentItem.weapon.pickupSound);
        Destroy(GetComponent<BoxCollider>());
        GetComponent<Animator>().Play("Destroy");
    }

    public void DestroyThis()
    {
        Destroy(gameObject);
    }
}
