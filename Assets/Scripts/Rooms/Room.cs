using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private RoomType roomType = RoomType.Exploration;

    [Header("Room Components")]
    [SerializeField] private BoxCollider2D roomBounds;

    [Header("Doors")]
    [SerializeField] private List<RoomDoor> doors = new List<RoomDoor>();

    [Header("Gameplay")]
    [SerializeField] private Transform enemySpawns;
    [SerializeField] private Transform rewardSpawns;
    [SerializeField] private Transform challengeAreas;


    // =====================================================
    // PROPERTIES
    // =====================================================

    public RoomType Type => roomType;

    public BoxCollider2D Bounds => roomBounds;

    public List<RoomDoor> Doors => doors;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        FindRoomComponents();
    }


    // =====================================================
    // FIND COMPONENTS
    // =====================================================

    private void FindRoomComponents()
    {
        doors.Clear();

        RoomDoor[] foundDoors =
            GetComponentsInChildren<RoomDoor>(true);

        doors.AddRange(foundDoors);


        Transform boundsTransform =
            transform.Find("Bounds");

        if (boundsTransform != null)
        {
            roomBounds =
                boundsTransform.GetComponent<BoxCollider2D>();
        }


        Transform gameplay =
            transform.Find("Gameplay");

        if (gameplay != null)
        {
            enemySpawns =
                gameplay.Find("EnemySpawns");

            rewardSpawns =
                gameplay.Find("RewardSpawns");

            challengeAreas =
                gameplay.Find("ChallengeAreas");
        }
    }


    // =====================================================
    // ROOM CONFIGURATION
    // =====================================================

    public void Configure(
        RoomType newRoomType,
        bool openLeft,
        bool openRight,
        bool openTop,
        bool openBottom)
    {
        roomType = newRoomType;

        ConfigureDoors(
            openLeft,
            openRight,
            openTop,
            openBottom
        );
    }


    // =====================================================
    // CONFIGURE DOORS
    // =====================================================

    public void ConfigureDoors(
        bool openLeft,
        bool openRight,
        bool openTop,
        bool openBottom)
    {
        // Garante que temos todas as portas,
        // inclusive as que estavam desativadas.
        FindRoomComponents();


        foreach (RoomDoor door in doors)
        {
            if (door == null)
            {
                continue;
            }


            bool shouldBeOpen = false;


            switch (door.Direction)
            {
                case DoorDirection.Left:
                    shouldBeOpen = openLeft;
                    break;


                case DoorDirection.Right:
                    shouldBeOpen = openRight;
                    break;


                case DoorDirection.Top:
                    shouldBeOpen = openTop;
                    break;


                case DoorDirection.Bottom:
                    shouldBeOpen = openBottom;
                    break;
            }


            door.Disconnect();

            door.SetActive(shouldBeOpen);


            if (shouldBeOpen)
            {
                door.Connect();
            }
        }
    }


    // =====================================================
    // GET DOOR
    // =====================================================

    public RoomDoor GetDoor(
        DoorDirection direction)
    {
        foreach (RoomDoor door in doors)
        {
            if (door.Direction == direction &&
                door.gameObject.activeSelf)
            {
                return door;
            }
        }

        return null;
    }


    // =====================================================
    // HAS DOOR
    // =====================================================

    public bool HasDoor(
        DoorDirection direction)
    {
        return GetDoor(direction) != null;
    }


    // =====================================================
    // AVAILABLE DOORS
    // =====================================================

    public List<RoomDoor> GetAvailableDoors()
    {
        List<RoomDoor> availableDoors =
            new List<RoomDoor>();


        foreach (RoomDoor door in doors)
        {
            if (door.IsAvailable)
            {
                availableDoors.Add(door);
            }
        }


        return availableDoors;
    }


    // =====================================================
    // RESET DOORS
    // =====================================================

    public void ResetDoors()
    {
        foreach (RoomDoor door in doors)
        {
            if (door == null)
            {
                continue;
            }

            door.Disconnect();
        }
    }


    // =====================================================
    // SPAWNS
    // =====================================================

    public Transform GetEnemySpawns()
    {
        return enemySpawns;
    }


    public Transform GetRewardSpawns()
    {
        return rewardSpawns;
    }


    public Transform GetChallengeAreas()
    {
        return challengeAreas;
    }


    // =====================================================
    // EDITOR
    // =====================================================

    private void OnValidate()
    {
        FindRoomComponents();
    }
}