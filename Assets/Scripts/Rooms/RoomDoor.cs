using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private DoorDirection direction;

    [Header("Connection")]
    [SerializeField] private bool isConnected = false;

    public DoorDirection Direction => direction;
    public bool IsConnected => isConnected;

    public bool IsAvailable =>
        gameObject.activeSelf && !isConnected;


    public void Connect()
    {
        isConnected = true;
    }


    public void Disconnect()
    {
        isConnected = false;
    }


    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}