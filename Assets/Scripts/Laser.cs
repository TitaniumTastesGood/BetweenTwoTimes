using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Vector2 direction;
    public LayerMask valid_collisions;
    bool powering_receptor = false;

    void Start()
    {
        if (!powering_receptor)
        {
            regenerate();
        }
    }

    public void block()
    {
        this.gameObject.SetActive(false);
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void unblock()
    {
        this.gameObject.SetActive(true);
        regenerate();
    }

    void continue_laser()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, valid_collisions);
        if (hit.collider == null)
        {
            Instantiate(this, transform.position + new Vector3(direction.x, direction.y, 0), transform.rotation, this.transform);
        }
        else if (hit.transform.CompareTag("Box"))
        {
            GameObject child_laser = null;
            Instantiate(this, transform.position + new Vector3(direction.x, direction.y, 0), transform.rotation, this.transform);
            foreach (Transform child in this.transform)
            {
                child_laser = (child.gameObject);
            }
            child_laser.SetActive(false);
            hit.transform.gameObject.GetComponent<Box>().blocking_laser = child_laser.GetComponent<Laser>();
            if (hit.transform.gameObject.GetComponent<Box>().reflective) {
                hit.transform.gameObject.GetComponent<Box>().future_version.GetComponent<Box>().emit_laser(direction);
            }
        }
        else if (hit.transform.CompareTag("Player"))
        {
            hit.transform.gameObject.GetComponent<Player_Movement>().knockback();
        }
        else if (hit.transform.CompareTag("Button") || hit.transform.CompareTag("Laser") || hit.transform.CompareTag("Transition"))
        {
            Instantiate(this, transform.position + new Vector3(direction.x, direction.y, 0), transform.rotation, this.transform);
        }
        else if (hit.transform.CompareTag("Receptor"))
        {
            GameObject next_laser = this.gameObject;
            next_laser.GetComponent<Laser>().powering_receptor = true;
            Instantiate(next_laser, transform.position + new Vector3(direction.x, direction.y, 0), transform.rotation, this.transform);
        }
    }

    public void regenerate()
    {
        foreach (Transform child in this.transform)
        {
            print(child.gameObject.name);
            Destroy(child.gameObject);
        }
        continue_laser();
    }
}
