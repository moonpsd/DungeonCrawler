using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    // =========================================================
    // SINGLETON
    // =========================================================

    public static DungeonManager Instance
    {
        get;
        private set;
    }


    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("Dungeon")]
    [SerializeField]
    private RoomGenerator roomGenerator;


    // =========================================================
    // PROGRESSÃO
    // =========================================================

    [Header("Progression")]
    [SerializeField]
    private int currentRoom = 1;


    [SerializeField]
    private int bossEvery = 10;


    public int CurrentRoom
    {
        get
        {
            return currentRoom;
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        // Impede dois DungeonManagers
        if (
            Instance != null
            &&
            Instance != this
        )
        {
            Destroy(gameObject);

            return;
        }


        Instance = this;
    }


    // =========================================================
    // PRÓXIMA SALA
    // =========================================================

    public void NextRoom()
    {
        currentRoom++;


        Debug.Log(
            "Entrando na sala "
            + currentRoom
        );


        // =====================================================
        // BOSS
        // =====================================================

        if (
            currentRoom % bossEvery == 0
        )
        {
            Debug.Log(
                "PRÓXIMA SALA É BOSS!"
            );
        }


        // =====================================================
        // GERAR NOVA SALA
        // =====================================================

        if (roomGenerator != null)
        {
            roomGenerator.RegenerateRoom();
        }
        else
        {
            Debug.LogError(
                "RoomGenerator não foi configurado no DungeonManager!"
            );
        }
    }
}