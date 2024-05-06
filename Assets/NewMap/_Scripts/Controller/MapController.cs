using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController Instance;

    public GameObject cavePoint;
    private Vector3 playercreatePos;
    public TilemapVisualizer tilemapVisualizer;

    public GameObject ColliderData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ColliderData = GameObject.Find("Cave1");
        CreateCavePoint();
        CreateRandomItem();
        CreateRandomEnemy();
    }

    /// <summary>
    /// Create Cave Point
    /// </summary>

    public void CreateCavePoint()
    {
        Vector2Int position2D = tilemapVisualizer.RandomRoomStartPositions;
        Vector3 position3D = PlayerConvertVector2IntToVector3(position2D);
        playercreatePos = position3D;

        cavePoint.transform.position = playercreatePos;
        ColliderData.transform.position = new Vector3(playercreatePos.x + 0.5f, playercreatePos.y, playercreatePos.z);
    }

    public Vector3 PlayerConvertVector2IntToVector3(Vector2Int position)
    {
        return new Vector3(position.x - 0.5f, position.y + 0.5f, 0f);
    }


    /// <summary>
    /// Spawn Item
    /// </summary>

    [Space]
    public GameObject PickupPrefab;
    public GameObject PokeballPrefab;
    public Transform item_Transform;
    private Vector3 itemcreatePos;

    private bool IsItem = false;

    public void CreateRandomItem()
    {
        int R = Random.Range(8, 15);

        for (int i = 0; i < R; i++)
        {
            if (RoomFirstDungeonGenerator.Instance.randomWalkRooms)
            {
                int rPos = Random.Range(5, RoomFirstDungeonGenerator.Instance.randomDummyPos.Count);
                Vector2Int position2D = RoomFirstDungeonGenerator.Instance.randomDummyPos[rPos];
                Vector3 position3D = itemConvertVector2IntToVector3(position2D);
                itemcreatePos = position3D;
            }
            else
            {
                int rPos = Random.Range(5, RoomFirstDungeonGenerator.Instance.simpleDummyPos.Count);
                Vector2Int position2D = RoomFirstDungeonGenerator.Instance.simpleDummyPos[rPos];
                Vector3 position3D = itemConvertVector2IntToVector3(position2D);
                itemcreatePos = position3D;
            }

            GameObject G = Instantiate(PickupPrefab, item_Transform);
            G.transform.position = itemcreatePos;

            //if (IsItem)
            //{
            //    IsItem = false;
            //    GameObject G = Instantiate(PickupPrefab, item_Transform);
            //    G.transform.position = itemcreatePos;
            //}
            //else
            //{
            //    IsItem = true;
            //    GameObject G = Instantiate(PokeballPrefab, item_Transform);
            //    G.transform.position = itemcreatePos;
            //}
        }
    }

    public Vector3 itemConvertVector2IntToVector3(Vector2Int position)
    {
        return new Vector3(position.x + 0.5f, position.y + 0.5f, 0f);
    }

    /// <summary>
    /// Spawn Enemy
    /// </summary>

    [Space]
    public GameObject NPC_Prefab;
    public GameObject trainer_Prefab;
    public Transform enemy_Transform;
    private Dictionary<Vector2Int, HashSet<Vector2Int>> roomEnemiesMap = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
    private bool IsTrainer = true;

    public void CreateRandomEnemy()
    {
        foreach (Vector2Int roomCenter in RoomFirstDungeonGenerator.Instance.roomlist)
        {
            if (!roomEnemiesMap.ContainsKey(roomCenter) || roomEnemiesMap[roomCenter].Count == 0)
            {
                roomEnemiesMap[roomCenter] = new HashSet<Vector2Int>();
            }

            IsTrainer = true;
            CreateEnemiesInRoom(roomCenter);
        }
    }

    void CreateEnemiesInRoom(Vector2Int roomCenter)
    {
        int numEnemies = Random.Range(2, 4);
        Vector2Int[] enemyPositions = GetUniqueRoomPositions(roomCenter, numEnemies);

        Shuffle(enemyPositions);

        for (int i = 0; i < numEnemies; i++)
        {
            Vector2Int position = GetValidSpawnPosition(enemyPositions[i]);
            Vector2 spawnPosition = position;
            if (position != Vector2Int.zero)
            {
                if (IsTrainer)
                {
                    IsTrainer = false;
                    GameObject G = Instantiate(trainer_Prefab, spawnPosition, Quaternion.identity);
                    G.transform.SetParent(enemy_Transform);
                }
                else
                {
                    GameObject G = Instantiate(NPC_Prefab, spawnPosition, Quaternion.identity);
                    G.transform.SetParent(enemy_Transform);
                }
                
                roomEnemiesMap[roomCenter].Add(position);
            }
        }
    }
    Vector2Int[] GetUniqueRoomPositions(Vector2Int roomCenter, int count)
    {
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
        BoundsInt roomBounds = new BoundsInt((Vector3Int)(roomCenter - Vector2Int.one * 10), Vector3Int.one * 20);
        while (positions.Count < count)
        {
            Vector2Int randomPosition = new Vector2Int(Random.Range(roomBounds.min.x + 7, roomBounds.max.x - 4), Random.Range(roomBounds.min.y + 7, roomBounds.max.y - 6));

            if (!positions.Contains(randomPosition))
            {
                positions.Add(randomPosition);
            }
        }
        return positions.ToArray();
    }

    Vector2Int GetValidSpawnPosition(Vector2Int position)
    {
        if (RoomFirstDungeonGenerator.Instance.randomWalkRooms)
        {
            foreach (Vector2Int pos in RoomFirstDungeonGenerator.Instance.randomDummyPos)
            {
                if (Vector2Int.Distance(position, pos) < 0.5f)
                {
                    return pos;
                }
            }
        }
        else
        {
            foreach (Vector2Int pos in RoomFirstDungeonGenerator.Instance.simpleDummyPos)
            {
                if (Vector2Int.Distance(position, pos) < 0.5f)
                {
                    return pos;
                }
            }
        }

        return Vector2Int.zero;
    }

    void Shuffle<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}
