using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class MapGenStronghold : MapGenStructure
{
    private static readonly ICollection<Biome> s_allowedBiomes =
    [
        Biome.Desert,
        Biome.Forest,
        Biome.ExtremeHills,
        Biome.Swampland,
        Biome.Taiga,
        Biome.IcePlains,
        Biome.IceMountains,
        Biome.DesertHills,
        Biome.ForestHills,
        Biome.ExtremeHillsEdge,
    ];

    private bool _ranBiomeCheck;
    private readonly ChunkPos?[] _structureCoords = new ChunkPos?[3];

    protected override bool CanSpawnStructureAtCoords(int chunkX, int chunkZ)
    {
        if (!_ranBiomeCheck)
        {
            JavaRandom random = new();
            random.SetSeed(World.Seed);
            double angle = random.NextDouble() * Math.PI * 2.0D;

            for (int i = 0; i < _structureCoords.Length; ++i)
            {
                double distance = (1.25D + random.NextDouble()) * 32.0D;
                int candidateChunkX = (int)System.Math.Round(System.Math.Cos(angle) * distance);
                int candidateChunkZ = (int)System.Math.Round(System.Math.Sin(angle) * distance);
                Vec3i? biomePos = World.Dimension.BiomeSource.FindBiomePosition((candidateChunkX << 4) + 8, (candidateChunkZ << 4) + 8, 112, s_allowedBiomes, random);
                if (biomePos != null)
                {
                    candidateChunkX = biomePos.Value.X >> 4;
                    candidateChunkZ = biomePos.Value.Z >> 4;
                }

                _structureCoords[i] = new ChunkPos(candidateChunkX, candidateChunkZ);
                angle += Math.PI * 2.0D / _structureCoords.Length;
            }

            _ranBiomeCheck = true;
        }

        for (int i = 0; i < _structureCoords.Length; ++i)
        {
            ChunkPos? pos = _structureCoords[i];
            if (pos != null && pos.Value.X == chunkX && pos.Value.Z == chunkZ)
            {
                return true;
            }
        }

        return false;
    }

    protected override List<Vec3i>? GetCoordList()
    {
        List<Vec3i> positions = [];
        for (int i = 0; i < _structureCoords.Length; ++i)
        {
            ChunkPos? chunkPos = _structureCoords[i];
            if (chunkPos != null)
            {
                positions.Add(new Vec3i((chunkPos.Value.X << 4) + 8, 64, (chunkPos.Value.Z << 4) + 8));
            }
        }

        return positions;
    }

    protected override StructureStart GetStructureStart(int chunkX, int chunkZ)
    {
        return new EmptyStructureStart();
    }
}
