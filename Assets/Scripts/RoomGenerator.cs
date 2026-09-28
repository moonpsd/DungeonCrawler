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
    [SerializeField] private float roomWidth = 16f;
    [SerializeField] private float roomHeight = 9f;


    // =========================================================
    // PLATAFORMAS
    // =========================================================

    [Header("Platform")]
    [SerializeField] private float minPlatformWidth = 2f;
    [SerializeField] private float maxPlatformWidth = 4f;
    [SerializeField] private float platformHeight = 0.5f;


    // =========================================================
    // LIMITES DO PULO
    // =========================================================

    [Header("Player Jump Limits")]
    [SerializeField] private float minGap = 0.5f;
    [SerializeField] private float maxGap = 1.5f;

    [SerializeField] private float maxJumpUp = 1f;
    [SerializeField] private float maxJumpDown = 1.5f;


    // =========================================================
    // SPAWN
    // =========================================================

    [Header("Player Spawn")]
    [SerializeField] private float playerSpawnHeight = 1.2f;


    // =========================================================
    // GERAÇÃO
    // =========================================================

    [Header("Generation")]
    [SerializeField] private int maxAttempts = 20;


    private List<GameObject> generatedPlatforms =
        new List<GameObject>();


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
        // Primeiro apagamos a sala atual
        ClearRoom();

        // Esperamos um frame para o Unity
        // terminar os Destroy()
        yield return null;

        // Geramos uma nova sala
        GenerateRoom();
    }


    // =========================================================
    // LIMPAR SALA
    // =========================================================

    private void ClearRoom()
    {
        // Apaga todas as plataformas
        // que estão dentro de Platforms
        for (int i = platformsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(
                platformsParent.GetChild(i).gameObject
            );
        }

        generatedPlatforms.Clear();
    }


    // =========================================================
    // GERAR SALA
    // =========================================================

    private void GenerateRoom()
    {
        generatedPlatforms.Clear();

        // Reseta a porta para poder ser usada novamente
        if (roomExit != null)
        {
            roomExit.ResetExit();
        }

        GenerateMainPath();
    }


    // =========================================================
    // GERAR CAMINHO PRINCIPAL
    // =========================================================

    private void GenerateMainPath()
    {
        float leftLimit = -roomWidth / 2f;
        float rightLimit = roomWidth / 2f;

        float bottomLimit = -roomHeight / 2f;
        float topLimit = roomHeight / 2f;


        // =====================================================
        // START PLATFORM
        // =====================================================

        float currentWidth = 2.5f;

        float currentX =
            leftLimit + currentWidth / 2f;

        float currentY =
            bottomLimit + 0.5f;


        GameObject startPlatform =
            CreatePlatform(
                currentX,
                currentY,
                currentWidth,
                "StartPlatform"
            );

        generatedPlatforms.Add(startPlatform);


        // =====================================================
        // SPAWN DO PLAYER
        // =====================================================

        SpawnPlayer(
            currentX,
            currentY
        );


        // =====================================================
        // CAMINHO
        // =====================================================

        int platformNumber = 0;


        while (true)
        {
            float currentRightEdge =
                currentX + currentWidth / 2f;


            float remainingDistance =
                rightLimit - currentRightEdge;


            // Estamos próximos do final da sala
            if (remainingDistance <= 4f)
            {
                break;
            }


            bool platformCreated = false;


            // =================================================
            // TENTATIVAS
            // =================================================

            for (
                int attempt = 0;
                attempt < maxAttempts;
                attempt++
            )
            {
                // ---------------------------------------------
                // LARGURA
                // ---------------------------------------------

                float nextWidth =
                    Random.Range(
                        minPlatformWidth,
                        maxPlatformWidth
                    );


                // ---------------------------------------------
                // GAP
                // ---------------------------------------------

                float gap =
                    Random.Range(
                        minGap,
                        maxGap
                    );


                // ---------------------------------------------
                // X
                // ---------------------------------------------

                float nextX =
                    currentRightEdge
                    + gap
                    + nextWidth / 2f;


                // Não deixa passar da sala
                if (
                    nextX + nextWidth / 2f
                    >= rightLimit - 1f
                )
                {
                    continue;
                }


                // ---------------------------------------------
                // Y
                // ---------------------------------------------

                float verticalChange =
                    Random.Range(
                        -maxJumpDown,
                        maxJumpUp
                    );


                float nextY =
                    currentY + verticalChange;


                float minY =
                    bottomLimit + 0.5f;


                float maxY =
                    topLimit - 1f;


                nextY =
                    Mathf.Clamp(
                        nextY,
                        minY,
                        maxY
                    );


                // ---------------------------------------------
                // VERIFICAR PULO
                // ---------------------------------------------

                if (
                    !CanReachPlatform(
                        currentX,
                        currentY,
                        currentWidth,
                        nextX,
                        nextY,
                        nextWidth
                    )
                )
                {
                    continue;
                }


                // ---------------------------------------------
                // VERIFICAR SOBREPOSIÇÃO
                // ---------------------------------------------

                if (
                    IsOverlapping(
                        nextX,
                        nextY,
                        nextWidth
                    )
                )
                {
                    continue;
                }


                // ---------------------------------------------
                // CRIAR
                // ---------------------------------------------

                GameObject platform =
                    CreatePlatform(
                        nextX,
                        nextY,
                        nextWidth,
                        "PathPlatform_" + platformNumber
                    );


                generatedPlatforms.Add(platform);


                // Agora essa passa a ser
                // a plataforma atual
                currentX = nextX;
                currentY = nextY;
                currentWidth = nextWidth;


                platformNumber++;

                platformCreated = true;

                break;
            }


            // Não encontrou nenhuma posição válida
            if (!platformCreated)
            {
                Debug.LogWarning(
                    "Não foi possível continuar o caminho."
                );

                break;
            }
        }


        // =====================================================
        // FINAL DA SALA
        // =====================================================

        CreateEndPlatform(
            currentX,
            currentY,
            currentWidth,
            rightLimit
        );
    }


    // =========================================================
    // SPAWN PLAYER
    // =========================================================

    private void SpawnPlayer(
        float platformX,
        float platformY
    )
    {
        if (player == null)
        {
            Debug.LogError(
                "Player não foi configurado!"
            );

            return;
        }


        Vector3 spawnPosition =
            transform.TransformPoint(
                new Vector3(
                    platformX,
                    platformY + playerSpawnHeight,
                    0f
                )
            );


        spawnPosition.z =
            player.position.z;


        player.position =
            spawnPosition;


        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();


        if (rb != null)
        {
            // Para qualquer movimento anterior
            rb.linearVelocity = Vector2.zero;

            // Só impede o Player de girar.
            // NÃO trava X ou Y.
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;
        }
    }


    // =========================================================
    // VERIFICAR SE CONSEGUE PULAR
    // =========================================================

    private bool CanReachPlatform(
        float currentX,
        float currentY,
        float currentWidth,
        float nextX,
        float nextY,
        float nextWidth
    )
    {
        float currentRight =
            currentX
            + currentWidth / 2f;


        float nextLeft =
            nextX
            - nextWidth / 2f;


        float horizontalGap =
            nextLeft
            - currentRight;


        float verticalDifference =
            nextY
            - currentY;


        if (horizontalGap > maxGap)
        {
            return false;
        }


        if (horizontalGap < minGap)
        {
            return false;
        }


        if (verticalDifference > maxJumpUp)
        {
            return false;
        }


        if (verticalDifference < -maxJumpDown)
        {
            return false;
        }


        return true;
    }


    // =========================================================
    // VERIFICAR SOBREPOSIÇÃO
    // =========================================================

    private bool IsOverlapping(
        float x,
        float y,
        float width
    )
    {
        foreach (
            GameObject platform
            in generatedPlatforms
        )
        {
            float otherX =
                platform.transform.localPosition.x;


            float otherY =
                platform.transform.localPosition.y;


            float otherWidth =
                platform.transform.localScale.x;


            float horizontalDistance =
                Mathf.Abs(
                    x - otherX
                );


            float verticalDistance =
                Mathf.Abs(
                    y - otherY
                );


            float minimumHorizontalDistance =
                width / 2f
                + otherWidth / 2f;


            if (
                horizontalDistance
                    < minimumHorizontalDistance
                &&
                verticalDistance
                    < platformHeight
            )
            {
                return true;
            }
        }


        return false;
    }


    // =========================================================
    // CRIAR PLATAFORMA FINAL
    // =========================================================

    private void CreateEndPlatform(
        float currentX,
        float currentY,
        float currentWidth,
        float rightLimit
    )
    {
        float endWidth = 2.5f;


        float currentRight =
            currentX
            + currentWidth / 2f;


        float endX =
            rightLimit
            - endWidth / 2f;


        float endLeft =
            endX
            - endWidth / 2f;


        float gap =
            endLeft
            - currentRight;


        // =====================================================
        // CONSEGUE IR DIRETO PARA O FINAL
        // =====================================================

        if (
            gap >= minGap
            &&
            gap <= maxGap
        )
        {
            GameObject end =
                CreatePlatform(
                    endX,
                    currentY,
                    endWidth,
                    "EndPlatform"
                );


            generatedPlatforms.Add(end);


            PositionExitDoor(
                endX,
                currentY
            );


            return;
        }


        // =====================================================
        // CRIAR PONTE INTERMEDIÁRIA
        // =====================================================

        float middleWidth = 2f;


        float middleGap =
            Mathf.Clamp(
                gap / 2f,
                minGap,
                maxGap * 0.8f
            );


        float middleX =
            currentRight
            + middleGap
            + middleWidth / 2f;


        if (
            middleX + middleWidth / 2f
            < endLeft
        )
        {
            GameObject middle =
                CreatePlatform(
                    middleX,
                    currentY,
                    middleWidth,
                    "FinalBridge"
                );


            generatedPlatforms.Add(middle);
        }


        // =====================================================
        // END PLATFORM
        // =====================================================

        GameObject finalPlatform =
            CreatePlatform(
                endX,
                currentY,
                endWidth,
                "EndPlatform"
            );


        generatedPlatforms.Add(
            finalPlatform
        );


        PositionExitDoor(
            endX,
            currentY
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


        float doorHeight = 1.5f;


        exitDoor.localPosition =
            new Vector3(
                platformX,
                platformY
                    + platformHeight / 2f
                    + doorHeight / 2f,
                0f
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