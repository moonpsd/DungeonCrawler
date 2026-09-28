using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    // =========================================================
    // REFERÊNCIAS
    // =========================================================

    [Header("References")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private Transform platformsParent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform exitDoor;
    [SerializeField] private RoomExit roomExit;


    // =========================================================
    // TAMANHO DA SALA
    // =========================================================

    [Header("Room Size")]
    [SerializeField] private float roomWidth = 24f;
    [SerializeField] private float roomHeight = 16f;


    // =========================================================
    // PLATAFORMAS
    // =========================================================

    [Header("Platform")]
    [SerializeField] private float minPlatformWidth = 2f;
    [SerializeField] private float maxPlatformWidth = 4f;
    [SerializeField] private float platformHeight = 0.5f;

    // Espaço extra usado para evitar plataformas grudadas.
    [SerializeField] private float platformSafetyMargin = 0.35f;


    // =========================================================
    // CAMINHO
    // =========================================================

    [Header("Path Generation")]
    [SerializeField] private int minPathLength = 8;
    [SerializeField] private int maxPathLength = 12;

    // Quantas posições diferentes tentaremos antes
    // de usar uma solução de emergência.
    [SerializeField] private int attemptsPerPlatform = 30;


    // =========================================================
    // MOVIMENTO HORIZONTAL
    // =========================================================

    [Header("Horizontal Movement")]
    [SerializeField] private float minHorizontalGap = 0.8f;
    [SerializeField] private float maxHorizontalGap = 2.2f;


    // =========================================================
    // MOVIMENTO VERTICAL
    // =========================================================

    [Header("Vertical Movement")]
    [SerializeField] private float minVerticalStep = 1.0f;
    [SerializeField] private float maxVerticalStep = 1.8f;

    // Quando subimos/descemos, também podemos deslocar
    // um pouco horizontalmente.
    [SerializeField] private float minVerticalHorizontalShift = 0.8f;
    [SerializeField] private float maxVerticalHorizontalShift = 2.2f;


    // =========================================================
    // DIREÇÕES
    // =========================================================

    [Header("Direction Chances")]
    [Range(0f, 100f)]
    [SerializeField] private float rightChance = 45f;

    [Range(0f, 100f)]
    [SerializeField] private float upChance = 30f;

    [Range(0f, 100f)]
    [SerializeField] private float downChance = 20f;

    [Range(0f, 100f)]
    [SerializeField] private float leftChance = 5f;


    // =========================================================
    // SPAWN
    // =========================================================

    [Header("Player Spawn")]
    [SerializeField] private float playerSpawnHeight = 1.2f;


    // =========================================================
    // PORTA
    // =========================================================

    [Header("Exit Door")]
    [SerializeField] private float doorHeight = 1.5f;


    // =========================================================
    // DADOS INTERNOS
    // =========================================================

    private List<GameObject> generatedPlatforms =
        new List<GameObject>();

    private GameObject startPlatform;
    private GameObject endPlatform;


    // =========================================================
    // DIREÇÕES
    // =========================================================

    private enum PathDirection
    {
        Right,
        Up,
        Down,
        Left
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        GenerateRoom();
    }


    // =========================================================
    // REGENERAR SALA
    // =========================================================

    public void RegenerateRoom()
    {
        StartCoroutine(RegenerateRoutine());
    }


    private IEnumerator RegenerateRoutine()
    {
        ClearRoom();

        // Espera os objetos antigos realmente serem destruídos.
        yield return null;

        GenerateRoom();

        // Espera a física atualizar.
        yield return new WaitForFixedUpdate();

        Physics2D.SyncTransforms();
    }


    // =========================================================
    // LIMPAR SALA
    // =========================================================

    private void ClearRoom()
    {
        if (platformsParent == null)
        {
            return;
        }

        for (int i = platformsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(
                platformsParent.GetChild(i).gameObject
            );
        }

        generatedPlatforms.Clear();

        startPlatform = null;
        endPlatform = null;
    }


    // =========================================================
    // GERAR SALA
    // =========================================================

    private void GenerateRoom()
    {
        generatedPlatforms.Clear();

        if (platformPrefab == null)
        {
            Debug.LogError(
                "Platform Prefab não foi configurado."
            );

            return;
        }

        if (platformsParent == null)
        {
            Debug.LogError(
                "Platforms Parent não foi configurado."
            );

            return;
        }

        if (roomExit != null)
        {
            roomExit.ResetExit();
        }

        GenerateGuaranteedPath();

        SpawnPlayer();
    }


    // =========================================================
    // GERAR CAMINHO GARANTIDO
    // =========================================================

    private void GenerateGuaranteedPath()
    {
        float leftLimit =
            -roomWidth / 2f;

        float rightLimit =
            roomWidth / 2f;

        float bottomLimit =
            -roomHeight / 2f;

        float topLimit =
            roomHeight / 2f;


        // =====================================================
        // PLATAFORMA INICIAL
        // =====================================================

        float startWidth = 3f;

        float startX =
            leftLimit
            + startWidth / 2f
            + 1f;

        float startY =
            bottomLimit + 2f;


        startPlatform =
            CreatePlatform(
                startX,
                startY,
                startWidth,
                "StartPlatform"
            );


        generatedPlatforms.Add(
            startPlatform
        );


        // Plataforma atual.
        Vector2 currentPosition =
            new Vector2(
                startX,
                startY
            );

        float currentWidth =
            startWidth;


        int pathLength =
            Random.Range(
                minPathLength,
                maxPathLength + 1
            );


        PathDirection previousDirection =
            PathDirection.Right;


        // =====================================================
        // CRIAR CAMINHO
        // =====================================================

        for (
            int i = 0;
            i < pathLength;
            i++
        )
        {
            bool created = false;


            // =================================================
            // TENTAR POSIÇÕES ALEATÓRIAS
            // =================================================

            for (
                int attempt = 0;
                attempt < attemptsPerPlatform;
                attempt++
            )
            {
                PathDirection direction =
                    ChooseDirection(
                        currentPosition,
                        previousDirection,
                        leftLimit,
                        rightLimit,
                        bottomLimit,
                        topLimit
                    );


                float nextWidth =
                    Random.Range(
                        minPlatformWidth,
                        maxPlatformWidth
                    );


                Vector2 nextPosition =
                    CalculateNextPosition(
                        currentPosition,
                        currentWidth,
                        nextWidth,
                        direction
                    );


                // =============================================
                // LIMITES DA SALA
                // =============================================

                if (
                    !IsInsideRoom(
                        nextPosition,
                        nextWidth,
                        leftLimit,
                        rightLimit,
                        bottomLimit,
                        topLimit
                    )
                )
                {
                    continue;
                }


                // =============================================
                // COLISÃO
                // =============================================

                if (
                    IsOverlapping(
                        nextPosition,
                        nextWidth
                    )
                )
                {
                    continue;
                }


                // =============================================
                // CRIAR
                // =============================================

                GameObject platform =
                    CreatePlatform(
                        nextPosition.x,
                        nextPosition.y,
                        nextWidth,
                        "PathPlatform_" + i
                    );


                generatedPlatforms.Add(
                    platform
                );


                currentPosition =
                    nextPosition;

                currentWidth =
                    nextWidth;

                previousDirection =
                    direction;

                created = true;

                break;
            }


            // =================================================
            // FALLBACK
            // =================================================
            //
            // Se nenhuma posição aleatória funcionar,
            // tentamos criar uma plataforma segura à direita.
            // =================================================

            if (!created)
            {
                bool fallbackCreated =
                    TryCreateFallbackPlatform(
                        ref currentPosition,
                        ref currentWidth,
                        i,
                        leftLimit,
                        rightLimit,
                        bottomLimit,
                        topLimit
                    );


                if (!fallbackCreated)
                {
                    Debug.Log(
                        "Caminho terminou antes do limite em: "
                        + i
                    );

                    break;
                }


                previousDirection =
                    PathDirection.Right;
            }
        }


        // =====================================================
        // A ÚLTIMA PLATAFORMA VIRA A PLATAFORMA FINAL
        // =====================================================

        CreateExitPlatform(
            currentPosition,
            currentWidth,
            leftLimit,
            rightLimit,
            bottomLimit,
            topLimit
        );
    }


    // =========================================================
    // ESCOLHER DIREÇÃO
    // =========================================================

    private PathDirection ChooseDirection(
        Vector2 currentPosition,
        PathDirection previousDirection,
        float leftLimit,
        float rightLimit,
        float bottomLimit,
        float topLimit
    )
    {
        float adjustedRight =
            rightChance;

        float adjustedUp =
            upChance;

        float adjustedDown =
            downChance;

        float adjustedLeft =
            leftChance;


        // =====================================================
        // EVITAR BORDAS
        // =====================================================

        float horizontalMargin = 4f;
        float verticalMargin = 3f;


        if (
            currentPosition.x
            > rightLimit - horizontalMargin
        )
        {
            adjustedRight *= 0.15f;

            adjustedLeft += 20f;
            adjustedUp += 15f;
            adjustedDown += 15f;
        }


        if (
            currentPosition.x
            < leftLimit + horizontalMargin
        )
        {
            adjustedLeft *= 0.1f;

            adjustedRight += 30f;
        }


        if (
            currentPosition.y
            > topLimit - verticalMargin
        )
        {
            adjustedUp = 0f;

            adjustedDown += 30f;
        }


        if (
            currentPosition.y
            < bottomLimit + verticalMargin
        )
        {
            adjustedDown = 0f;

            adjustedUp += 30f;
        }


        // =====================================================
        // EVITAR ZIG-ZAG VERTICAL EXAGERADO
        // =====================================================

        if (
            previousDirection
            == PathDirection.Up
        )
        {
            adjustedDown *= 0.25f;
        }


        if (
            previousDirection
            == PathDirection.Down
        )
        {
            adjustedUp *= 0.25f;
        }


        // =====================================================
        // ESCOLHA
        // =====================================================

        float total =
            adjustedRight
            + adjustedUp
            + adjustedDown
            + adjustedLeft;


        if (total <= 0f)
        {
            return PathDirection.Right;
        }


        float roll =
            Random.Range(
                0f,
                total
            );


        if (roll < adjustedRight)
        {
            return PathDirection.Right;
        }


        roll -= adjustedRight;


        if (roll < adjustedUp)
        {
            return PathDirection.Up;
        }


        roll -= adjustedUp;


        if (roll < adjustedDown)
        {
            return PathDirection.Down;
        }


        return PathDirection.Left;
    }


    // =========================================================
    // CALCULAR PRÓXIMA POSIÇÃO
    // =========================================================

    private Vector2 CalculateNextPosition(
        Vector2 currentPosition,
        float currentWidth,
        float nextWidth,
        PathDirection direction
    )
    {
        float horizontalGap =
            Random.Range(
                minHorizontalGap,
                maxHorizontalGap
            );


        float verticalStep =
            Random.Range(
                minVerticalStep,
                maxVerticalStep
            );


        float verticalHorizontalShift =
            Random.Range(
                minVerticalHorizontalShift,
                maxVerticalHorizontalShift
            );


        Vector2 nextPosition =
            currentPosition;


        switch (direction)
        {
            // =================================================
            // DIREITA
            // =================================================

            case PathDirection.Right:

                nextPosition.x +=
                    currentWidth / 2f
                    + horizontalGap
                    + nextWidth / 2f;

                // Pequena variação de altura.
                nextPosition.y +=
                    Random.Range(
                        -0.35f,
                        0.35f
                    );

                break;


            // =================================================
            // ESQUERDA
            // =================================================

            case PathDirection.Left:

                nextPosition.x -=
                    currentWidth / 2f
                    + horizontalGap
                    + nextWidth / 2f;

                nextPosition.y +=
                    Random.Range(
                        -0.35f,
                        0.35f
                    );

                break;


            // =================================================
            // CIMA
            // =================================================

            case PathDirection.Up:

                nextPosition.y +=
                    verticalStep;

                nextPosition.x +=
                    Random.Range(
                        -verticalHorizontalShift,
                        verticalHorizontalShift
                    );

                break;


            // =================================================
            // BAIXO
            // =================================================

            case PathDirection.Down:

                nextPosition.y -=
                    verticalStep;

                nextPosition.x +=
                    Random.Range(
                        -verticalHorizontalShift,
                        verticalHorizontalShift
                    );

                break;
        }


        return nextPosition;
    }


    // =========================================================
    // VERIFICAR LIMITES
    // =========================================================

    private bool IsInsideRoom(
        Vector2 position,
        float width,
        float leftLimit,
        float rightLimit,
        float bottomLimit,
        float topLimit
    )
    {
        float halfWidth =
            width / 2f;


        float left =
            position.x - halfWidth;

        float right =
            position.x + halfWidth;


        float bottom =
            position.y
            - platformHeight / 2f;

        float top =
            position.y
            + platformHeight / 2f;


        float wallMargin = 0.5f;


        if (
            left
            < leftLimit + wallMargin
        )
        {
            return false;
        }


        if (
            right
            > rightLimit - wallMargin
        )
        {
            return false;
        }


        if (
            bottom
            < bottomLimit + wallMargin
        )
        {
            return false;
        }


        if (
            top
            > topLimit - wallMargin
        )
        {
            return false;
        }


        return true;
    }


    // =========================================================
    // VERIFICAR SOBREPOSIÇÃO
    // =========================================================

    private bool IsOverlapping(
        Vector2 position,
        float width
    )
    {
        foreach (
            GameObject platform
            in generatedPlatforms
        )
        {
            if (platform == null)
            {
                continue;
            }


            Vector2 otherPosition =
                platform.transform.localPosition;


            float otherWidth =
                platform.transform.localScale.x;


            float horizontalDistance =
                Mathf.Abs(
                    position.x
                    - otherPosition.x
                );


            float verticalDistance =
                Mathf.Abs(
                    position.y
                    - otherPosition.y
                );


            float requiredHorizontalDistance =
                width / 2f
                + otherWidth / 2f
                + platformSafetyMargin;


            float requiredVerticalDistance =
                platformHeight
                + platformSafetyMargin;


            // Os retângulos estão ocupando
            // praticamente a mesma região.
            if (
                horizontalDistance
                    < requiredHorizontalDistance
                &&
                verticalDistance
                    < requiredVerticalDistance
            )
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // FALLBACK
    // =========================================================

    private bool TryCreateFallbackPlatform(
        ref Vector2 currentPosition,
        ref float currentWidth,
        int index,
        float leftLimit,
        float rightLimit,
        float bottomLimit,
        float topLimit
    )
    {
        float nextWidth =
            Mathf.Lerp(
                minPlatformWidth,
                maxPlatformWidth,
                0.5f
            );


        // Primeiro tentamos direita.
        Vector2 nextPosition =
            currentPosition;


        nextPosition.x +=
            currentWidth / 2f
            + minHorizontalGap
            + nextWidth / 2f;


        if (
            IsInsideRoom(
                nextPosition,
                nextWidth,
                leftLimit,
                rightLimit,
                bottomLimit,
                topLimit
            )
            &&
            !IsOverlapping(
                nextPosition,
                nextWidth
            )
        )
        {
            GameObject platform =
                CreatePlatform(
                    nextPosition.x,
                    nextPosition.y,
                    nextWidth,
                    "FallbackPlatform_" + index
                );


            generatedPlatforms.Add(
                platform
            );


            currentPosition =
                nextPosition;

            currentWidth =
                nextWidth;


            return true;
        }


        // =====================================================
        // TENTAR SUBIR
        // =====================================================

        nextPosition =
            currentPosition
            + new Vector2(
                0f,
                minVerticalStep
            );


        if (
            IsInsideRoom(
                nextPosition,
                nextWidth,
                leftLimit,
                rightLimit,
                bottomLimit,
                topLimit
            )
            &&
            !IsOverlapping(
                nextPosition,
                nextWidth
            )
        )
        {
            GameObject platform =
                CreatePlatform(
                    nextPosition.x,
                    nextPosition.y,
                    nextWidth,
                    "FallbackPlatform_" + index
                );


            generatedPlatforms.Add(
                platform
            );


            currentPosition =
                nextPosition;

            currentWidth =
                nextWidth;


            return true;
        }


        // =====================================================
        // TENTAR DESCER
        // =====================================================

        nextPosition =
            currentPosition
            + new Vector2(
                0f,
                -minVerticalStep
            );


        if (
            IsInsideRoom(
                nextPosition,
                nextWidth,
                leftLimit,
                rightLimit,
                bottomLimit,
                topLimit
            )
            &&
            !IsOverlapping(
                nextPosition,
                nextWidth
            )
        )
        {
            GameObject platform =
                CreatePlatform(
                    nextPosition.x,
                    nextPosition.y,
                    nextWidth,
                    "FallbackPlatform_" + index
                );


            generatedPlatforms.Add(
                platform
            );


            currentPosition =
                nextPosition;

            currentWidth =
                nextWidth;


            return true;
        }


        return false;
    }


    // =========================================================
    // CRIAR PLATAFORMA DA SAÍDA
    // =========================================================

    private void CreateExitPlatform(
        Vector2 currentPosition,
        float currentWidth,
        float leftLimit,
        float rightLimit,
        float bottomLimit,
        float topLimit
    )
    {
        // A própria última plataforma pode
        // funcionar como plataforma final.
        if (generatedPlatforms.Count == 0)
        {
            return;
        }


        endPlatform =
            generatedPlatforms[
                generatedPlatforms.Count - 1
            ];


        endPlatform.name =
            "EndPlatform";


        Vector2 endPosition =
            endPlatform.transform.localPosition;


        PositionExitDoor(
            endPosition.x,
            endPosition.y
        );
    }


    // =========================================================
    // POSICIONAR PORTA
    // =========================================================

    private void PositionExitDoor(
        float platformX,
        float platformY
    )
    {
        if (exitDoor == null)
        {
            Debug.LogWarning(
                "ExitDoor não foi configurada."
            );

            return;
        }


        exitDoor.localPosition =
            new Vector3(
                platformX,
                platformY
                    + platformHeight / 2f
                    + doorHeight / 2f,
                exitDoor.localPosition.z
            );
    }


    // =========================================================
    // SPAWN DO PLAYER
    // =========================================================

    private void SpawnPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning(
                "Player não foi configurado."
            );

            return;
        }


        if (startPlatform == null)
        {
            Debug.LogError(
                "StartPlatform não existe."
            );

            return;
        }


        Vector3 platformWorldPosition =
            startPlatform.transform.position;


        float platformTop =
            platformWorldPosition.y
            + platformHeight / 2f;


        Vector3 spawnPosition =
            new Vector3(
                platformWorldPosition.x,
                platformTop
                    + playerSpawnHeight,
                player.position.z
            );


        // =====================================================
        // ZERAR FÍSICA
        // =====================================================

        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();


        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            rb.position =
                new Vector2(
                    spawnPosition.x,
                    spawnPosition.y
                );
        }


        player.position =
            spawnPosition;


        Physics2D.SyncTransforms();


        Debug.Log(
            "PLAYER SPAWNADO EM: "
            + spawnPosition
        );
    }


    // =========================================================
    // CRIAR PLATAFORMA
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


        platform.name =
            platformName;


        platform.transform.localPosition =
            new Vector3(
                x,
                y,
                0f
            );


        platform.transform.localScale =
            new Vector3(
                width,
                platformHeight,
                1f
            );


        return platform;
    }
}