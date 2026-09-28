using System.Collections.Generic;
using UnityEngine;

public static class ProgressionRules
{
    // =====================================================
    // CONFIGURAÇÕES
    // =====================================================

    // Quantos nodes iniciais ficam protegidos
    // de salas perigosas/especiais.
    private const int SafeStartNodes = 3;

    // Distância mínima entre salas especiais
    // no Main Path.
    private const int MinSpecialRoomDistance = 2;


    // =====================================================
    // MAIN
    // =====================================================

    public static void Apply(MapGraph graph)
    {
        if (graph == null || graph.NodeCount == 0)
        {
            Debug.LogWarning(
                "ProgressionRules: Graph is empty."
            );

            return;
        }

        List<MapNode> mainPath =
            GetMainPath(graph);

        if (mainPath.Count == 0)
        {
            Debug.LogWarning(
                "ProgressionRules: Main Path not found."
            );

            return;
        }

        ProtectStartArea(mainPath);

        ProtectBossSequence(mainPath);

        FixSpecialRoomSpacing(mainPath);

        FixBranchRewards(graph);

        Debug.Log(
            "ProgressionRules: Rules applied."
        );
    }


    // =====================================================
    // START AREA
    // =====================================================

    private static void ProtectStartArea(
        List<MapNode> mainPath)
    {
        int limit =
            Mathf.Min(
                SafeStartNodes,
                mainPath.Count
            );

        for (int i = 1; i < limit; i++)
        {
            MapNode node =
                mainPath[i];

            // Nada muito pesado no começo.
            if (
                node.RoomType == RoomType.Elite ||
                node.RoomType == RoomType.Rest ||
                node.RoomType == RoomType.Shop ||
                node.RoomType == RoomType.Reward ||
                node.RoomType == RoomType.PreBoss ||
                node.RoomType == RoomType.Boss)
            {
                node.SetRoomType(
                    RoomType.Exploration
                );
            }
        }
    }


    // =====================================================
    // BOSS SEQUENCE
    // =====================================================

    private static void ProtectBossSequence(
        List<MapNode> mainPath)
    {
        if (mainPath.Count < 3)
        {
            return;
        }

        int bossIndex =
            mainPath.Count - 1;

        int preBossIndex =
            mainPath.Count - 2;


        // Último node SEMPRE Boss.
        mainPath[bossIndex]
            .SetRoomType(
                RoomType.Boss
            );


        // Penúltimo SEMPRE PreBoss.
        mainPath[preBossIndex]
            .SetRoomType(
                RoomType.PreBoss
            );


        // Nunca permitimos outro Boss
        // ou PreBoss antes disso.
        for (
            int i = 0;
            i < preBossIndex;
            i++)
        {
            if (
                mainPath[i].RoomType ==
                RoomType.Boss ||
                mainPath[i].RoomType ==
                RoomType.PreBoss)
            {
                mainPath[i].SetRoomType(
                    RoomType.Exploration
                );
            }
        }
    }


    // =====================================================
    // SPECIAL ROOM SPACING
    // =====================================================

    private static void FixSpecialRoomSpacing(
        List<MapNode> mainPath)
    {
        int lastSpecialIndex =
            -999;

        for (
            int i = 1;
            i < mainPath.Count - 2;
            i++)
        {
            MapNode node =
                mainPath[i];


            if (!IsSpecialRoom(
                    node.RoomType))
            {
                continue;
            }


            int distance =
                i - lastSpecialIndex;


            if (
                distance <
                MinSpecialRoomDistance)
            {
                node.SetRoomType(
                    RoomType.Exploration
                );

                continue;
            }


            lastSpecialIndex = i;
        }
    }


    // =====================================================
    // BRANCH RULES
    // =====================================================

    private static void FixBranchRewards(
        MapGraph graph)
    {
        foreach (
            MapNode node
            in graph.Nodes)
        {
            if (
                node.PathType !=
                NodePathType.Branch)
            {
                continue;
            }


            // Branch intermediária não deve
            // virar Boss / PreBoss / Elite.
            if (
                node.RoomType ==
                RoomType.Boss ||
                node.RoomType ==
                RoomType.PreBoss ||
                node.RoomType ==
                RoomType.Elite)
            {
                node.SetRoomType(
                    RoomType.Exploration
                );
            }


            // Se for o fim de uma branch,
            // queremos algum motivo para
            // o jogador ter explorado até ali.
            if (IsBranchEnd(node))
            {
                EnsureUsefulBranchEnd(node);
            }
        }
    }


    private static void EnsureUsefulBranchEnd(
        MapNode node)
    {
        switch (node.RoomType)
        {
            case RoomType.Reward:
            case RoomType.Shop:
            case RoomType.Event:
            case RoomType.Secret:

                // Já é um final útil.
                return;
        }


        // Se chegou aqui com um tipo comum,
        // transformamos em Reward.
        node.SetRoomType(
            RoomType.Reward
        );
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private static bool IsSpecialRoom(
        RoomType type)
    {
        return
            type == RoomType.Elite ||
            type == RoomType.Rest ||
            type == RoomType.Shop ||
            type == RoomType.Reward;
    }


    private static bool IsBranchEnd(
        MapNode node)
    {
        return
            node.PathType ==
            NodePathType.Branch &&
            node.Connections.Count == 1;
    }


    private static List<MapNode>
        GetMainPath(
            MapGraph graph)
    {
        List<MapNode> mainPath =
            new List<MapNode>();


        foreach (
            MapNode node
            in graph.Nodes)
        {
            if (
                node.PathType ==
                NodePathType.MainPath)
            {
                mainPath.Add(node);
            }
        }


        mainPath.Sort(
            (a, b) =>
                a.Depth.CompareTo(
                    b.Depth
                )
        );


        return mainPath;
    }
}