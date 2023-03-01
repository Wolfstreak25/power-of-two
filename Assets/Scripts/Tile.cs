using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Block OccupiedBlock;
    

    public Vector2 location => transform.position;
}
