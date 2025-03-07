using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    public bool is_past;
    public Player_Movement other_player;
    public int level = 0;
    [SerializeField] KeyCode up_key;
    [SerializeField] KeyCode left_key;
    [SerializeField] KeyCode down_key;
    [SerializeField] KeyCode right_key;
    [SerializeField] KeyCode reset_key;
    [SerializeField] KeyCode undo_key;
    float undo_timer = 0f;
    public Levels level_manager;
    public LayerMask valid_collisions;
    bool is_moving;
    string sliding = "";
    float movement_timer;
    public List<int> solved_rooms = new List<int>();
    Vector3 original_location;
    Vector3 destination_location;
    public float movement_speed = 10;
    float sliding_multiplier = 10;
    Button held_button;
    public bool waiting_at_end = false;
    public Transition waiting_on;
    bool was_waiting_at_end_last_movement = false;
    bool on_button = false;
    public Portal covered_portal = null;
    bool door_shocked = false;
    bool shocked = false;
    public Transform start_position;
    public Transform end_position;
    bool enter_at_start = true;
    public Vector2 spawnpoint_override = Vector2.zero;
    public SpriteRenderer sprite_renderer;
    public List<Sprite> directional_sprites;
    public List<Vector2> previous_positions;
    public List<Box> previous_moved_boxes;
    public List<Vector3> previous_box_positions;
    public List<bool> previous_box_times;
    public AudioClip step_sound;
    public AudioClip level_transition_sound;

    void Start()
    {
        sprite_renderer = GetComponent<SpriteRenderer>();
        set_position_to_beginning();
    }

    void Update()
    {
        if (Input.GetKeyDown(up_key)) {
            move("up");
        }
        if (Input.GetKeyDown(down_key))
        {
            move("down");
        }
        if (Input.GetKeyDown(left_key))
        {
            move("left");
        }
        if (Input.GetKeyDown(right_key))
        {
            move("right");
        }
        if (Input.GetKeyDown(undo_key))
        {
            undo();
        }
        if (Input.GetKeyDown(reset_key))
        {
            if (!is_moving && !other_player.is_moving)
            {
                set_position_to_beginning();
            }
        }
        if (is_moving)
        {
            transform.position = new Vector3(Mathf.Lerp(original_location.x, destination_location.x, movement_timer), Mathf.Lerp(original_location.y, destination_location.y, movement_timer), 0);
            if (sliding == "")
            {
                movement_timer += Time.deltaTime * movement_speed;
            }
            else
            {
                movement_timer += Time.deltaTime * movement_speed * sliding_multiplier;
            }
            if (movement_timer >= 1) {
                is_moving = false;
                transform.position = destination_location;
                if (waiting_at_end && other_player.waiting_at_end && !other_player.is_moving && waiting_on.type == other_player.waiting_on.type)
                {
                    if (waiting_on.type == "Start")
                    {
                        if (level > 0)
                        {
                            if (waiting_on.is_warp_fixed)
                            {
                                level = waiting_on.fixed_warp_to;
                                other_player.level = waiting_on.fixed_warp_to;
                            }
                            else
                            {
                                level--;
                                other_player.level--;
                            }
                            enter_at_start = false;
                            other_player.enter_at_start = false;
                        }
                    }
                    else if (waiting_on.type == "End")
                    {
                        if (!solved_rooms.Contains(level))
                        {
                            solved_rooms.Add(level);
                            other_player.solved_rooms.Add(level);
                        }
                        if (waiting_on.is_warp_fixed)
                        {
                            level = waiting_on.fixed_warp_to;
                            other_player.level = waiting_on.fixed_warp_to;
                            if (waiting_on != null)
                            {
                                spawnpoint_override = waiting_on.foyer_spawn;
                                other_player.spawnpoint_override = waiting_on.foyer_spawn;
                            }
                        }
                        else
                        {
                            level++;
                            other_player.level++;
                        }
                        enter_at_start = true;
                        other_player.enter_at_start = true;
                    }
                    set_position_to_beginning();
                    other_player.set_position_to_beginning();
                    AudioSource.PlayClipAtPoint(level_transition_sound, transform.position);
                }
                if (sliding != "")
                {
                    move(sliding);
                }
                if (shocked || door_shocked)
                {
                    door_shocked = false;
                    shocked = false;
                    knockback();
                }
            }
        }
    }

    void move(string direction, bool turn = true, bool door_slam = false)
    {
        if (!is_moving)
        {
            was_waiting_at_end_last_movement = waiting_at_end;
            waiting_at_end = false;
            Vector3 movement_direction = Vector3.zero;
            Box moved_box = null;
            if (direction == "up")
            {
                movement_direction = Vector3.up;
                if (turn)
                {
                    sprite_renderer.sprite = directional_sprites[0];
                }
            }
            else if (direction == "down")
            {
                movement_direction = Vector3.down;
                if (turn)
                {
                    sprite_renderer.sprite = directional_sprites[2];
                }
            }
            else if (direction == "left")
            {
                movement_direction = Vector3.left;
                if (turn)
                {
                    sprite_renderer.sprite = directional_sprites[1];
                }
            }
            else if (direction == "right")
            {
                movement_direction = Vector3.right;
                if (turn)
                {
                    sprite_renderer.sprite = directional_sprites[3];
                }
            }
            else
            {
                movement_direction = Vector3.zero;
            }
            Physics2D.queriesHitTriggers = true;
            if (sliding == "")
            {
                RaycastHit2D trigger_hit = Physics2D.Raycast(transform.position, movement_direction, 1f, valid_collisions);
                if (trigger_hit.collider != null)
                {
                    if (trigger_hit.transform.CompareTag("Ice"))
                    {
                        sliding = direction;
                        GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
            }
            else
            {
                RaycastHit2D trigger_hit = Physics2D.Raycast(transform.position - movement_direction, movement_direction, 1f, valid_collisions);
                if (trigger_hit.collider == null || trigger_hit.transform.CompareTag("Box") || trigger_hit.transform.CompareTag("Button"))
                {
                    sliding = "";
                    GetComponent<BoxCollider2D>().enabled = true;
                    return;
                }
            }
            Physics2D.queriesHitTriggers = false;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, movement_direction, 1.4f, valid_collisions);
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("Wall"))
                {
                    if (hit.transform.gameObject.name.Contains("Door") && door_slam)
                    {
                        door_shocked = true;
                    }
                    else
                    {
                        if (sliding == "")
                        {
                            movement_direction = Vector3.zero;
                        }
                        else
                        {
                            direction = invert_direction(direction);
                            sliding = direction;
                            movement_direction *= -1;
                        }
                    }
                    waiting_at_end = was_waiting_at_end_last_movement;
                }
                if (hit.transform.CompareTag("Button"))
                {
                    on_button = true;
                    held_button = hit.transform.gameObject.GetComponent<Button>();
                    held_button.hold();
                }
                else
                {
                    if (on_button)
                    {
                        held_button.release();
                        on_button = false;
                    }
                }
                if (hit.transform.CompareTag("Portal"))
                {
                    covered_portal = hit.transform.GetComponent<Portal>();
                    covered_portal.covered = true;
                }
                else
                {
                    if (covered_portal != null)
                    {
                        covered_portal.covered = false;
                        covered_portal = null;
                    }
                }
                if (hit.transform.CompareTag("Box"))
                {
                    var box_script = hit.transform.gameObject.GetComponent<Box>();
                    if (box_script.can_be_pushed(movement_direction))
                    {
                        moved_box = box_script;
                        if (!box_script.updated_this_frame)
                        {
                            if (box_script.on_button)
                            {
                                on_button = true;
                                held_button = box_script.held_button;
                                held_button.hold();
                            }
                            if (box_script.covered_portal != null)
                            {
                                covered_portal = box_script.covered_portal;
                            }
                            if (box_script.blocking_laser != null)
                            {
                                if (box_script.blocking_laser.direction == Vector2.up && direction != "down")
                                {
                                    shocked = true;
                                }
                                else if (box_script.blocking_laser.direction == Vector2.left && direction != "right")
                                {
                                    shocked = true;
                                }
                                else if (box_script.blocking_laser.direction == Vector2.down && direction != "up")
                                {
                                    shocked = true;
                                }
                                else if (box_script.blocking_laser.direction == Vector2.right && direction != "left")
                                {
                                    shocked = true;
                                }
                            }
                        }
                        box_script.push(movement_direction, movement_speed);
                    }
                    else
                    {
                        if (sliding == "")
                        {
                            movement_direction = Vector3.zero;
                        }
                        else
                        {
                            direction = invert_direction(direction);
                            sliding = direction;
                            movement_direction *= -1;
                        }
                    }
                }
                if (hit.transform.CompareTag("Laser"))
                {
                    shocked = true;
                }
                if (hit.transform.CompareTag("Transition"))
                {
                    waiting_at_end = true;
                    waiting_on = hit.transform.gameObject.GetComponent<Transition>();
                }
            }
            else
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
            }
            is_moving = true;
            movement_timer = 0;
            original_location = transform.position;
            destination_location = transform.position + movement_direction;
            if (sliding == "" && movement_direction != Vector3.zero && !shocked)
            {
                update_previous_actions(destination_location, moved_box);
            }
            if (movement_direction != Vector3.zero)
            {
                AudioSource.PlayClipAtPoint(step_sound, transform.position);
            }
        }
    }

    void update_previous_actions(Vector3 location, Box pushed_box)
    {
        previous_positions.Add(location);
        other_player.previous_positions.Add(other_player.transform.position);
        if (pushed_box != null)
        {
            previous_moved_boxes.Add(pushed_box);
            previous_box_positions.Add(pushed_box.start_location);
            previous_box_times.Add(pushed_box.is_past);
            if (is_past && pushed_box.is_past != pushed_box.future_version.GetComponent<Box>().is_past)
            {
                other_player.previous_moved_boxes.Add(pushed_box.future_version.GetComponent<Box>());
                other_player.previous_box_positions.Add(pushed_box.future_version.GetComponent<Box>().start_location);
                other_player.previous_box_times.Add(pushed_box.future_version.GetComponent<Box>().is_past);
            }
            else
            {
                other_player.previous_moved_boxes.Add(null);
            }
        }
        else
        {
            previous_moved_boxes.Add(null);
            other_player.previous_moved_boxes.Add(null);
        }
    }

    void undo()
    {
        if (!is_moving && !other_player.is_moving && previous_positions.Count > 1)
        {
            previous_positions.RemoveAt(previous_positions.Count - 1);
            transform.position = previous_positions[previous_positions.Count - 1];
            if (previous_moved_boxes[previous_moved_boxes.Count - 1] != null)
            {
                previous_moved_boxes[previous_moved_boxes.Count - 1].transform.position = previous_box_positions[previous_box_positions.Count - 1];
                previous_moved_boxes[previous_moved_boxes.Count - 1].undo_pushed_boxes();
                previous_box_positions.RemoveAt(previous_box_positions.Count - 1);
                if (previous_box_times[previous_box_times.Count - 1])
                {
                    previous_moved_boxes[previous_moved_boxes.Count - 1].valid_collisions = LayerMask.GetMask("Past");
                    previous_moved_boxes[previous_moved_boxes.Count - 1].gameObject.layer = LayerMask.NameToLayer("Past");
                }
                else
                {
                    previous_moved_boxes[previous_moved_boxes.Count - 1].valid_collisions = LayerMask.GetMask("Future");
                    previous_moved_boxes[previous_moved_boxes.Count - 1].gameObject.layer = LayerMask.NameToLayer("Future");
                }
                previous_moved_boxes[previous_moved_boxes.Count - 1].is_past = previous_box_times[previous_box_times.Count - 1];
                previous_box_times.RemoveAt(previous_box_times.Count - 1);
            }
            previous_moved_boxes.RemoveAt(previous_moved_boxes.Count - 1);
            Transform laser = this.transform.parent.Find("Puzzle_Elements").Find("Lasers");
            for (int i = 0; i < laser.childCount; i++)
            {
                laser.GetChild(i).GetComponent<Laser_Emitter>().set_position_to_beginning(level);
            }
        }
    }

    public void knockback(string override_direction = "")
    {
        if (override_direction != "")
        {
            move(override_direction, false);
        }
        else if (sprite_renderer.sprite == directional_sprites[0])
        {
            move("down", false);
        }
        else if (sprite_renderer.sprite == directional_sprites[1])
        {
            move("right", false);
        }
        else if (sprite_renderer.sprite == directional_sprites[2])
        {
            move("up", false);
        }
        else if (sprite_renderer.sprite == directional_sprites[3])
        {
            move("left", false);
        }
    }

    string invert_direction(string direction)
    {
        if (direction == "up")
        {
            return "down";
        }
        else if (direction == "down")
        {
            return "up";
        }
        else if (direction == "left")
        {
            return "right";
        }
        else if (direction == "right")
        {
            return "left";
        }
        else
        {
            return "";
        }
    }

    public void set_position_to_beginning()
    {
        level_manager.load_level(level);
        if (spawnpoint_override != Vector2.zero && level == 4)
        {
            transform.position = spawnpoint_override;
        }
        else
        {
            if (enter_at_start)
            {
                transform.position = start_position.position;
            }
            else
            {
                transform.position = end_position.position;
            }
            if (level == 3 || level == 5)
            {
                spawnpoint_override = Vector2.zero;
            }
        }
        previous_positions.Clear();
        previous_positions.Add(transform.position);
        previous_moved_boxes.Clear();
        previous_moved_boxes.Add(null);
        previous_box_positions.Clear();
        previous_box_times.Clear();
        Transform element = this.transform.parent.Find("Puzzle_Elements");
        Transform boxes = element.Find("Boxes");
        Transform buttons = element.Find("Buttons");
        Transform doors = element.Find("Doors");
        Transform ice = element.Find("Ice_Grid");
        Transform portal = element.Find("Portals");
        Transform laser = element.Find("Lasers");
        Transform receptor = element.Find("Laser_Receptors");
        waiting_at_end = false;
        sliding = "";
        for (int i = 0; i < doors.childCount; i++)
        {
            doors.GetChild(i).GetComponent<Door>().set_position_to_beginning(level);
        }
        for (int i = 0; i < buttons.childCount; i++)
        {
            buttons.GetChild(i).GetComponent<Button>().set_position_to_beginning(level);
        }
        for (int i = 0; i < boxes.childCount; i++)
        {
            boxes.GetChild(i).GetComponent<Box>().set_position_to_beginning(level);
        }
        for (int i = 0; i < ice.childCount; i++)
        {
            ice.GetChild(i).GetComponent<Ice>().set_position_to_beginning(level);
        }
        for (int i = 0; i < portal.childCount; i++)
        {
            portal.GetChild(i).GetComponent<Portal>().set_position_to_beginning(level);
        }
        for (int i = 0; i < receptor.childCount; i++)
        {
            receptor.GetChild(i).GetComponent<Laser_Receptor>().set_position_to_beginning(level);
        }
        for (int i = 0; i < laser.childCount; i++)
        {
            laser.GetChild(i).GetComponent<Laser_Emitter>().set_position_to_beginning(level);
        }
    }
}
