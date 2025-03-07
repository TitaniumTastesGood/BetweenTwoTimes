using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser_Receptor : MonoBehaviour
{
    public List<int> present_levels = new List<int>();
    public List<Vector2> starting_positions = new List<Vector2>();
    public Door attached_door;
    bool powered = false;
    GameObject powering_laser = null;

    private void Update()
    {
        if (powering_laser != null && powered == false)
        {
            powered = true;
            attached_door.open();
        }
        else if (powering_laser == null && powered == true)
        {
            powered = false;
            attached_door.close();
        }
    }

    public void set_position_to_beginning(int level)
    {
        if (present_levels.Contains(level))
        {
            powered = false;
            gameObject.SetActive(true);
            int level_index = present_levels.IndexOf(level);
            transform.position = new Vector3(starting_positions[level_index].x, starting_positions[level_index].y, 0);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
