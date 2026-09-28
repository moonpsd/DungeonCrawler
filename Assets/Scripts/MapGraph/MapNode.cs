using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapNode
{
    [SerializeField] private int id;
    [SerializeField] private Vector2Int gridPosition;
    [SerializeField] private int depth;
    [SerializeField] private RoomType roomType;
    [SerializeField] private NodePathType pathType;
    [SerializeField] private List<int> connections = new List<int>();

    public int ID => id;
    public Vector2Int GridPosition => gridPosition;
    public int Depth => depth;
    public RoomType RoomType => roomType;
    public NodePathType PathType => pathType;
    public List<int> Connections => connections;

    public MapNode(
        int id,
        Vector2Int gridPosition,
        int depth,
        RoomType roomType,
        NodePathType pathType)
    {
        this.id = id;
        this.gridPosition = gridPosition;
        this.depth = depth;
        this.roomType = roomType;
        this.pathType = pathType;
    }

    public void AddConnection(int nodeID)
    {
        if (!connections.Contains(nodeID))
        {
            connections.Add(nodeID);
        }
    }

    public void RemoveConnection(int nodeID)
    {
        connections.Remove(nodeID);
    }

    public bool IsConnectedTo(int nodeID)
    {
        return connections.Contains(nodeID);
    }

    public void SetRoomType(RoomType newType)
    {
        roomType = newType;
    }
}