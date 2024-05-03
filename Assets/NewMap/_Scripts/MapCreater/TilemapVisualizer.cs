using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RoomDesign
{
    None,
    Square,
    Random
}

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap, CaveDoor;
    [SerializeField]
    private TileBase floorTile, wallTop, wallSideRight, wallSiderLeft, wallBottom, wallFull,
        wallInnerCornerDownLeft, wallInnerCornerDownRight, cave,
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    [SerializeField]
    private List<TileBase> squareTile = new List<TileBase>();

    [HideInInspector]
    public Vector2Int RandomRoomStartPositions;

    [HideInInspector]
    public bool IsDoor = false;

    public RoomDesign roomDesign;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions, List<Vector2Int> roomCenters)
    {
        if (roomDesign == RoomDesign.None)
        {
            PaintTiles(floorPositions, floorTilemap, floorTile);
        }
        else if (roomDesign == RoomDesign.Random)
        {
            foreach (var position in floorPositions)
            {
                PaintSingleTile(floorTilemap, floorTile, position);
            }

            TileBase[] squareTiles = { squareTile[0], squareTile[1] };

            foreach (var centerPosition in roomCenters)
            {
                int numTiles = UnityEngine.Random.Range(5, 13);

                for (int i = 0; i < numTiles; i++)
                {
                    Vector2Int offset = new Vector2Int(UnityEngine.Random.Range(-1, 2), UnityEngine.Random.Range(-1, 2));
                    Vector2Int tilePosition = centerPosition + offset;

                    TileBase tile = GetRandomTile(squareTiles);
                    PaintSingleTile(floorTilemap, tile, tilePosition);
                }
            }
        }
        else if (roomDesign == RoomDesign.Square)
        {
            foreach (var position in floorPositions)
            {
                PaintSingleTile(floorTilemap, floorTile, position);
            }

            foreach (var centerPosition in roomCenters)
            {
                int width = 4;
                int height = 4;

                int startX = centerPosition.x - width / 2;
                int startY = centerPosition.y - height / 2;

                for (int x = startX; x < startX + width; x++)
                {
                    for (int y = startY; y < startY + height; y++)
                    {
                        PaintSingleTile(floorTilemap, squareTile[0], new Vector2Int(x, y));
                    }
                }
            }
        }
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    private TileBase GetRandomTile(TileBase[] tileSet)
    {
        int randomIndex = UnityEngine.Random.Range(0, tileSet.Length);
        return tileSet[randomIndex];
    }

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        TileBase tile1 = null;
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }
        else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
        {
            if (IsDoor)
            {
                tile = wallSiderLeft;
            }
            else
            {
                IsDoor = true;
                tile1 = cave;
                RandomRoomStartPositions = position;
            }
        }
        else if (WallTypesHelper.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);

        if (tile1 != null)
            PaintSingleTile(CaveDoor, tile1, position);
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        CaveDoor.ClearAllTiles();
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeASInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeASInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottmEightDirections.Contains(typeASInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }
}
