using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Box : MonoBehaviour
{
    public GameObject future_version;
    public bool is_past;
    public bool original_is_past;
    bool is_pushed;
    public Laser blocking_laser = null;
    public Laser reflecting_laser = null;
    public Vector3 start_location;
    public Vector3 push_location;
    float push_speed;
    public bool updated_this_frame = false;
    bool started_sliding_this_frame = false;
    float sliding_multiplier = 10;
    float push_timer;
    public LayerMask valid_collisions;
    public bool on_button = false;
    public Button held_button;
    Box other_pushed_box;
    public Portal covered_portal;
    public List<int> present_levels = new List<int>();
    public List<Vector2> start_positions = new List<Vector2>();
    public List<bool> reflective_levels = new List<bool>();
    bool teleporting = false;
    public List<GameObject> button_objects = new List<GameObject>();
    Vector3 sliding = Vector3.zero;
    public List<Box> previous_pushed_boxes;
    public List<Vector3> previous_pushed_box_positions;
    public Player_Movement player;
    public AudioClip push_sound;
    public bool reflective = false;
    public GameObject laser_object_past;
    public GameObject laser_object_future;

    void Start()
    {
        
    }

    void Update()
    {
        if (is_pushed)
        {
            transform.position = new Vector3(Mathf.Lerp(start_location.x, push_location.x, push_timer), Mathf.Lerp(start_location.y, push_location.y, push_timer), 0);
            if (sliding == Vector3.zero)
            {
                push_timer += Time.deltaTime * push_speed;
            }
            else
            {
                push_timer += Time.deltaTime * push_speed * sliding_multiplier;
            }
            if (push_timer > 1)
            {
                is_pushed = false;
                transform.position = push_location;
                if (teleporting)
                {
                    teleporting = false;
                    if (!covered_portal.covered)
                    {
                        covered_portal.covered = true;
                        transform.position = covered_portal.transform.position;
                        if (is_past)
                        {
                            valid_collisions = LayerMask.GetMask("Future");
                            gameObject.layer = LayerMask.NameToLayer("Future");
                        }
                        else
                        {
                            valid_collisions = LayerMask.GetMask("Past");
                            gameObject.layer = LayerMask.NameToLayer("Past");
                        }
                        is_past = !is_past;
                    }
                    else
                    {
                        covered_portal = covered_portal.connected_portal.GetComponent<Portal>();
                        covered_portal.covered = true;
                    }
                }
                if (transform.childCount > 0)
                {
                    foreach (Transform child in this.transform)
                    {
                        if (child.gameObject.name == "Laser(Clone)")
                        {
                            child.GetComponent<Laser>().regenerate();
                        }
                    }
                }
                if (sliding != Vector3.zero)
                {
                    if (can_be_pushed(sliding))
                    {
                        push(sliding, push_speed);
                    }
                }
            }
        }
        if (reflecting_laser == null && transform.childCount > 0)
        {
            kill_all_children();
        }
    }

    public bool can_be_pushed(Vector3 direction, bool forceful = false)
    {
        if (is_pushed)
        {
            return false;
        }
        Physics2D.queriesHitTriggers = true;
        if (sliding == Vector3.zero)
        {
            RaycastHit2D trigger_hit = Physics2D.Raycast(transform.position, direction, 1f, valid_collisions);
            if (trigger_hit.collider != null)
            {
                if (trigger_hit.transform.CompareTag("Ice"))
                {
                    sliding = direction;
                    started_sliding_this_frame = true;
                    GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
        else
        {
            RaycastHit2D trigger_hit = Physics2D.Raycast(transform.position - direction, direction, 1f, valid_collisions);
            if (trigger_hit.collider == null || trigger_hit.transform.CompareTag("Box") || trigger_hit.transform.CompareTag("Button"))
            {
                sliding = Vector3.zero;
                GetComponent<BoxCollider2D>().enabled = true;
                return false;
            }
        }
        Physics2D.queriesHitTriggers = false;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1.4f, valid_collisions);
        if (hit.collider != null)
        {
            if (hit.transform.CompareTag("Wall") || hit.transform.CompareTag("Transition"))
            {
                if (sliding == Vector3.zero)
                {
                    return false;
                }
                else
                {
                    direction *= -1;
                    sliding = direction;
                }
            }
            if (hit.transform.CompareTag("Player"))
            {
                if (sliding == Vector3.zero && !forceful)
                {
                    return false;
                }
                else
                {
                    if (direction == Vector3.up)
                    {
                        hit.transform.GetComponent<Player_Movement>().knockback("up");
                    }
                    else if (direction == Vector3.left)
                    {
                        hit.transform.GetComponent<Player_Movement>().knockback("left");
                    }
                    else if(direction == Vector3.down)
                    {
                        hit.transform.GetComponent<Player_Movement>().knockback("down");
                    }
                    else if(direction == Vector3.right)
                    {
                        hit.transform.GetComponent<Player_Movement>().knockback("right");
                    }
                }
            }
            if (hit.transform.CompareTag("Box"))
            {
                other_pushed_box = hit.transform.gameObject.GetComponent<Box>();
                Portal others_portal = null;
                if (other_pushed_box.covered_portal != null)
                {
                    others_portal = other_pushed_box.covered_portal;
                }
                if (other_pushed_box.can_be_pushed(direction))
                {
                    if (others_portal)
                    {
                        covered_portal = others_portal.connected_portal.GetComponent<Portal>();
                        teleporting = true;
                    }
                }
                else
                {
                    other_pushed_box = null;
                    if (sliding == Vector3.zero)
                    {
                        return false;
                    }
                    else
                    {
                        direction *= -1;
                        sliding = direction;
                    }
                }
            }
            if (hit.transform.CompareTag("Button"))
            {
                held_button = hit.transform.gameObject.GetComponent<Button>();
                if (!on_button)
                {
                    held_button.hold();
                }
                on_button = true;
                updated_this_frame = true;
            }
            if (hit.transform.CompareTag("Portal"))
            {
                covered_portal = hit.transform.GetComponent<Portal>().connected_portal.GetComponent<Portal>();
                teleporting = true;
                updated_this_frame = true;
            }
            if (hit.transform.CompareTag("Laser"))
            {
                blocking_laser = hit.transform.gameObject.GetComponent<Laser>();
                blocking_laser.block();
                updated_this_frame = true;
                if (reflective)
                {
                    future_version.GetComponent<Box>().emit_laser(hit.transform.gameObject.GetComponent<Laser>().direction);
                    future_version.GetComponent<Box>().reflecting_laser = hit.transform.gameObject.GetComponent<Laser>();
                }
            }
        }
        return true;
    }

    public void push(Vector3 direction, float speed)
    {
        if (!updated_this_frame)
        {
            if (on_button)
            {
                held_button.release();
                on_button = false;
            }
            if (covered_portal != null)
            {
                covered_portal.covered = false;
                covered_portal = null;
            }
            if (blocking_laser != null)
            {
                blocking_laser.unblock();
                blocking_laser = null;
                if (reflective)
                {
                    future_version.GetComponent<Box>().kill_all_children();
                }
            }
        }
        updated_this_frame = false;
        AudioSource.PlayClipAtPoint(push_sound, transform.position);
        is_pushed = true;
        start_location = transform.position;
        push_timer = 0;
        push_location = direction + transform.position;
        push_speed = speed;
        previous_pushed_boxes.Add(null);
        if (future_version != null)
        {
            if (this.is_past && !future_version.GetComponent<Box>().is_past && (sliding == Vector3.zero || started_sliding_this_frame == true))
            {
                Box future_box = future_version.GetComponent<Box>();
                if (future_box.can_be_pushed(direction))
                {
                    future_box.push(direction, speed);
                }
            }
        }
        if (other_pushed_box != null)
        {
            if (other_pushed_box.can_be_pushed(direction))
            {
                previous_pushed_boxes[previous_pushed_boxes.Count - 1] = other_pushed_box;
                previous_pushed_box_positions.Add(other_pushed_box.transform.position);
                other_pushed_box.push(direction, speed);
                other_pushed_box = null;
            }
        }
        started_sliding_this_frame = false;
    }

    public void emit_laser(Vector2 direction)
    {
        GameObject laser_object = null;
        if (is_past)
        {
            laser_object = laser_object_past;
        }
        else
        {
            laser_object = laser_object_future;
        }
        Laser laser_code = laser_object.GetComponent<Laser>();
        laser_code.direction = direction;
        Instantiate(laser_object, this.transform);
    }

    void kill_all_children()
    {
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void undo_pushed_boxes()
    {
        if (previous_pushed_boxes.Count > 0)
        {
            if (previous_pushed_boxes[previous_pushed_boxes.Count - 1] != null)
            {
                previous_pushed_boxes[previous_pushed_boxes.Count - 1].transform.position = previous_pushed_box_positions[previous_pushed_box_positions.Count - 1];
                previous_pushed_boxes[previous_pushed_boxes.Count - 1].undo_pushed_boxes();
                previous_pushed_boxes.RemoveAt(previous_pushed_boxes.Count - 1);
                previous_pushed_box_positions.RemoveAt(previous_pushed_box_positions.Count - 1);
            }
        }
    }

    public void set_position_to_beginning(int level)
    {
        kill_all_children();
        previous_pushed_boxes.Clear();
        previous_pushed_box_positions.Clear();
        if (present_levels.Contains(level))
        {
            gameObject.SetActive(true);
            int level_index = present_levels.IndexOf(level);
            transform.position = new Vector3(start_positions[level_index].x, start_positions[level_index].y, 0);
            reflective = reflective_levels[level_index];
        }
        else
        {
            gameObject.SetActive(false);
        }
        sliding = Vector3.zero;
        if (on_button)
        {
            held_button.release();
            on_button = false;
        }
        if (original_is_past)
        {
            valid_collisions = LayerMask.GetMask("Past");
            gameObject.layer = LayerMask.NameToLayer("Past");
            is_past = true;
        }
        else
        {
            valid_collisions = LayerMask.GetMask("Future");
            gameObject.layer = LayerMask.NameToLayer("Future");
            is_past = false;
        }
        for (int i = 0; i < button_objects.Count; i++)
        {
            if (transform.position == button_objects[i].transform.position)
            {
                held_button = button_objects[i].GetComponent<Button>();
                held_button.hold();
                on_button = true;
            }
        }
    }
}
