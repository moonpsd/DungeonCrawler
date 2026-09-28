using System.Collections.Generic;
using UnityEngine;

public class RoomPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private RoomPrefabDatabase prefabDatabase;


    [Header("Room Placement")]
    [SerializeField]
    private Vector2 roomSpacing =
        new Vector2(55f, 38f);


    [Header("Generation")]
    [SerializeField]
    private bool placeOnStart = false;


    [Header("Debug")]
    [SerializeField]
    private bool logRooms = true;


    // Guarda todos os GameObjects criados.
    private readonly List<GameObject> spawnedRooms =
        new List<GameObject>();


    // Liga o ID de um MapNode à Room física correspondente.
    //
    // Exemplo:
    // 0 -> Room_0_Start
    // 1 -> Room_1_Exploration
    // 2 -> Room_2_Combat
    private readonly Dictionary<int, Room> roomsByNodeID =
        new Dictionary<int, Room>();


    // =====================================================
    // PROPERTIES
    // =====================================================

    public Vector2 RoomSpacing => roomSpacing;

    public int SpawnedRoomCount => spawnedRooms.Count;


    // =====================================================
    // UNITY
    // =====================================================

    private void Start()
    {
        if (placeOnStart)
        {
            PlaceRooms();
        }
    }


    // =====================================================
    // PLACE ROOMS
    // =====================================================

    public void PlaceRooms()
    {
        ClearRooms();


        if (mapGenerator == null)
        {
            Debug.LogError(
                "RoomPlacer: MapGenerator não foi atribuído."
            );

            return;
        }


        if (prefabDatabase == null)
        {
            Debug.LogError(
                "RoomPlacer: RoomPrefabDatabase não foi atribuído."
            );

            return;
        }


        MapGraph graph =
            mapGenerator.Graph;


        if (graph == null)
        {
            Debug.LogError(
                "RoomPlacer: MapGraph ainda não foi gerado."
            );

            return;
        }


        foreach (MapNode node in graph.Nodes)
        {
            PlaceRoom(
                graph,
                node
            );
        }


        Debug.Log(
            $"RoomPlacer: {spawnedRooms.Count} salas instanciadas."
        );


        Debug.Log(
            $"RoomPlacer: {roomsByNodeID.Count} nodes registrados."
        );
    }


    // =====================================================
    // PLACE SINGLE ROOM
    // =====================================================

    private void PlaceRoom(
        MapGraph graph,
        MapNode node)
    {
        GetRequiredDoors(
            graph,
            node,
            out bool needsLeft,
            out bool needsRight,
            out bool needsTop,
            out bool needsBottom
        );


        // =========================
        // FIND PREFAB
        // =========================

        RoomPrefabData prefabData =
            prefabDatabase.GetCompatibleRoom(
                node.RoomType,
                needsLeft,
                needsRight,
                needsTop,
                needsBottom
            );


        if (prefabData == null)
        {
            Debug.LogWarning(
                $"RoomPlacer: Nenhum prefab encontrado para Node {node.ID}."
            );

            return;
        }


        // =========================
        // POSITION
        // =========================

        Vector3 worldPosition =
            GridToWorld(
                node.GridPosition
            );


        // =========================
        // INSTANTIATE
        // =========================

        GameObject roomObject =
            Instantiate(
                prefabData.gameObject,
                worldPosition,
                Quaternion.identity,
                transform
            );


        roomObject.name =
            $"Room_{node.ID}_{node.RoomType}";


        // =========================
        // GET ROOM
        // =========================

        Room room =
            roomObject.GetComponent<Room>();


        if (room == null)
        {
            Debug.LogError(
                $"RoomPlacer: O prefab {prefabData.name} " +
                $"não possui componente Room."
            );

            Destroy(roomObject);

            return;
        }


        // =========================
        // CONFIGURE ROOM
        // =========================

        room.Configure(
            node.RoomType,
            needsLeft,
            needsRight,
            needsTop,
            needsBottom
        );


        // =========================
        // REGISTER ROOM
        // =========================

        spawnedRooms.Add(
            roomObject
        );


        if (roomsByNodeID.ContainsKey(node.ID))
        {
            Debug.LogWarning(
                $"RoomPlacer: Node {node.ID} já estava registrado. " +
                $"A referência será substituída."
            );

            roomsByNodeID[node.ID] =
                room;
        }
        else
        {
            roomsByNodeID.Add(
                node.ID,
                room
            );
        }


        // =========================
        // DEBUG
        // =========================

        if (logRooms)
        {
            Debug.Log(
                $"RoomPlacer: Node {node.ID} | " +
                $"{node.RoomType} | " +
                $"Grid {node.GridPosition} | " +
                $"World {worldPosition} | " +
                $"Doors: " +
                $"L:{needsLeft} " +
                $"R:{needsRight} " +
                $"T:{needsTop} " +
                $"B:{needsBottom}"
            );
        }
    }


    // =====================================================
    // GET ROOM BY NODE ID
    // =====================================================

    public Room GetRoomByNodeID(int nodeID)
    {
        if (roomsByNodeID.TryGetValue(
                nodeID,
                out Room room))
        {
            return room;
        }


        return null;
    }


    // =====================================================
    // TRY GET ROOM
    // =====================================================

    public bool TryGetRoom(
        int nodeID,
        out Room room)
    {
        return roomsByNodeID.TryGetValue(
            nodeID,
            out room
        );
    }


    // =====================================================
    // HAS ROOM
    // =====================================================

    public bool HasRoom(int nodeID)
    {
        return roomsByNodeID.ContainsKey(
            nodeID
        );
    }


    // =====================================================
    // REQUIRED DOORS
    // =====================================================

    private void GetRequiredDoors(
        MapGraph graph,
        MapNode node,
        out bool left,
        out bool right,
        out bool top,
        out bool bottom)
    {
        left = false;
        right = false;
        top = false;
        bottom = false;


        foreach (int connectedID in node.Connections)
        {
            MapNode connectedNode =
                graph.GetNode(
                    connectedID
                );


            if (connectedNode == null)
            {
                continue;
            }


            Vector2Int direction =
                connectedNode.GridPosition -
                node.GridPosition;


            if (direction == Vector2Int.left)
            {
                left = true;
            }
            else if (direction == Vector2Int.right)
            {
                right = true;
            }
            else if (direction == Vector2Int.up)
            {
                top = true;
            }
            else if (direction == Vector2Int.down)
            {
                bottom = true;
            }
            else
            {
                Debug.LogWarning(
                    $"RoomPlacer: Nodes {node.ID} e " +
                    $"{connectedNode.ID} não são vizinhos diretos."
                );
            }
        }
    }


    // =====================================================
    // GRID -> WORLD
    // =====================================================

    private Vector3 GridToWorld(
        Vector2Int gridPosition)
    {
        return transform.position +
               new Vector3(
                   gridPosition.x *
                   roomSpacing.x,

                   gridPosition.y *
                   roomSpacing.y,

                   0f
               );
    }


    // =====================================================
    // CLEAR ROOMS
    // =====================================================

    public void ClearRooms()
    {
        foreach (GameObject room in spawnedRooms)
        {
            if (room != null)
            {
                Destroy(room);
            }
        }


        spawnedRooms.Clear();

        roomsByNodeID.Clear();
    }
}