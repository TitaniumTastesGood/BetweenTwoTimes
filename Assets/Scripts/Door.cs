using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public List<int> present_levels = new List<int>();
    public List<Vector2> start_positions = new List<Vector2>();
    public List<Vector2> level_direction = new List<Vector2>();
    public SpriteRenderer door_sprite;
    public Collider2D collision;
    public List<Sprite> opening_sprites = new List<Sprite>();
    bool forced_open = false;
    public GameObject player;
    public List<Box> boxes = new List<Box>();
    Vector2 direction = Vector2.zero;

    public void set_position_to_beginning(int level)
    {
        door_sprite.GetComponent<SpriteRenderer>();
        if (present_levels.Contains(level))
        {
            gameObject.SetActive(true);
            int level_index = present_levels.IndexOf(level);
            transform.position = new Vector3(start_positions[level_index].x, start_positions[level_index].y, 0);
            Vector2 facing_direction = level_direction[present_levels.IndexOf(level)];
            if (facing_direction == Vector2.down)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (facing_direction == Vector2.right)
            {
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else if (facing_direction == Vector2.up)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (facing_direction == Vector2.left)
            {
                transform.rotation = Quaternion.Euler(0, 270, 0);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
        if (player.GetComponent<Player_Movement>().solved_rooms.Contains(level))
        {
            forced_open = true;
            open(true);
        }
        else
        {
            forced_open = false;
            close(true);
        }
    }

    public void open(bool instant = false)
    {
        if (this.gameObject.activeSelf)
        {
            StartCoroutine(play_animation(true, instant));
            collision.isTrigger = true;
        }
    }

    public void close(bool instant = false)
    {
        if (!forced_open && this.gameObject.activeSelf)
        {
            StartCoroutine(play_animation(false, instant));
            collision.isTrigger = false;
            if (player.transform.position == this.transform.position)
            {
                if (direction == Vector2.up)
                {
                    player.GetComponent<Player_Movement>().knockback("down");
                }
                else if (direction == Vector2.left)
                {
                    player.GetComponent<Player_Movement>().knockback("right");
                }
                else if (direction == Vector2.down)
                {
                    player.GetComponent<Player_Movement>().knockback("up");
                }
                else if (direction == Vector2.right)
                {
                    player.GetComponent<Player_Movement>().knockback("left");
                }
            }
            for (int i = 0; boxes.Count > i; i++)
            {
                Box checked_box = boxes[i];
                if (checked_box.transform.position == this.transform.position)
                {
                    if (checked_box.gameObject.layer == this.gameObject.layer)
                    {
                        if (checked_box.can_be_pushed(new Vector3((direction.x * -1), (direction.y * -1), 0), true))
                        {
                            checked_box.push(new Vector3((direction.x * -1), (direction.y * -1), 0), player.GetComponent<Player_Movement>().movement_speed);
                        }
                    }
                }
            }
        }
    }

    IEnumerator play_animation(bool forward, bool instant)
    {
        if (!forced_open)
        {
            if (forward)
            {
                if (!instant)
                {
                    door_sprite.sprite = opening_sprites[1];
                    yield return new WaitForSeconds(0.1f);
                    door_sprite.sprite = opening_sprites[2];
                    yield return new WaitForSeconds(0.1f);
                    door_sprite.sprite = opening_sprites[3];
                    yield return new WaitForSeconds(0.1f);
                    door_sprite.sprite = opening_sprites[4];
                }
                else
                {
                    door_sprite.sprite = opening_sprites[4];
                    yield return null;
                }
            }
            else
            {
                if (!instant)
                {
                    door_sprite.sprite = opening_sprites[3];
                    yield return new WaitForSeconds(0.1f);
                    door_sprite.sprite = opening_sprites[2];
                    yield return new WaitForSeconds(0.1f);
                    door_sprite.sprite = opening_sprites[1];
                    yield return new WaitForSeconds(0.1f);
                    door_sprite.sprite = opening_sprites[0];
                }
                else
                {
                    door_sprite.sprite = opening_sprites[0];
                    yield return null;
                }
            }
        }
    }
}
