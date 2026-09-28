using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapGenerator : MonoBehaviour
{
    [Header("Main Path")]
    [SerializeField] private int mainPathLength = 15;

    [Range(0f, 1f)]
    [SerializeField] private float continueDirectionChance = 0.70f;


    [Header("Branches")]
    [SerializeField] private int numberOfBranches = 4;

    [SerializeField] private int minBranchLength = 2;

    [SerializeField] private int maxBranchLength = 5;

    [Range(0f, 1f)]
    [SerializeField] private float branchStraightChance = 0.65f;

    [SerializeField] private int maxConnectionsPerNode = 3;


    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private int maxGenerationAttempts = 20;


    [Header("Debug")]
    [SerializeField] private float gizmoSpacing = 3f;

    [SerializeField] private float nodeSize = 0.6f;

    [Header("Room Placement")]
    [SerializeField] private RoomPlacer roomPlacer;


    private MapGraph mapGraph;

    private readonly List<MapNode> mainPathNodes =
        new List<MapNode>();

    private int nextNodeID;


    private readonly Vector2Int[] directions =
    {
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.down
    };


    public MapGraph Graph => mapGraph;


    // =========================
    // UNITY
    // =========================

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateMap();
        }
    }


    // =========================
    // GENERATION
    // =========================

    public void GenerateMap()
    {
        bool validMap = false;
        int attempt = 0;


        // =====================================================
        // TENTATIVAS DE GERAÇÃO
        // =====================================================

        while (!validMap && attempt < maxGenerationAttempts)
        {
            attempt++;


            // =========================
            // RESET
            // =========================

            mapGraph = new MapGraph();

            mainPathNodes.Clear();

            nextNodeID = 0;


            // =========================
            // STRUCTURE
            // =========================

            GenerateMainPath();

            GenerateBranches();


            // =========================
            // STRUCTURE VALIDATION
            // =========================

            bool structureValid =
                MapValidator.Validate(
                    mapGraph,
                    mainPathLength,
                    maxConnectionsPerNode
                );


            if (!structureValid)
            {
                Debug.LogWarning(
                    $"Generation attempt {attempt}: Structure invalid. Retrying..."
                );

                continue;
            }


            // =========================
            // ROOM TYPES
            // =========================

            RoomTypeAssigner.AssignRoomTypes(
                mapGraph
            );


            // =========================
            // PROGRESSION RULES
            // =========================

            ProgressionRules.Apply(
                mapGraph
            );


            // =========================
            // TYPE VALIDATION
            // =========================

            bool typesValid =
                MapTypeValidator.Validate(
                    mapGraph
                );


            if (!typesValid)
            {
                Debug.LogWarning(
                    $"Generation attempt {attempt}: Room types invalid. Retrying..."
                );

                continue;
            }


            // =========================
            // MAPA APROVADO
            // =========================

            validMap = true;
        }


        // =====================================================
        // RESULTADO FINAL
        // =====================================================

        if (!validMap)
        {
            Debug.LogError(
                $"Could not generate a valid map after {maxGenerationAttempts} attempts."
            );

            return;
        }


        Debug.Log(
            $"Map generated successfully after {attempt} attempt(s). " +
            $"Nodes: {mapGraph.NodeCount}"
        );


        // =====================================================
        // ROOM PLACEMENT
        // =====================================================

        if (roomPlacer == null)
        {
            Debug.LogError(
                "MapGenerator: RoomPlacer não foi atribuído no Inspector."
            );

            return;
        }


        roomPlacer.PlaceRooms();
    }

    // =========================
    // MAIN PATH
    // =========================

    private void GenerateMainPath()
    {
        MapNode startNode = new MapNode(
            GetNextNodeID(),
            Vector2Int.zero,
            0,
            RoomType.Start,
            NodePathType.MainPath
        );

        mapGraph.AddNode(startNode);

        mainPathNodes.Add(startNode);

        MapNode currentNode = startNode;

        Vector2Int previousDirection =
            Vector2Int.right;


        while (mainPathNodes.Count < mainPathLength)
        {
            List<Vector2Int> availableDirections =
                GetAvailableDirections(
                    currentNode.GridPosition
                );


            if (availableDirections.Count == 0)
            {
                Debug.LogWarning(
                    "Main path got stuck."
                );

                break;
            }


            Vector2Int chosenDirection =
                ChooseDirection(
                    availableDirections,
                    previousDirection,
                    continueDirectionChance
                );


            Vector2Int newPosition =
                currentNode.GridPosition +
                chosenDirection;


            MapNode newNode = new MapNode(
                GetNextNodeID(),
                newPosition,
                currentNode.Depth + 1,
                RoomType.Exploration,
                NodePathType.MainPath
            );


            mapGraph.AddNode(newNode);

            mapGraph.ConnectNodes(
                currentNode.ID,
                newNode.ID
            );


            mainPathNodes.Add(newNode);

            previousDirection =
                chosenDirection;

            currentNode =
                newNode;
        }
    }


    // =========================
    // BRANCHES
    // =========================

    private void GenerateBranches()
    {
        if (mainPathNodes.Count < 3)
        {
            return;
        }


        int branchesCreated = 0;

        int attempts = 0;

        int maxAttempts =
            numberOfBranches * 20;


        while (
            branchesCreated < numberOfBranches &&
            attempts < maxAttempts)
        {
            attempts++;


            // Nunca cria branch no Start
            // nem no último node principal.
            int randomIndex = Random.Range(
                1,
                mainPathNodes.Count - 1
            );


            MapNode branchStart =
                mainPathNodes[randomIndex];


            if (
                branchStart.Connections.Count >=
                maxConnectionsPerNode)
            {
                continue;
            }


            int desiredLength =
                Random.Range(
                    minBranchLength,
                    maxBranchLength + 1
                );


            if (TryGenerateBranch(
                    randomIndex,
                    desiredLength))
            {
                branchesCreated++;
            }
        }


        Debug.Log(
            $"Branches created: {branchesCreated}"
        );
    }


    private bool TryGenerateBranch(
        int mainPathIndex,
        int desiredLength)
    {
        MapNode branchStart =
            mainPathNodes[mainPathIndex];


        List<Vector2Int> firstDirections =
            GetBranchStartDirections(
                mainPathIndex
            );


        if (firstDirections.Count == 0)
        {
            return false;
        }


        Vector2Int firstDirection =
            firstDirections[
                Random.Range(
                    0,
                    firstDirections.Count
                )
            ];


        List<MapNode> createdNodes =
            new List<MapNode>();


        MapNode currentNode =
            branchStart;

        Vector2Int previousDirection =
            firstDirection;


        for (int i = 0; i < desiredLength; i++)
        {
            List<Vector2Int> availableDirections =
                GetAvailableDirections(
                    currentNode.GridPosition
                );


            if (availableDirections.Count == 0)
            {
                break;
            }


            Vector2Int chosenDirection;


            // Primeiro node obrigatoriamente usa
            // a direção perpendicular escolhida.
            if (i == 0)
            {
                if (!availableDirections.Contains(
                        firstDirection))
                {
                    break;
                }

                chosenDirection =
                    firstDirection;
            }
            else
            {
                chosenDirection =
                    ChooseDirection(
                        availableDirections,
                        previousDirection,
                        branchStraightChance
                    );
            }


            Vector2Int newPosition =
                currentNode.GridPosition +
                chosenDirection;


            MapNode newNode =
                new MapNode(
                    GetNextNodeID(),
                    newPosition,
                    currentNode.Depth + 1,
                    RoomType.Exploration,
                    NodePathType.Branch
                );


            mapGraph.AddNode(newNode);

            mapGraph.ConnectNodes(
                currentNode.ID,
                newNode.ID
            );


            createdNodes.Add(newNode);

            currentNode =
                newNode;

            previousDirection =
                chosenDirection;
        }


        // Branch ficou pequena demais.
        // Apaga tudo que essa tentativa criou.
        if (createdNodes.Count <
            minBranchLength)
        {
            RollbackBranch(
                createdNodes
            );

            return false;
        }


        return true;
    }


    // =========================
    // BRANCH START DIRECTION
    // =========================

    private List<Vector2Int>
        GetBranchStartDirections(
            int mainPathIndex)
    {
        List<Vector2Int> result =
            new List<Vector2Int>();


        MapNode current =
            mainPathNodes[mainPathIndex];


        MapNode previous =
            mainPathNodes[
                mainPathIndex - 1
            ];


        MapNode next =
            mainPathNodes[
                mainPathIndex + 1
            ];


        Vector2Int incomingDirection =
            current.GridPosition -
            previous.GridPosition;


        Vector2Int outgoingDirection =
            next.GridPosition -
            current.GridPosition;


        // Se o caminho principal estiver
        // horizontal nesse ponto:
        //
        // M --- M --- M
        //
        // branch tenta sair para cima/baixo.

        if (
            IsHorizontal(incomingDirection) &&
            IsHorizontal(outgoingDirection))
        {
            TryAddDirection(
                result,
                current.GridPosition,
                Vector2Int.up
            );

            TryAddDirection(
                result,
                current.GridPosition,
                Vector2Int.down
            );

            return result;
        }


        // Se estiver vertical:
        //
        // M
        // |
        // M
        // |
        // M
        //
        // branch tenta esquerda/direita.

        if (
            IsVertical(incomingDirection) &&
            IsVertical(outgoingDirection))
        {
            TryAddDirection(
                result,
                current.GridPosition,
                Vector2Int.left
            );

            TryAddDirection(
                result,
                current.GridPosition,
                Vector2Int.right
            );

            return result;
        }


        // Se estivermos em uma curva,
        // permitimos qualquer direção vazia,
        // exceto as ocupadas pelo Main Path.

        foreach (
            Vector2Int direction
            in directions)
        {
            TryAddDirection(
                result,
                current.GridPosition,
                direction
            );
        }


        return result;
    }


    private void TryAddDirection(
        List<Vector2Int> list,
        Vector2Int origin,
        Vector2Int direction)
    {
        Vector2Int target =
            origin + direction;


        if (!mapGraph.HasNodeAt(target))
        {
            list.Add(direction);
        }
    }


    // =========================
    // ROLLBACK
    // =========================

    private void RollbackBranch(
        List<MapNode> nodes)
    {
        // Remove de trás para frente.
        for (
            int i = nodes.Count - 1;
            i >= 0;
            i--)
        {
            mapGraph.RemoveNode(
                nodes[i]
            );
        }
    }


    // =========================
    // DIRECTIONS
    // =========================

    private Vector2Int ChooseDirection(
        List<Vector2Int> availableDirections,
        Vector2Int previousDirection,
        float straightChance)
    {
        bool canContinueStraight =
            availableDirections.Contains(
                previousDirection
            );


        if (
            canContinueStraight &&
            Random.value < straightChance)
        {
            return previousDirection;
        }


        List<Vector2Int> alternatives =
            new List<Vector2Int>();


        foreach (
            Vector2Int direction
            in availableDirections)
        {
            if (
                direction !=
                previousDirection)
            {
                alternatives.Add(
                    direction
                );
            }
        }


        if (alternatives.Count == 0)
        {
            return availableDirections[
                Random.Range(
                    0,
                    availableDirections.Count
                )
            ];
        }


        return alternatives[
            Random.Range(
                0,
                alternatives.Count
            )
        ];
    }


    private List<Vector2Int>
        GetAvailableDirections(
            Vector2Int currentPosition)
    {
        List<Vector2Int> available =
            new List<Vector2Int>();


        foreach (
            Vector2Int direction
            in directions)
        {
            Vector2Int target =
                currentPosition +
                direction;


            if (!mapGraph.HasNodeAt(target))
            {
                available.Add(
                    direction
                );
            }
        }


        return available;
    }


    private bool IsHorizontal(
        Vector2Int direction)
    {
        return direction.x != 0;
    }


    private bool IsVertical(
        Vector2Int direction)
    {
        return direction.y != 0;
    }


    // =========================
    // ID
    // =========================

    private int GetNextNodeID()
    {
        int id =
            nextNodeID;

        nextNodeID++;

        return id;
    }


    // =========================
    // GIZMOS
    // =========================

    private void OnDrawGizmos()
    {
        if (mapGraph == null)
        {
            return;
        }


        DrawConnections();

        DrawNodes();

        DrawNodeLabels();
    }


    private void DrawConnections()
    {
        Gizmos.color =
            Color.white;


        foreach (
            MapNode node
            in mapGraph.Nodes)
        {
            Vector3 nodePosition =
                GridToGizmoPosition(
                    node.GridPosition
                );


            foreach (
                int connectionID
                in node.Connections)
            {
                MapNode connectedNode =
                    mapGraph.GetNode(
                        connectionID
                    );


                if (connectedNode == null)
                {
                    continue;
                }


                Vector3 connectedPosition =
                    GridToGizmoPosition(
                        connectedNode.GridPosition
                    );


                Gizmos.DrawLine(
                    nodePosition,
                    connectedPosition
                );
            }
        }
    }


    private void DrawNodes()
    {
        foreach (
            MapNode node
            in mapGraph.Nodes)
        {
            Vector3 position =
                GridToGizmoPosition(
                    node.GridPosition
                );


            if (
                node.RoomType ==
                RoomType.Start)
            {
                Gizmos.color =
                    Color.green;
            }
            else if (
                node.PathType ==
                NodePathType.MainPath)
            {
                Gizmos.color =
                    Color.cyan;
            }
            else
            {
                Gizmos.color =
                    Color.yellow;
            }


            Gizmos.DrawCube(
                position,
                new Vector3(
                    nodeSize,
                    nodeSize,
                    nodeSize
                )
            );
        }
    }


    private void DrawNodeLabels()
    {
#if UNITY_EDITOR

        foreach (
            MapNode node
            in mapGraph.Nodes)
        {
            Vector3 position =
                GridToGizmoPosition(
                    node.GridPosition
                );


            position +=
                Vector3.up * 0.55f;


            string path =
                node.PathType ==
                NodePathType.MainPath
                    ? "M"
                    : "B";


            string label =
                $"{node.ID} | " +
                $"D{node.Depth} | " +
                $"{path} | " +
                $"{GetRoomTypeShortName(node.RoomType)}";


            Handles.Label(
                position,
                label
            );
        }

#endif
    }

    private string GetRoomTypeShortName(
    RoomType type)
    {
        switch (type)
        {
            case RoomType.Start:
                return "START";

            case RoomType.Exploration:
                return "EXP";

            case RoomType.Combat:
                return "COM";

            case RoomType.Reward:
                return "REW";

            case RoomType.Shop:
                return "SHOP";

            case RoomType.Event:
                return "EVT";

            case RoomType.Secret:
                return "SEC";

            case RoomType.Elite:
                return "ELITE";

            case RoomType.Rest:
                return "REST";

            case RoomType.PreBoss:
                return "PRE";

            case RoomType.Boss:
                return "BOSS";

            default:
                return "?";
        }
    }

    private Vector3 GridToGizmoPosition(
        Vector2Int gridPosition)
    {
        return transform.position +
               new Vector3(
                   gridPosition.x *
                   gizmoSpacing,

                   gridPosition.y *
                   gizmoSpacing,

                   0f
               );
    }
}