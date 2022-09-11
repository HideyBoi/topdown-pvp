using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollision : MonoBehaviour
{
    public RoomBehaviour room;
    public int id;

    public void BreakHoles()
    {
        if (Random.Range(0, 100) < 48)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 3f);

            List<GameObject> walls = new List<GameObject>();

            if (colliders != null)
            {
                foreach (Collider collider in colliders)
                {
                    if (collider.tag == "Wall")
                    {
                        walls.Add(collider.gameObject);
                    }
                }
            }

            if (walls.Count == 2)
            {
                foreach (GameObject wall in walls)
                {
                    wall.gameObject.GetComponent<WallCollision>().UpdateFloorForBrokenWall();
                }
            }
        }
    }

    public void UpdateFloorForBrokenWall()
    {
        room.currStatus[id] = true;
        room.UpdateRoom(room.currStatus);
        transform.parent.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}
