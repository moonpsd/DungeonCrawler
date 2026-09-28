using System.Collections.Generic;
using UnityEngine;

public class RoomPrefabData : MonoBehaviour
{
    [Header("Room Types")]
    [SerializeField]
    private List<RoomType> allowedRoomTypes = new List<RoomType>();


    [Header("Available Doors")]
    [SerializeField] private bool hasLeftDoor = true;
    [SerializeField] private bool hasRightDoor = true;
    [SerializeField] private bool hasTopDoor = true;
    [SerializeField] private bool hasBottomDoor = true;


    [Header("Room Size")]
    [SerializeField]
    private Vector2 roomSize = new Vector2(40f, 24f);


    // =========================
    // PROPERTIES
    // =========================

    public bool HasLeftDoor => hasLeftDoor;
    public bool HasRightDoor => hasRightDoor;
    public bool HasTopDoor => hasTopDoor;
    public bool HasBottomDoor => hasBottomDoor;

    public Vector2 RoomSize => roomSize;


    // =========================
    // ROOM TYPE
    // =========================

    public bool SupportsRoomType(RoomType type)
    {
        return allowedRoomTypes.Contains(type);
    }


    // =========================
    // CONNECTIONS
    // =========================

    public bool SupportsConnections(
        bool needsLeft,
        bool needsRight,
        bool needsTop,
        bool needsBottom)
    {
        if (needsLeft && !hasLeftDoor)
        {
            return false;
        }

        if (needsRight && !hasRightDoor)
        {
            return false;
        }

        if (needsTop && !hasTopDoor)
        {
            return false;
        }

        if (needsBottom && !hasBottomDoor)
        {
            return false;
        }

        return true;
    }
}