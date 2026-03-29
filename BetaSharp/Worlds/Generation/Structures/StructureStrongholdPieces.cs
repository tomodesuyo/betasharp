using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

internal static class StructureStrongholdPieces
{
    private static readonly StructureStrongholdPieceWeight[] s_pieceWeights =
    [
        new(typeof(ComponentStrongholdStraight), 40, 0),
        new(typeof(ComponentStrongholdPrison), 5, 5),
        new(typeof(ComponentStrongholdLeftTurn), 20, 0),
        new(typeof(ComponentStrongholdRightTurn), 20, 0),
        new(typeof(ComponentStrongholdRoomCrossing), 10, 6),
        new(typeof(ComponentStrongholdStairsStraight), 5, 5),
        new(typeof(ComponentStrongholdStairs), 5, 5),
        new(typeof(ComponentStrongholdCrossing), 5, 4),
        new(typeof(ComponentStrongholdChestCorridor), 5, 4),
        new StructureStrongholdPieceWeight2(typeof(ComponentStrongholdLibrary), 10, 2),
        new StructureStrongholdPieceWeight3(typeof(ComponentStrongholdPortalRoom), 20, 1),
    ];

    private static readonly StructureStrongholdStones s_strongholdStones = new();
    [ThreadStatic] private static List<StructureStrongholdPieceWeight>? s_structurePieceList;
    [ThreadStatic] private static Type? s_forcedComponentType;
    [ThreadStatic] private static int s_totalWeight;

    public static void PrepareStructurePieces()
    {
        s_structurePieceList = [];
        for (int i = 0; i < s_pieceWeights.Length; ++i)
        {
            StructureStrongholdPieceWeight piece = ClonePieceWeight(s_pieceWeights[i]);
            s_structurePieceList.Add(piece);
        }

        s_forcedComponentType = null;
    }

    public static StructurePieceBlockSelector GetStrongholdStones() => s_strongholdStones;

    public static void SetComponentType(Type componentType)
    {
        s_forcedComponentType = componentType;
    }

    private static StructureStrongholdPieceWeight ClonePieceWeight(StructureStrongholdPieceWeight piece)
    {
        return piece switch
        {
            StructureStrongholdPieceWeight2 => new StructureStrongholdPieceWeight2(piece.PieceType, piece.PieceWeight, piece.InstancesLimit),
            StructureStrongholdPieceWeight3 => new StructureStrongholdPieceWeight3(piece.PieceType, piece.PieceWeight, piece.InstancesLimit),
            _ => new StructureStrongholdPieceWeight(piece.PieceType, piece.PieceWeight, piece.InstancesLimit),
        };
    }

    private static bool CanAddStructurePieces()
    {
        s_structurePieceList ??= [];
        bool hasAvailable = false;
        s_totalWeight = 0;

        for (int i = 0; i < s_structurePieceList.Count; ++i)
        {
            StructureStrongholdPieceWeight piece = s_structurePieceList[i];
            if (piece.InstancesLimit > 0 && piece.InstancesSpawned < piece.InstancesLimit)
            {
                hasAvailable = true;
            }

            s_totalWeight += piece.PieceWeight;
        }

        return hasAvailable;
    }

    private static ComponentStronghold? GetStrongholdComponentFromWeightedPiece(Type pieceType, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        if (pieceType == typeof(ComponentStrongholdStraight))
        {
            return ComponentStrongholdStraight.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdPrison))
        {
            return ComponentStrongholdPrison.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdLeftTurn))
        {
            return ComponentStrongholdLeftTurn.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdRightTurn))
        {
            return ComponentStrongholdRightTurn.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdRoomCrossing))
        {
            return ComponentStrongholdRoomCrossing.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdStairsStraight))
        {
            return ComponentStrongholdStairsStraight.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdStairs))
        {
            return ComponentStrongholdStairs.GetStrongholdStairsComponent(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdCrossing))
        {
            return ComponentStrongholdCrossing.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdChestCorridor))
        {
            return ComponentStrongholdChestCorridor.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdLibrary))
        {
            return ComponentStrongholdLibrary.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceType == typeof(ComponentStrongholdPortalRoom))
        {
            return ComponentStrongholdPortalRoom.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        return null;
    }

    private static ComponentStronghold? GetNextComponent(ComponentStrongholdStairs2 startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        s_structurePieceList ??= [];
        if (!CanAddStructurePieces())
        {
            return null;
        }

        if (s_forcedComponentType != null)
        {
            ComponentStronghold? forcedComponent = GetStrongholdComponentFromWeightedPiece(s_forcedComponentType, components, random, x, y, z, facing, componentType);
            s_forcedComponentType = null;
            if (forcedComponent != null)
            {
                return forcedComponent;
            }
        }

        for (int attempt = 0; attempt < 5; ++attempt)
        {
            int weight = random.NextInt(s_totalWeight);
            for (int i = 0; i < s_structurePieceList.Count; ++i)
            {
                StructureStrongholdPieceWeight pieceWeight = s_structurePieceList[i];
                weight -= pieceWeight.PieceWeight;
                if (weight >= 0)
                {
                    continue;
                }

                if (!pieceWeight.CanSpawnMoreStructuresOfType(componentType) || pieceWeight == startPiece.CurrentPieceWeight)
                {
                    break;
                }

                ComponentStronghold? component = GetStrongholdComponentFromWeightedPiece(pieceWeight.PieceType, components, random, x, y, z, facing, componentType);
                if (component == null)
                {
                    break;
                }

                pieceWeight.InstancesSpawned++;
                startPiece.CurrentPieceWeight = pieceWeight;
                if (!pieceWeight.CanSpawnMoreStructures())
                {
                    s_structurePieceList.RemoveAt(i);
                }

                return component;
            }
        }

        StructureBoundingBox? corridorBounds = ComponentStrongholdCorridor.FindValidPlacement(components, random, x, y, z, facing);
        if (corridorBounds != null && corridorBounds.MinY > 1)
        {
            return new ComponentStrongholdCorridor(componentType, random, corridorBounds, facing);
        }

        return null;
    }

    private static StructureComponent? GetNextValidComponent(ComponentStrongholdStairs2 startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        if (componentType > 50)
        {
            return null;
        }

        if (Math.Abs(x - startPiece.GetBoundingBox().MinX) > 112 || Math.Abs(z - startPiece.GetBoundingBox().MinZ) > 112)
        {
            return null;
        }

        ComponentStronghold? component = GetNextComponent(startPiece, components, random, x, y, z, facing, componentType + 1);
        if (component != null)
        {
            components.Add(component);
            startPiece.PendingComponents.Add(component);
        }

        return component;
    }

    public static StructureComponent? GetNextValidComponentAccess(ComponentStrongholdStairs2 startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        => GetNextValidComponent(startPiece, components, random, x, y, z, facing, componentType);
}
