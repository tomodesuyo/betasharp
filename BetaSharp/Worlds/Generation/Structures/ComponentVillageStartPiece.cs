using BetaSharp.Worlds.Biomes.Source;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageStartPiece : ComponentVillageWell
{
    public readonly List<StructureComponent> PendingHouses = [];
    public readonly List<StructureComponent> PendingRoads = [];
    public readonly List<StructureVillagePieceWeight> StructureVillageWeightedPieceList;
    public readonly int TerrainType;
    public readonly bool InDesert;
    public readonly Biome StartBiome;
    public StructureVillagePieceWeight? CurrentPieceWeight;
    private readonly BiomeSource _biomeSource;

    public ComponentVillageStartPiece(BiomeSource biomeSource, int componentType, JavaRandom random, int x, int z, List<StructureVillagePieceWeight> weightedPieces, int terrainType)
        : base(componentType, random, x, z)
    {
        _biomeSource = biomeSource;
        StructureVillageWeightedPieceList = weightedPieces;
        TerrainType = terrainType;
        StartBiome = biomeSource.GetBiome(x, z);
        InDesert = StartBiome == Biome.Desert || StartBiome == Biome.DesertHills;
        SetStartPiece(this);
    }

    public BiomeSource GetBiomeSource() => _biomeSource;
}
