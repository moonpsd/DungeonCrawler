using System.Collections.Generic;
using UnityEngine;

public class RoomPrefabDatabase : MonoBehaviour
{
    [Header("Room Prefabs")]
    [SerializeField]
    private List<RoomPrefabData> roomPrefabs =
        new List<RoomPrefabData>();


    // =====================================================
    // GET COMPATIBLE ROOM
    // =====================================================

    public RoomPrefabData GetCompatibleRoom(
        RoomType roomType,
        bool needsLeft,
        bool needsRight,
        bool needsTop,
        bool needsBottom)
    {
        List<RoomPrefabData> compatibleRooms =
            new List<RoomPrefabData>();


        foreach (RoomPrefabData room in roomPrefabs)
        {
            if (room == null)
            {
                continue;
            }


            // Verifica se esse prefab pode ser
            // usado para esse tipo de sala.
            if (!room.SupportsRoomType(roomType))
            {
                continue;
            }


            // Verifica se o prefab possui
            // todas as portas necessárias.
            if (!room.SupportsConnections(
                    needsLeft,
                    needsRight,
                    needsTop,
                    needsBottom))
            {
                continue;
            }


            compatibleRooms.Add(room);
        }


        // Nenhum prefab compatível.
        if (compatibleRooms.Count == 0)
        {
            Debug.LogWarning(
                $"RoomPrefabDatabase: Nenhuma sala compatível encontrada.\n" +
                $"Type: {roomType}\n" +
                $"Left: {needsLeft} | " +
                $"Right: {needsRight} | " +
                $"Top: {needsTop} | " +
                $"Bottom: {needsBottom}"
            );

            return null;
        }


        // Escolhe aleatoriamente entre
        // todos os prefabs compatíveis.
        int randomIndex =
            Random.Range(
                0,
                compatibleRooms.Count
            );


        return compatibleRooms[randomIndex];
    }


    // =====================================================
    // GET ALL COMPATIBLE ROOMS
    // =====================================================

    public List<RoomPrefabData> GetAllCompatibleRooms(
        RoomType roomType,
        bool needsLeft,
        bool needsRight,
        bool needsTop,
        bool needsBottom)
    {
        List<RoomPrefabData> compatibleRooms =
            new List<RoomPrefabData>();


        foreach (RoomPrefabData room in roomPrefabs)
        {
            if (room == null)
            {
                continue;
            }


            if (!room.SupportsRoomType(roomType))
            {
                continue;
            }


            if (!room.SupportsConnections(
                    needsLeft,
                    needsRight,
                    needsTop,
                    needsBottom))
            {
                continue;
            }


            compatibleRooms.Add(room);
        }


        return compatibleRooms;
    }


    // =====================================================
    // DEBUG / INFO
    // =====================================================

    public int GetPrefabCount()
    {
        return roomPrefabs.Count;
    }
}