using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public List<int> present_levels = new List<int>();
    public GameObject attached_door;
    public int held_down = 0;
    public List<Vector2> start_positions = new List<Vector2>();
    public AudioClip button_sound;

    public void hold()
    {
        held_down++;
        AudioSource.PlayClipAtPoint(button_sound, transform.position);
        check_held_status();
    }

    public void release()
    {
        held_down--;
        check_held_status();
    }

    void check_held_status()
    {
        if (held_down > 0)
        {
            attached_door.GetComponent<Door>().open();
        }
        else
        {
            attached_door.GetComponent<Door>().close();
        }
    }

    public void set_position_to_beginning(int level)
    {
        if (present_levels.Contains(level))
        {
            gameObject.SetActive(true);
            int level_index = present_levels.IndexOf(level);
            transform.position = new Vector3(start_positions[level_index].x, start_positions[level_index].y, 0);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
