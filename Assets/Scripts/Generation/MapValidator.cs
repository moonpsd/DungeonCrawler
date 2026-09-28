using System.Collections.Generic;
using UnityEngine;

public static class MapValidator
{
    // =========================
    // MAIN VALIDATION
    // =========================

    public static bool Validate(
        MapGraph graph,
        int expectedMainPathLength,
        int maxConnectionsPerNode)
    {
        if (graph == null)
        {
            Debug.LogWarning(
                "MapValidator: Graph is null."
            );

            return false;
        }

        if (graph.NodeCount == 0)
        {
            Debug.LogWarning(
                "MapValidator: Graph is empty."
            );

            return false;
        }


        bool valid = true;


        if (!ValidateStart(graph))
        {
            valid = false;
        }


        if (!ValidateUniquePositions(graph))
        {
            valid = false;
        }


        if (!ValidateConnections(graph))
        {
            valid = false;
        }


        if (!ValidateReachability(graph))
        {
            valid = false;
        }


        if (!ValidateMainPath(
                graph,
                expectedMainPathLength))
        {
            valid = false;
        }


        if (!ValidateConnectionLimit(
                graph,
                maxConnectionsPerNode))
        {
            valid = false;
        }


        if (valid)
        {
            Debug.Log(
                "MapValidator: Map is VALID."
            );
        }
        else
        {
            Debug.LogWarning(
                "MapValidator: Map is INVALID."
            );
        }


        return valid;
    }


    // =========================
    // START
    // =========================

    private static bool ValidateStart(
        MapGraph graph)
    {
        int startCount = 0;


        foreach (MapNode node in graph.Nodes)
        {
            if (node.RoomType ==
                RoomType.Start)
            {
                startCount++;
            }
        }


        if (startCount != 1)
        {
            Debug.LogWarning(
                $"MapValidator: Expected exactly 1 Start, found {startCount}."
            );

            return false;
        }


        return true;
    }


    // =========================
    // POSITIONS
    // =========================

    private static bool ValidateUniquePositions(
        MapGraph graph)
    {
        HashSet<Vector2Int> positions =
            new HashSet<Vector2Int>();


        foreach (MapNode node in graph.Nodes)
        {
            if (!positions.Add(
                    node.GridPosition))
            {
                Debug.LogWarning(
                    $"MapValidator: Duplicate position found at {node.GridPosition}."
                );

                return false;
            }
        }


        return true;
    }


    // =========================
    // CONNECTIONS
    // =========================

    private static bool ValidateConnections(
        MapGraph graph)
    {
        foreach (MapNode node in graph.Nodes)
        {
            foreach (
                int connectedID
                in node.Connections)
            {
                MapNode connectedNode =
                    graph.GetNode(
                        connectedID
                    );


                if (connectedNode == null)
                {
                    Debug.LogWarning(
                        $"MapValidator: Node {node.ID} references missing node {connectedID}."
                    );

                    return false;
                }


                if (!connectedNode
                    .IsConnectedTo(node.ID))
                {
                    Debug.LogWarning(
                        $"MapValidator: Connection {node.ID} -> {connectedID} is not bidirectional."
                    );

                    return false;
                }


                // Não permite conexão
                // do node com ele mesmo.
                if (connectedID == node.ID)
                {
                    Debug.LogWarning(
                        $"MapValidator: Node {node.ID} is connected to itself."
                    );

                    return false;
                }
            }
        }


        return true;
    }


    // =========================
    // REACHABILITY
    // =========================

    private static bool ValidateReachability(
        MapGraph graph)
    {
        MapNode startNode = null;


        foreach (MapNode node in graph.Nodes)
        {
            if (node.RoomType ==
                RoomType.Start)
            {
                startNode = node;

                break;
            }
        }


        if (startNode == null)
        {
            return false;
        }


        HashSet<int> visited =
            new HashSet<int>();

        Queue<MapNode> queue =
            new Queue<MapNode>();


        visited.Add(startNode.ID);

        queue.Enqueue(startNode);


        while (queue.Count > 0)
        {
            MapNode current =
                queue.Dequeue();


            foreach (
                int connectionID
                in current.Connections)
            {
                if (visited.Contains(
                        connectionID))
                {
                    continue;
                }


                MapNode connectedNode =
                    graph.GetNode(
                        connectionID
                    );


                if (connectedNode == null)
                {
                    continue;
                }


                visited.Add(
                    connectedNode.ID
                );

                queue.Enqueue(
                    connectedNode
                );
            }
        }


        if (visited.Count !=
            graph.NodeCount)
        {
            Debug.LogWarning(
                $"MapValidator: Only {visited.Count}/{graph.NodeCount} nodes are reachable."
            );

            return false;
        }


        return true;
    }


    // =========================
    // MAIN PATH
    // =========================

    private static bool ValidateMainPath(
        MapGraph graph,
        int expectedLength)
    {
        int mainPathCount = 0;


        foreach (MapNode node in graph.Nodes)
        {
            if (
                node.PathType ==
                NodePathType.MainPath)
            {
                mainPathCount++;
            }
        }


        if (mainPathCount !=
            expectedLength)
        {
            Debug.LogWarning(
                $"MapValidator: Main Path should contain {expectedLength} nodes, but contains {mainPathCount}."
            );

            return false;
        }


        return true;
    }


    // =========================
    // CONNECTION LIMIT
    // =========================

    private static bool ValidateConnectionLimit(
        MapGraph graph,
        int maxConnections)
    {
        foreach (MapNode node in graph.Nodes)
        {
            if (
                node.Connections.Count >
                maxConnections)
            {
                Debug.LogWarning(
                    $"MapValidator: Node {node.ID} has {node.Connections.Count} connections. Maximum allowed is {maxConnections}."
                );

                return false;
            }
        }


        return true;
    }
}