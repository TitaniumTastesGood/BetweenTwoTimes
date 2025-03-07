using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Player_Movement player;
    public GameObject connected_portal;
    public bool covered = false;
    public List<int> present_levels = new List<int>();
    public List<Vector2> starting_positions = new List<Vector2>();

    public void set_position_to_beginning(int level)
    {
        if (present_levels.Contains(level))
        {
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
