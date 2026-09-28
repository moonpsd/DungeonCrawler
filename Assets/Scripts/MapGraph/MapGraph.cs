using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapGraph
{
    [SerializeField]
    private List<MapNode> nodes = new List<MapNode>();

    public List<MapNode> Nodes => nodes;

    public int NodeCount => nodes.Count;

    public void AddNode(MapNode node)
    {
        if (node == null)
        {
            return;
        }

        nodes.Add(node);
    }

    public void RemoveNode(MapNode node)
    {
        if (node == null)
        {
            return;
        }

        List<int> connectionsCopy =
            new List<int>(node.Connections);

        foreach (int connectedID in connectionsCopy)
        {
            MapNode connectedNode =
                GetNode(connectedID);

            if (connectedNode != null)
            {
                connectedNode.RemoveConnection(node.ID);
            }
        }

        nodes.Remove(node);
    }

    public MapNode GetNode(int id)
    {
        foreach (MapNode node in nodes)
        {
            if (node.ID == id)
            {
                return node;
            }
        }

        return null;
    }

    public MapNode GetNodeAt(Vector2Int gridPosition)
    {
        foreach (MapNode node in nodes)
        {
            if (node.GridPosition == gridPosition)
            {
                return node;
            }
        }

        return null;
    }

    public bool HasNodeAt(Vector2Int gridPosition)
    {
        return GetNodeAt(gridPosition) != null;
    }

    public void ConnectNodes(int nodeAID, int nodeBID)
    {
        MapNode nodeA = GetNode(nodeAID);
        MapNode nodeB = GetNode(nodeBID);

        if (nodeA == null || nodeB == null)
        {
            Debug.LogWarning(
                $"Could not connect nodes {nodeAID} and {nodeBID}."
            );

            return;
        }

        nodeA.AddConnection(nodeBID);
        nodeB.AddConnection(nodeAID);
    }

    public void Clear()
    {
        nodes.Clear();
    }
}