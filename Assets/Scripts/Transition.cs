using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : MonoBehaviour
{
    public string type = "";
    public int fixed_warp_from;
    public int fixed_warp_to;
    public Vector2 foyer_spawn;
    public bool is_warp_fixed = false;
    public List<Vector2> locations;
}
