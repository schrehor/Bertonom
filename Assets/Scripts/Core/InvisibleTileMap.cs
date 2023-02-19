using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InvisibleTileMap : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TilemapRenderer>().enabled = false;
    }
}
