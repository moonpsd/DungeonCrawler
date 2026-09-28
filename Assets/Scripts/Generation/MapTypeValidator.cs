using System.Collections.Generic;
using UnityEngine;

public static class MapTypeValidator
{
    // =====================================================
    // MAIN VALIDATION
    // =====================================================

    public static bool Validate(MapGraph graph)
    {
        if (graph == null || graph.NodeCount == 0)
        {
            Debug.LogWarning(
                "MapTypeValidator: Graph is null or empty."
            );

            return false;
        }

        bool valid = true;

        List<MapNode> mainPath =
            GetMainPath(graph);


        if (!ValidateSpecialRoomCount(graph))
        {
            valid = false;
        }


        if (!ValidateBossPosition(mainPath))
        {
            valid = false;
        }


        if (!ValidatePreBossPosition(mainPath))
        {
            valid = false;
        }


        if (!ValidateBranches(graph))
        {
            valid = false;
        }


        if (!ValidateStartArea(mainPath))
        {
            valid = false;
        }


        if (valid)
        {
            Debug.Log(
                "MapTypeValidator: Room types are VALID."
            );
        }
        else
        {
            Debug.LogWarning(
                "MapTypeValidator: Room types are INVALID."
            );
        }


        return valid;
    }


    // =====================================================
    // SPECIAL ROOM COUNT
    // =====================================================

    private static bool ValidateSpecialRoomCount(
        MapGraph graph)
    {
        int startCount = 0;
        int bossCount = 0;
        int preBossCount = 0;


        foreach (MapNode node in graph.Nodes)
        {
            switch (node.RoomType)
            {
                case RoomType.Start:
                    startCount++;
                    break;

                case RoomType.Boss:
                    bossCount++;
                    break;

                case RoomType.PreBoss:
                    preBossCount++;
                    break;
            }
        }


        if (startCount != 1)
        {
            Debug.LogWarning(
                $"MapTypeValidator: Expected 1 Start, found {startCount}."
            );

            return false;
        }


        if (bossCount != 1)
        {
            Debug.LogWarning(
                $"MapTypeValidator: Expected 1 Boss, found {bossCount}."
            );

            return false;
        }


        if (preBossCount != 1)
        {
            Debug.LogWarning(
                $"MapTypeValidator: Expected 1 PreBoss, found {preBossCount}."
            );

            return false;
        }


        return true;
    }


    // =====================================================
    // BOSS
    // =====================================================

    private static bool ValidateBossPosition(
        List<MapNode> mainPath)
    {
        if (mainPath.Count == 0)
        {
            Debug.LogWarning(
                "MapTypeValidator: Main Path is empty."
            );

            return false;
        }


        MapNode lastNode =
            mainPath[mainPath.Count - 1];


        if (lastNode.RoomType != RoomType.Boss)
        {
            Debug.LogWarning(
                $"MapTypeValidator: Last Main Path node ({lastNode.ID}) is not Boss."
            );

            return false;
        }


        if (lastNode.PathType !=
            NodePathType.MainPath)
        {
            Debug.LogWarning(
                "MapTypeValidator: Boss is not on Main Path."
            );

            return false;
        }


        return true;
    }


    // =====================================================
    // PRE BOSS
    // =====================================================

    private static bool ValidatePreBossPosition(
        List<MapNode> mainPath)
    {
        if (mainPath.Count < 2)
        {
            return false;
        }


        MapNode preBoss =
            mainPath[mainPath.Count - 2];


        if (preBoss.RoomType !=
            RoomType.PreBoss)
        {
            Debug.LogWarning(
                $"MapTypeValidator: Penultimate Main Path node ({preBoss.ID}) is not PreBoss."
            );

            return false;
        }


        return true;
    }


    // =====================================================
    // BRANCHES
    // =====================================================

    private static bool ValidateBranches(
        MapGraph graph)
    {
        foreach (MapNode node in graph.Nodes)
        {
            if (node.PathType !=
                NodePathType.Branch)
            {
                continue;
            }


            // Esses tipos nunca podem
            // existir em branches.
            if (
                node.RoomType == RoomType.Start ||
                node.RoomType == RoomType.Boss ||
                node.RoomType == RoomType.PreBoss ||
                node.RoomType == RoomType.Elite)
            {
                Debug.LogWarning(
                    $"MapTypeValidator: Branch node {node.ID} has forbidden type {node.RoomType}."
                );

                return false;
            }


            if (IsBranchEnd(node))
            {
                if (!IsUsefulBranchEnd(
                        node.RoomType))
                {
                    Debug.LogWarning(
                        $"MapTypeValidator: Branch end {node.ID} has invalid type {node.RoomType}."
                    );

                    return false;
                }
            }
        }


        return true;
    }


    // =====================================================
    // START AREA
    // =====================================================

    private static bool ValidateStartArea(
        List<MapNode> mainPath)
    {
        if (mainPath.Count == 0)
        {
            return false;
        }


        if (mainPath[0].RoomType !=
            RoomType.Start)
        {
            Debug.LogWarning(
                "MapTypeValidator: First Main Path node is not Start."
            );

            return false;
        }


        // D1 e D2 não podem começar
        // com salas especiais pesadas.
        int protectedNodes =
            Mathf.Min(
                3,
                mainPath.Count
            );


        for (
            int i = 1;
            i < protectedNodes;
            i++)
        {
            RoomType type =
                mainPath[i].RoomType;


            if (
                type == RoomType.Elite ||
                type == RoomType.Rest ||
                type == RoomType.Boss ||
                type == RoomType.PreBoss)
            {
                Debug.LogWarning(
                    $"MapTypeValidator: Main Path node {mainPath[i].ID} has forbidden early type {type}."
                );

                return false;
            }
        }


        return true;
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private static bool IsBranchEnd(
        MapNode node)
    {
        return
            node.PathType ==
            NodePathType.Branch &&
            node.Connections.Count == 1;
    }


    private static bool IsUsefulBranchEnd(
        RoomType type)
    {
        return
            type == RoomType.Reward ||
            type == RoomType.Shop ||
            type == RoomType.Event ||
            type == RoomType.Secret;
    }


    private static List<MapNode>
        GetMainPath(
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


        result.Sort(
            (a, b) =>
                a.Depth.CompareTo(
                    b.Depth
                )
        );


        return result;
    }
}