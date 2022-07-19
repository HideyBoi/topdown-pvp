using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollision : MonoBehaviour
{
    public RoomBehaviour room;
    public int id;

    // Start is called before the first frame update
    public void BreakHoles()
    {
        if (Random.Range(0, 100) < 25)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, .01f);

            List<GameObject> walls = new List<GameObject>();

            foreach (Collider collider in colliders)
            {
                if (collider.tag == "Wall")
                {
                    walls.Add(collider.gameObject);
                }
            }

            if (walls.Count == 2)
            {
                foreach (GameObject wall in walls)
                {
                    wall.transform.parent.gameObject.GetComponent<WallCollision>().UpdateFloorForBrokenWall();
                }            
            }
        }
    }

    public void UpdateFloorForBrokenWall()
    {
        room.currStatus[id] = true;
        room.UpdateRoom(room.currStatus);
        gameObject.SetActive(false);
    }
}
