using System.Collections.Generic;
using UnityEngine;

public class CorridorPlacer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private RoomPlacer roomPlacer;


    [Header("Corridor Prefabs")]
    [SerializeField] private Corridor horizontalCorridorPrefab;
    [SerializeField] private Corridor verticalCorridorPrefab;


    [Header("Debug")]
    [SerializeField] private bool logCorridors = true;


    private readonly List<Corridor> spawnedCorridors =
        new List<Corridor>();


    // =====================================================
    // GENERATE
    // =====================================================

    public void PlaceCorridors()
    {
        ClearCorridors();


        if (mapGenerator == null)
        {
            Debug.LogError(
                "CorridorPlacer: MapGenerator não atribuído."
            );

            return;
        }


        if (roomPlacer == null)
        {
            Debug.LogError(
                "CorridorPlacer: RoomPlacer não atribuído."
            );

            return;
        }


        MapGraph graph = mapGenerator.Graph;


        if (graph == null)
        {
            Debug.LogError(
                "CorridorPlacer: MapGraph não existe."
            );

            return;
        }


        HashSet<string> createdConnections =
            new HashSet<string>();


        foreach (MapNode node in graph.Nodes)
        {
            foreach (int connectedID in node.Connections)
            {
                int smallerID =
                    Mathf.Min(node.ID, connectedID);

                int largerID =
                    Mathf.Max(node.ID, connectedID);


                string connectionKey =
                    $"{smallerID}-{largerID}";


                // Impede:
                //
                // Node 1 -> Node 2
                // Node 2 -> Node 1
                //
                // de criarem dois corredores.
                if (!createdConnections.Add(connectionKey))
                {
                    continue;
                }


                MapNode otherNode =
                    graph.GetNode(connectedID);


                if (otherNode == null)
                {
                    continue;
                }


                CreateCorridor(
                    node,
                    otherNode
                );
            }
        }


        Debug.Log(
            $"CorridorPlacer: {spawnedCorridors.Count} corredores criados."
        );
    }


    // =====================================================
    // CREATE CORRIDOR
    // =====================================================

    private void CreateCorridor(
        MapNode nodeA,
        MapNode nodeB)
    {
        if (!roomPlacer.TryGetRoom(
                nodeA.ID,
                out Room roomA))
        {
            Debug.LogWarning(
                $"CorridorPlacer: Room do Node {nodeA.ID} não encontrada."
            );

            return;
        }


        if (!roomPlacer.TryGetRoom(
                nodeB.ID,
                out Room roomB))
        {
            Debug.LogWarning(
                $"CorridorPlacer: Room do Node {nodeB.ID} não encontrada."
            );

            return;
        }


        Vector2Int direction =
            nodeB.GridPosition -
            nodeA.GridPosition;


        DoorDirection doorDirectionA;
        DoorDirection doorDirectionB;

        Corridor corridorPrefab;


        // =========================
        // HORIZONTAL
        // =========================

        if (direction == Vector2Int.right)
        {
            doorDirectionA = DoorDirection.Right;
            doorDirectionB = DoorDirection.Left;

            corridorPrefab =
                horizontalCorridorPrefab;
        }
        else if (direction == Vector2Int.left)
        {
            doorDirectionA = DoorDirection.Left;
            doorDirectionB = DoorDirection.Right;

            corridorPrefab =
                horizontalCorridorPrefab;
        }


        // =========================
        // VERTICAL
        // =========================

        else if (direction == Vector2Int.up)
        {
            doorDirectionA = DoorDirection.Top;
            doorDirectionB = DoorDirection.Bottom;

            corridorPrefab =
                verticalCorridorPrefab;
        }
        else if (direction == Vector2Int.down)
        {
            doorDirectionA = DoorDirection.Bottom;
            doorDirectionB = DoorDirection.Top;

            corridorPrefab =
                verticalCorridorPrefab;
        }
        else
        {
            Debug.LogWarning(
                $"CorridorPlacer: Nodes {nodeA.ID} e {nodeB.ID} " +
                $"não são vizinhos."
            );

            return;
        }


        if (corridorPrefab == null)
        {
            Debug.LogError(
                "CorridorPlacer: Prefab de corredor não atribuído."
            );

            return;
        }


        // =========================
        // GET DOORS
        // =========================

        RoomDoor doorA =
            roomA.GetDoor(
                doorDirectionA
            );


        RoomDoor doorB =
            roomB.GetDoor(
                doorDirectionB
            );


        if (doorA == null || doorB == null)
        {
            Debug.LogWarning(
                $"CorridorPlacer: Não encontrou as portas " +
                $"entre Node {nodeA.ID} e Node {nodeB.ID}."
            );

            return;
        }

        // =========================
        // CONNECT DOORS
        // =========================

        doorA.Connect(doorB);
        doorB.Connect(doorA);

        // =========================
        // SPAWN
        // =========================

        Corridor corridor =
            Instantiate(
                corridorPrefab,
                Vector3.zero,
                Quaternion.identity,
                transform
            );


        corridor.name =
            $"Corridor_{nodeA.ID}_{nodeB.ID}";


        corridor.Build(
         doorA,
         doorB
        );


        spawnedCorridors.Add(
            corridor
        );


        if (logCorridors)
        {
            Debug.Log(
                $"CorridorPlacer: " +
                $"Node {nodeA.ID} -> Node {nodeB.ID} | " +
                $"{doorDirectionA} -> {doorDirectionB}"
            );
        }
    }


    // =====================================================
    // CLEAR
    // =====================================================

    public void ClearCorridors()
    {
        foreach (Corridor corridor in spawnedCorridors)
        {
            if (corridor != null)
            {
                Destroy(corridor.gameObject);
            }
        }


        spawnedCorridors.Clear();
    }
}