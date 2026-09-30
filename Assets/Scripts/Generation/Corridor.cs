using UnityEngine;

public class Corridor : MonoBehaviour
{
    public enum CorridorDirection
    {
        Horizontal,
        Vertical
    }


    [Header("Corridor")]
    [SerializeField]
    private CorridorDirection direction;


    [Header("Walls")]
    [SerializeField]
    private Transform firstWall;

    [SerializeField]
    private Transform secondWall;


    [Header("Settings")]

    [Tooltip("Espaço interno disponível para o jogador passar.")]
    [SerializeField]
    private float corridorWidth = 8f;

    [Tooltip("Espessura do chão, teto ou paredes.")]
    [SerializeField]
    private float wallThickness = 1f;


    // =====================================================
    // PROPERTIES
    // =====================================================

    public CorridorDirection Direction => direction;


    // =====================================================
    // BUILD
    // =====================================================

    public void Build(
        RoomDoor doorA,
        RoomDoor doorB)
    {
        if (doorA == null || doorB == null)
        {
            Debug.LogError(
                "Corridor: Uma das portas recebidas é nula."
            );

            return;
        }


        Vector3 start =
            doorA.transform.position;

        Vector3 end =
            doorB.transform.position;


        Vector3 center =
            (start + end) * 0.5f;


        transform.position = center;

        transform.rotation = Quaternion.identity;

        transform.localScale = Vector3.one;


        if (direction == CorridorDirection.Horizontal)
        {
            BuildHorizontal(
                start,
                end
            );
        }
        else
        {
            BuildVertical(
                start,
                end
            );
        }
    }


    // =====================================================
    // HORIZONTAL
    // =====================================================

    private void BuildHorizontal(
        Vector3 start,
        Vector3 end)
    {
        float length =
            Mathf.Abs(
                end.x - start.x
            );


        if (length <= 0f)
        {
            Debug.LogWarning(
                $"Corridor {name}: comprimento horizontal inválido."
            );

            return;
        }


        float halfWidth =
            corridorWidth * 0.5f;


        // =========================
        // GROUND
        // =========================

        SetupWall(
            firstWall,

            new Vector3(
                0f,
                -halfWidth - wallThickness * 0.5f,
                0f
            ),

            new Vector3(
                length,
                wallThickness,
                1f
            )
        );


        // =========================
        // CEILING
        // =========================

        SetupWall(
            secondWall,

            new Vector3(
                0f,
                halfWidth + wallThickness * 0.5f,
                0f
            ),

            new Vector3(
                length,
                wallThickness,
                1f
            )
        );
    }


    // =====================================================
    // VERTICAL
    // =====================================================

    private void BuildVertical(
        Vector3 start,
        Vector3 end)
    {
        float length =
            Mathf.Abs(
                end.y - start.y
            );


        if (length <= 0f)
        {
            Debug.LogWarning(
                $"Corridor {name}: comprimento vertical inválido."
            );

            return;
        }


        float halfWidth =
            corridorWidth * 0.5f;


        // =========================
        // LEFT WALL
        // =========================

        SetupWall(
            firstWall,

            new Vector3(
                -halfWidth - wallThickness * 0.5f,
                0f,
                0f
            ),

            new Vector3(
                wallThickness,
                length,
                1f
            )
        );


        // =========================
        // RIGHT WALL
        // =========================

        SetupWall(
            secondWall,

            new Vector3(
                halfWidth + wallThickness * 0.5f,
                0f,
                0f
            ),

            new Vector3(
                wallThickness,
                length,
                1f
            )
        );
    }


    // =====================================================
    // SETUP WALL
    // =====================================================

    private void SetupWall(
        Transform wall,
        Vector3 localPosition,
        Vector3 localScale)
    {
        if (wall == null)
        {
            Debug.LogError(
                $"Corridor {name}: Wall não foi atribuída."
            );

            return;
        }


        wall.localPosition =
            localPosition;

        wall.localRotation =
            Quaternion.identity;

        wall.localScale =
            localScale;
    }
}