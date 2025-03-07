using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser_Emitter : MonoBehaviour
{
    public Vector2 direction = Vector2.left;
    public GameObject laser_obj;
    public Laser laser_code;
    public List<int> present_levels = new List<int>();
    public List<Vector2> starting_positions = new List<Vector2>();
    public List<Vector2> starting_direction = new List<Vector2>();


    public void emit_laser()
    {
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
        laser_code = laser_obj.GetComponent<Laser>();
        laser_code.direction = this.direction;
        laser_obj.layer = this.gameObject.layer;
        Instantiate(laser_obj, this.transform);
    }

    public void set_position_to_beginning(int level)
    {
        if (present_levels.Contains(level))
        {
            gameObject.SetActive(true);
            int level_index = present_levels.IndexOf(level);
            transform.position = new Vector3(starting_positions[level_index].x, starting_positions[level_index].y, 0);
            direction = starting_direction[level_index];
            emit_laser();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
