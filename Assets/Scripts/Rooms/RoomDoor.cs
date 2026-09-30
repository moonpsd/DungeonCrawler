using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private DoorDirection direction;


    [Header("Connection")]
    [SerializeField] private bool isConnected = false;

    [SerializeField] private RoomDoor connectedDoor;


    // =====================================================
    // PROPERTIES
    // =====================================================

    public DoorDirection Direction => direction;

    public bool IsConnected => isConnected;

    public RoomDoor ConnectedDoor => connectedDoor;


    public bool IsAvailable =>
        gameObject.activeSelf &&
        !isConnected;


    // =====================================================
    // CONNECT
    // =====================================================

    public void Connect(RoomDoor otherDoor)
    {
        if (otherDoor == null)
        {
            Debug.LogWarning(
                $"RoomDoor ({name}): Tentativa de conectar com uma porta nula."
            );

            return;
        }


        connectedDoor = otherDoor;
        isConnected = true;
    }


    // =====================================================
    // CONNECT WITHOUT REFERENCE
    // =====================================================
    // Mantemos temporariamente para compatibilidade
    // com código antigo.

    public void Connect()
    {
        isConnected = true;
    }


    // =====================================================
    // DISCONNECT
    // =====================================================

    public void Disconnect()
    {
        connectedDoor = null;
        isConnected = false;
    }


    // =====================================================
    // SET ACTIVE
    // =====================================================

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}