using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

public static class StructureVillagePieces
{
    public static List<StructureVillagePieceWeight> GetStructureVillageWeightedPieceList(JavaRandom random, int terrainType)
    {
        List<StructureVillagePieceWeight> pieces =
        [
            new(typeof(ComponentVillageHouse4Garden), 4, NextIntInclusive(random, 2 + terrainType, 4 + terrainType * 2)),
            new(typeof(ComponentVillageChurch), 20, NextIntInclusive(random, terrainType, 1 + terrainType)),
            new(typeof(ComponentVillageHouse1), 20, NextIntInclusive(random, terrainType, 2 + terrainType)),
            new(typeof(ComponentVillageWoodHut), 3, NextIntInclusive(random, 2 + terrainType, 5 + terrainType * 3)),
            new(typeof(ComponentVillageHall), 15, NextIntInclusive(random, terrainType, 2 + terrainType)),
            new(typeof(ComponentVillageField), 3, NextIntInclusive(random, 1 + terrainType, 4 + terrainType)),
            new(typeof(ComponentVillageField2), 3, NextIntInclusive(random, 2 + terrainType, 4 + terrainType * 2)),
            new(typeof(ComponentVillageHouse2), 15, NextIntInclusive(random, 0, 1 + terrainType)),
            new(typeof(ComponentVillageHouse3), 8, NextIntInclusive(random, terrainType, 3 + terrainType * 2)),
        ];

        pieces.RemoveAll(piece => piece.VillagePiecesLimit == 0);
        return pieces;
    }

    private static int GetAvailablePieceWeight(List<StructureVillagePieceWeight> pieces)
    {
        bool hasAvailable = false;
        int totalWeight = 0;
        for (int i = 0; i < pieces.Count; ++i)
        {
            StructureVillagePieceWeight piece = pieces[i];
            if (piece.VillagePiecesLimit > 0 && piece.VillagePiecesSpawned < piece.VillagePiecesLimit)
            {
                hasAvailable = true;
            }

            totalWeight += piece.VillagePieceWeight;
        }

        return hasAvailable ? totalWeight : -1;
    }

    private static ComponentVillage? GetVillageComponentFromWeightedPiece(StructureVillagePieceWeight pieceWeight, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        if (pieceWeight.PieceType == typeof(ComponentVillageHouse4Garden))
        {
            return ComponentVillageHouse4Garden.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageChurch))
        {
            return ComponentVillageChurch.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageWoodHut))
        {
            return ComponentVillageWoodHut.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageHall))
        {
            return ComponentVillageHall.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageHouse1))
        {
            return ComponentVillageHouse1.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageField))
        {
            return ComponentVillageField.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageField2))
        {
            return ComponentVillageField2.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageHouse2))
        {
            return ComponentVillageHouse2.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        if (pieceWeight.PieceType == typeof(ComponentVillageHouse3))
        {
            return ComponentVillageHouse3.FindValidPlacement(components, random, x, y, z, facing, componentType);
        }

        return null;
    }

    private static ComponentVillage? GetNextVillageComponent(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        int totalWeight = GetAvailablePieceWeight(startPiece.StructureVillageWeightedPieceList);
        if (totalWeight <= 0)
        {
            return null;
        }

        for (int attempts = 0; attempts < 5; ++attempts)
        {
            int value = random.NextInt(totalWeight);
            for (int i = 0; i < startPiece.StructureVillageWeightedPieceList.Count; ++i)
            {
                StructureVillagePieceWeight pieceWeight = startPiece.StructureVillageWeightedPieceList[i];
                value -= pieceWeight.VillagePieceWeight;
                if (value >= 0)
                {
                    continue;
                }

                if (!pieceWeight.CanSpawnMoreVillagePiecesOfType(componentType) ||
                    pieceWeight == startPiece.CurrentPieceWeight && startPiece.StructureVillageWeightedPieceList.Count > 1)
                {
                    break;
                }

                ComponentVillage? component = GetVillageComponentFromWeightedPiece(pieceWeight, components, random, x, y, z, facing, componentType);
                if (component == null)
                {
                    break;
                }

                component.SetStartPiece(startPiece);
                pieceWeight.VillagePiecesSpawned++;
                startPiece.CurrentPieceWeight = pieceWeight;
                if (!pieceWeight.CanSpawnMoreVillagePieces())
                {
                    startPiece.StructureVillageWeightedPieceList.RemoveAt(i);
                }

                return component;
            }
        }

        StructureBoundingBox? torchBounds = ComponentVillageTorch.FindValidPlacement(components, random, x, y, z, facing);
        if (torchBounds == null)
        {
            return null;
        }

        ComponentVillageTorch torch = new(componentType, random, torchBounds, facing);
        torch.SetStartPiece(startPiece);
        return torch;
    }

    private static StructureComponent? GetNextVillageStructureComponent(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        if (componentType > 50)
        {
            return null;
        }

        if (Math.Abs(x - startPiece.GetBoundingBox().MinX) > 112 || Math.Abs(z - startPiece.GetBoundingBox().MinZ) > 112)
        {
            return null;
        }

        ComponentVillage? component = GetNextVillageComponent(startPiece, components, random, x, y, z, facing, componentType + 1);
        if (component == null)
        {
            return null;
        }

        StructureBoundingBox bounds = component.GetBoundingBox();
        int centerX = (bounds.MinX + bounds.MaxX) / 2;
        int centerZ = (bounds.MinZ + bounds.MaxZ) / 2;
        int size = Math.Max(bounds.GetXSize(), bounds.GetZSize());
        if (!startPiece.GetBiomeSource().AreBiomesViable(centerX, centerZ, size / 2 + 4, MapGenVillage.VillageSpawnBiomes))
        {
            return null;
        }

        components.Add(component);
        startPiece.PendingHouses.Add(component);
        return component;
    }

    private static StructureComponent? GetNextComponentVillagePath(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        if (componentType > 3 + startPiece.TerrainType)
        {
            return null;
        }

        if (Math.Abs(x - startPiece.GetBoundingBox().MinX) > 112 || Math.Abs(z - startPiece.GetBoundingBox().MinZ) > 112)
        {
            return null;
        }

        StructureBoundingBox? bounds = ComponentVillagePathGen.FindValidPlacement(startPiece, components, random, x, y, z, facing);
        if (bounds == null || bounds.MinY <= 10)
        {
            return null;
        }

        ComponentVillagePathGen path = new(componentType, random, bounds, facing);
        int centerX = (bounds.MinX + bounds.MaxX) / 2;
        int centerZ = (bounds.MinZ + bounds.MaxZ) / 2;
        int size = Math.Max(bounds.GetXSize(), bounds.GetZSize());
        if (!startPiece.GetBiomeSource().AreBiomesViable(centerX, centerZ, size / 2 + 4, MapGenVillage.VillageSpawnBiomes))
        {
            return null;
        }

        components.Add(path);
        startPiece.PendingRoads.Add(path);
        return path;
    }

    public static StructureComponent? GetNextStructureComponent(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        => GetNextVillageStructureComponent(startPiece, components, random, x, y, z, facing, componentType);

    public static StructureComponent? GetNextStructureComponentVillagePath(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
        => GetNextComponentVillagePath(startPiece, components, random, x, y, z, facing, componentType);

    private static int NextIntInclusive(JavaRandom random, int min, int max)
    {
        return random.NextInt(max - min + 1) + min;
    }
}
