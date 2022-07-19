using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    public WallCollision[] walls; // 0 - Up 1 -Down 2 - Right 3- Left
    public GameObject[] doors;

    public bool[] currStatus;
    public bool doneBreakingHoles;

    public void UpdateRoom(bool[] status)
    {
        currStatus = status;
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].gameObject.SetActive(!status[i]);
        }

        if (!doneBreakingHoles)
        {
            foreach (var wall in walls)
            {
                wall.BreakHoles();
            }
            doneBreakingHoles = true;
        }
            
    }
}
