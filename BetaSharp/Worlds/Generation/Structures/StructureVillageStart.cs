using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class StructureVillageStart : StructureStart
{
    private readonly bool _hasMoreThanTwoComponents;

    public StructureVillageStart(IWorldContext world, JavaRandom random, int chunkX, int chunkZ, int terrainType)
    {
        List<StructureVillagePieceWeight> weightedPieces = StructureVillagePieces.GetStructureVillageWeightedPieceList(random, terrainType);
        ComponentVillageStartPiece startPiece = new(world.Dimension.BiomeSource, 0, random, (chunkX << 4) + 2, (chunkZ << 4) + 2, weightedPieces, terrainType);
        Components.Add(startPiece);
        startPiece.BuildComponent(startPiece, Components, random);

        while (startPiece.PendingRoads.Count > 0 || startPiece.PendingHouses.Count > 0)
        {
            StructureComponent next;
            if (startPiece.PendingRoads.Count > 0)
            {
                int index = random.NextInt(startPiece.PendingRoads.Count);
                next = startPiece.PendingRoads[index];
                startPiece.PendingRoads.RemoveAt(index);
            }
            else
            {
                int index = random.NextInt(startPiece.PendingHouses.Count);
                next = startPiece.PendingHouses[index];
                startPiece.PendingHouses.RemoveAt(index);
            }

            next.BuildComponent(startPiece, Components, random);
        }

        UpdateBoundingBox();

        int nonRoadCount = 0;
        for (int i = 0; i < Components.Count; ++i)
        {
            if (Components[i] is not ComponentVillageRoadPiece)
            {
                ++nonRoadCount;
            }
        }

        _hasMoreThanTwoComponents = nonRoadCount > 2;
    }

    public override bool IsSizeableStructure() => _hasMoreThanTwoComponents;
}
