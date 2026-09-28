using System.Collections.Generic;
using UnityEngine;

public static class RoomTypeAssigner
{
    // =========================
    // MAIN
    // =========================

    public static void AssignRoomTypes(
        MapGraph graph)
    {
        if (graph == null ||
            graph.NodeCount == 0)
        {
            Debug.LogWarning(
                "RoomTypeAssigner: Graph is empty."
            );

            return;
        }

        List<MapNode> mainPath =
            GetMainPathNodes(graph);

        if (mainPath.Count == 0)
        {
            Debug.LogWarning(
                "RoomTypeAssigner: No Main Path found."
            );

            return;
        }


        ResetRoomTypes(graph);

        AssignMainPath(mainPath);

        AssignBranches(graph);

        Debug.Log(
            "RoomTypeAssigner: Room types assigned."
        );
    }


    // =========================
    // RESET
    // =========================

    private static void ResetRoomTypes(
        MapGraph graph)
    {
        foreach (MapNode node in graph.Nodes)
        {
            node.SetRoomType(
                RoomType.Exploration
            );
        }
    }


    // =========================
    // MAIN PATH
    // =========================

    private static void AssignMainPath(
        List<MapNode> mainPath)
    {
        // Primeiro node sempre é Start.
        mainPath[0].SetRoomType(
            RoomType.Start
        );


        // Precisamos de espaço para
        // PreBoss + Boss.
        if (mainPath.Count >= 3)
        {
            MapNode preBoss =
                mainPath[
                    mainPath.Count - 2
                ];

            MapNode boss =
                mainPath[
                    mainPath.Count - 1
                ];


            preBoss.SetRoomType(
                RoomType.PreBoss
            );

            boss.SetRoomType(
                RoomType.Boss
            );
        }


        // Nodes intermediários.
        for (
            int i = 1;
            i < mainPath.Count - 2;
            i++)
        {
            MapNode node =
                mainPath[i];


            float progress =
                (float)i /
                (mainPath.Count - 1);


            RoomType type =
                ChooseMainPathRoomType(
                    progress
                );


            node.SetRoomType(type);
        }


        AssignRestRoom(mainPath);

        AssignEliteRoom(mainPath);
    }


    // =========================
    // MAIN PATH RANDOM TYPE
    // =========================

    private static RoomType
        ChooseMainPathRoomType(
            float progress)
    {
        float roll =
            Random.value;


        // =====================
        // INÍCIO DA RUN
        // =====================

        if (progress < 0.30f)
        {
            if (roll < 0.65f)
            {
                return RoomType.Exploration;
            }

            return RoomType.Combat;
        }


        // =====================
        // MEIO DA RUN
        // =====================

        if (progress < 0.70f)
        {
            if (roll < 0.40f)
            {
                return RoomType.Exploration;
            }

            if (roll < 0.85f)
            {
                return RoomType.Combat;
            }

            return RoomType.Event;
        }


        // =====================
        // FINAL DA RUN
        // =====================

        if (roll < 0.30f)
        {
            return RoomType.Exploration;
        }

        return RoomType.Combat;
    }


    // =========================
    // REST
    // =========================

    private static void AssignRestRoom(
        List<MapNode> mainPath)
    {
        // Queremos um Rest antes da região
        // final, mas nunca sobrescrevendo
        // PreBoss/Boss.

        if (mainPath.Count < 7)
        {
            return;
        }


        int minIndex =
            Mathf.RoundToInt(
                mainPath.Count * 0.55f
            );

        int maxIndex =
            mainPath.Count - 3;


        if (minIndex > maxIndex)
        {
            return;
        }


        int index =
            Random.Range(
                minIndex,
                maxIndex + 1
            );


        mainPath[index].SetRoomType(
            RoomType.Rest
        );
    }


    // =========================
    // ELITE
    // =========================

    private static void AssignEliteRoom(
        List<MapNode> mainPath)
    {
        // Elite só aparece depois
        // de certa progressão.

        if (mainPath.Count < 8)
        {
            return;
        }


        int minIndex =
            Mathf.RoundToInt(
                mainPath.Count * 0.45f
            );

        int maxIndex =
            mainPath.Count - 4;


        List<int> candidates =
            new List<int>();


        for (
            int i = minIndex;
            i <= maxIndex;
            i++)
        {
            RoomType type =
                mainPath[i].RoomType;


            if (
                type != RoomType.Rest &&
                type != RoomType.PreBoss &&
                type != RoomType.Boss)
            {
                candidates.Add(i);
            }
        }


        if (candidates.Count == 0)
        {
            return;
        }


        int chosenIndex =
            candidates[
                Random.Range(
                    0,
                    candidates.Count
                )
            ];


        mainPath[
            chosenIndex
        ].SetRoomType(
            RoomType.Elite
        );
    }


    // =========================
    // BRANCHES
    // =========================

    private static void AssignBranches(
        MapGraph graph)
    {
        foreach (MapNode node in graph.Nodes)
        {
            if (
                node.PathType !=
                NodePathType.Branch)
            {
                continue;
            }


            if (IsBranchEnd(graph, node))
            {
                node.SetRoomType(
                    ChooseBranchEndType()
                );
            }
            else
            {
                node.SetRoomType(
                    ChooseBranchNormalType()
                );
            }
        }
    }


    // =========================
    // BRANCH END
    // =========================

    private static bool IsBranchEnd(
        MapGraph graph,
        MapNode node)
    {
        int branchConnections = 0;


        foreach (
            int connectionID
            in node.Connections)
        {
            MapNode connected =
                graph.GetNode(
                    connectionID
                );


            if (
                connected != null &&
                connected.PathType ==
                NodePathType.Branch)
            {
                branchConnections++;
            }
        }


        // Uma branch terminal possui apenas
        // uma conexão total.
        return node.Connections.Count == 1;
    }


    private static RoomType
        ChooseBranchEndType()
    {
        float roll =
            Random.value;


        if (roll < 0.50f)
        {
            return RoomType.Reward;
        }


        if (roll < 0.70f)
        {
            return RoomType.Event;
        }


        if (roll < 0.85f)
        {
            return RoomType.Shop;
        }


        return RoomType.Secret;
    }


    // =========================
    // NORMAL BRANCH
    // =========================

    private static RoomType
        ChooseBranchNormalType()
    {
        float roll =
            Random.value;


        if (roll < 0.45f)
        {
            return RoomType.Exploration;
        }


        if (roll < 0.80f)
        {
            return RoomType.Combat;
        }


        return RoomType.Event;
    }


    // =========================
    // MAIN PATH LIST
    // =========================

    private static List<MapNode>
        GetMainPathNodes(
            MapGraph graph)
    {
        List<MapNode> result =
            new List<MapNode>();


        foreach (MapNode node in graph.Nodes)
        {
            if (
                node.PathType ==
                NodePathType.MainPath)
            {
                result.Add(node);
            }
        }


        // Depth representa a ordem
        // do caminho principal.
        result.Sort(
            (a, b) =>
                a.Depth.CompareTo(
                    b.Depth
                )
        );


        return result;
    }
}