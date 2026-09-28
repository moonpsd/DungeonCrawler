using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private int numberOfRooms = 5;

    private Room previousRoom;

    private void Start()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            CreateRoom();
        }
    }

    private void CreateRoom()
    {
        GameObject prefab =
            roomPrefabs[Random.Range(0, roomPrefabs.Length)];

        GameObject newRoomObject =
            Instantiate(prefab, Vector3.zero, Quaternion.identity);

        Room newRoom = newRoomObject.GetComponent<Room>();

        if (newRoom == null)
        {
            Debug.LogError(newRoomObject.name + " NÃO TEM Room.cs!");
            return;
        }

        if (newRoom.leftConnection == null)
        {
            Debug.LogError(newRoomObject.name + " NÃO TEM LeftConnection configurado!");
            return;
        }

        if (newRoom.rightConnection == null)
        {
            Debug.LogError(newRoomObject.name + " NÃO TEM RightConnection configurado!");
            return;
        }

        if (previousRoom == null)
        {
            newRoom.transform.position = Vector3.zero;

            Debug.Log("PRIMEIRA SALA: " + newRoom.name);
        }
        else
        {
            ConnectRooms(previousRoom, newRoom);
        }

        previousRoom = newRoom;
    }

    private void ConnectRooms(Room previous, Room next)
    {
        Vector3 targetPosition =
            previous.rightConnection.position;

        Vector3 connectionOffset =
            next.leftConnection.position -
            next.transform.position;

        next.transform.position =
            targetPosition - connectionOffset;

        Debug.Log(
            "Conectando " +
            previous.name +
            " -> " +
            next.name +
            " | Nova posição: " +
            next.transform.position
        );
    }
}