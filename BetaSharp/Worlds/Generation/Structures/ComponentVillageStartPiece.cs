using BetaSharp.Worlds.Biomes.Source;
using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageStartPiece : ComponentVillageWell
{
    public readonly List<StructureComponent> PendingHouses = [];
    public readonly List<StructureComponent> PendingRoads = [];
    public readonly List<StructureVillagePieceWeight> StructureVillageWeightedPieceList;
    public readonly int TerrainType;
    public StructureVillagePieceWeight? CurrentPieceWeight;
    private readonly BiomeSource _biomeSource;

    public ComponentVillageStartPiece(BiomeSource biomeSource, int componentType, JavaRandom random, int x, int z, List<StructureVillagePieceWeight> weightedPieces, int terrainType)
        : base(componentType, random, x, z)
    {
        _biomeSource = biomeSource;
        StructureVillageWeightedPieceList = weightedPieces;
        TerrainType = terrainType;
    }

    public BiomeSource GetBiomeSource() => _biomeSource;
}
