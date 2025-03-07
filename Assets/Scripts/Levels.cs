using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levels : MonoBehaviour
{
    public GameObject start_warp;
    public GameObject end_warp;
    public int foyer_level;
    public Camera camera_script;
    List<Vector2> start_points = new List<Vector2>();
    List<Vector2> end_points = new List<Vector2>();
    public List<GameObject> fixed_transitions;
    public List<bool> follows_player = new List<bool>();

    void Awake()
    {
        start_points.Add(new Vector2(0, 5)); //attic 0 start coordinates
        end_points.Add(new Vector2(0, -5)); //attic 0 end coordinates
        follows_player.Add(false); //attic 0 follows player
        start_points.Add(new Vector2(0, 4)); //attic 1 start coordinates
        end_points.Add(new Vector2(0, -4)); //attic 1 end coordinates
        follows_player.Add(false); //attic 1 follows player
        start_points.Add(new Vector2(0, 5)); //attic 2 start coordinates
        end_points.Add(new Vector2(0, -5)); //attic 2 end coordinates
        follows_player.Add(false); //attic 2 follows player
        start_points.Add(new Vector2(0, 5)); //attic 3 start coordinates
        end_points.Add(new Vector2(0, -5)); //attic 3 end coordinates
        follows_player.Add(false); //attic 3 follows player
        start_points.Add(new Vector2(-1, 4)); //foyer start coordinates
        end_points.Add(new Vector2(0, -4)); //foyer end coordinates
        follows_player.Add(false); //foyer follows player
        start_points.Add(new Vector2(0, 6)); //ice 0 start coordinates
        end_points.Add(new Vector2(0, -6)); //ice 0 end coordinates
        follows_player.Add(true); //ice 0 follows player
        start_points.Add(new Vector2(-1, 6)); //ice 1 start coordinates
        end_points.Add(new Vector2(-1, -6)); //ice 1 end coordinates
        follows_player.Add(true); //ice 1 follows player
        start_points.Add(new Vector2(2, 5)); //portal 0 start coordinates
        end_points.Add(new Vector2(0, -5)); //portal 0 end coordinates
        follows_player.Add(false); //portal 0 follows player
        start_points.Add(new Vector2(0, 5)); //portal 1 start coordinates
        end_points.Add(new Vector2(0, -5)); //portal 1 end coordinates
        follows_player.Add(false); //portal 1 follows player
        start_points.Add(new Vector2(-3, 5)); //laser 0 start coordinates
        end_points.Add(new Vector2(0, -5)); //laser 0 end coordinates
        follows_player.Add(false); //laser 0 follows player
        start_points.Add(new Vector2(0, 5)); //laser 1 start coordinates
        end_points.Add(new Vector2(0, -5)); //laser 1 end coordinates
        follows_player.Add(false); //laser 1 follows player
    }

    public void load_level(int level)
    {
        if (follows_player[level])
        {
            camera_script.GetComponent<Player_Camera>().follow_player = true;
        }
        else
        {
            camera_script.GetComponent<Player_Camera>().follow_player = false;
            camera_script.transform.position = Vector3.back;
        }
        for (int i = 1; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.active)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            if (i == level + 1)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        start_warp.transform.position = start_points[level];
        end_warp.transform.position = end_points[level];
        start_warp.gameObject.SetActive(true);
        end_warp.gameObject.SetActive(true);
        for (int i = 0; i < fixed_transitions.Count; i++)
        {
            Transition checked_transition = fixed_transitions[i].GetComponent<Transition>();
            if (checked_transition.fixed_warp_from == level)
            {
                checked_transition.gameObject.SetActive(true);
                if (level != foyer_level) {
                    if (checked_transition.type == "Start")
                    {
                        checked_transition.transform.position = start_warp.transform.position;
                        start_warp.gameObject.SetActive(false);
                    }
                    else if (checked_transition.type == "End")
                    {
                        checked_transition.transform.position = end_warp.transform.position;
                        end_warp.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                checked_transition.gameObject.SetActive(false);
            }
        }
    }
}
