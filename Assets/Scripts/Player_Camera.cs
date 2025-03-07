using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Camera : MonoBehaviour
{
    public bool follow_player = false;
    public Transform player_location;

    void Update()
    {
        if (follow_player)
        {
            transform.position = player_location.position + Vector3.back;
        }
    }
}
