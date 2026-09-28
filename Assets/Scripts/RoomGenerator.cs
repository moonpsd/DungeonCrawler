using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private Transform platformsParent;

    [Header("Room Size")]
    [SerializeField] private float roomWidth = 16f;
    [SerializeField] private float roomHeight = 9f;

    [Header("Platform")]
    [SerializeField] private float minPlatformWidth = 2f;
    [SerializeField] private float maxPlatformWidth = 4f;

    [Header("Jump")]
    [SerializeField] private float minHorizontalGap = 0.8f;
    [SerializeField] private float maxHorizontalGap = 2f;

    [SerializeField] private float maxJumpUp = 1.5f;
    [SerializeField] private float maxJumpDown = 2f;

    private void Start()
    {
        GenerateRoom();
    }

    private void GenerateRoom()
    {
        GenerateGround();
        GenerateMainPath();
    }

    // =========================================================
    // GROUND
    // =========================================================

    private void GenerateGround()
    {
        GameObject ground =
            Instantiate(platformPrefab, transform);

        ground.name = "Ground";

        ground.transform.localPosition =
            new Vector3(
                0f,
                -roomHeight / 2f,
                0f
            );

        ground.transform.localScale =
            new Vector3(
                roomWidth,
                0.5f,
                1f
            );
    }

    // =========================================================
    // MAIN PATH
    // =========================================================

    private void GenerateMainPath()
    {
        float leftLimit = -roomWidth / 2f;
        float rightLimit = roomWidth / 2f;

        float groundY = -roomHeight / 2f;

        // -----------------------------------------------------
        // PRIMEIRA PLATAFORMA
        // -----------------------------------------------------

        float currentWidth = 2.5f;

        float currentX =
            leftLimit + currentWidth / 2f;

        float currentY =
            groundY + 1.5f;

        CreatePlatform(
            currentX,
            currentY,
            currentWidth,
            "StartPlatform"
        );

        int platformNumber = 0;

        // -----------------------------------------------------
        // CRIA O CAMINHO
        // -----------------------------------------------------

        while (true)
        {
            // Borda direita da plataforma atual
            float currentRightEdge =
                currentX + currentWidth / 2f;

            // Espaço restante
            float remainingSpace =
                rightLimit - currentRightEdge;

            // Se estivermos perto do final, paramos
            if (remainingSpace < 3.5f)
                break;

            // ---------------------------------------------
            // Tamanho da próxima plataforma
            // ---------------------------------------------

            float nextWidth =
                Random.Range(
                    minPlatformWidth,
                    maxPlatformWidth
                );

            // ---------------------------------------------
            // GAP entre as plataformas
            // ---------------------------------------------

            float gap =
                Random.Range(
                    minHorizontalGap,
                    maxHorizontalGap
                );

            // ---------------------------------------------
            // Calcula X pela BORDA
            // ---------------------------------------------

            float nextX =
                currentRightEdge
                + gap
                + nextWidth / 2f;

            // Não deixa passar do limite da sala
            if (nextX + nextWidth / 2f >
                rightLimit - 1f)
            {
                break;
            }

            // ---------------------------------------------
            // Altura
            // ---------------------------------------------

            float verticalChange =
                Random.Range(
                    -maxJumpDown,
                    maxJumpUp
                );

            float nextY =
                currentY + verticalChange;

            float minY =
                groundY + 1f;

            float maxY =
                roomHeight / 2f - 1f;

            nextY =
                Mathf.Clamp(
                    nextY,
                    minY,
                    maxY
                );

            // ---------------------------------------------
            // Cria
            // ---------------------------------------------

            CreatePlatform(
                nextX,
                nextY,
                nextWidth,
                "PathPlatform_" + platformNumber
            );

            // Agora essa passa a ser a plataforma atual
            currentX = nextX;
            currentY = nextY;
            currentWidth = nextWidth;

            platformNumber++;
        }

        // -----------------------------------------------------
        // PLATAFORMA FINAL
        // -----------------------------------------------------

        float endWidth = 2.5f;

        float endX =
            rightLimit - endWidth / 2f;

        CreatePlatform(
            endX,
            currentY,
            endWidth,
            "EndPlatform"
        );
    }

    // =========================================================
    // CREATE PLATFORM
    // =========================================================

    private GameObject CreatePlatform(
        float x,
        float y,
        float width,
        string platformName
    )
    {
        GameObject platform =
            Instantiate(
                platformPrefab,
                platformsParent
            );

        platform.name = platformName;

        platform.transform.localPosition =
            new Vector3(
                x,
                y,
                0f
            );

        platform.transform.localScale =
            new Vector3(
                width,
                0.5f,
                1f
            );

        return platform;
    }
}