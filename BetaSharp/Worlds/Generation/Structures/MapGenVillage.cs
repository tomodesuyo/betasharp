using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class MapGenVillage(int size) : MapGenStructure
{
    public static readonly ICollection<Biome> VillageSpawnBiomes =
    [
        Biome.Plains,
        Biome.Desert,
    ];

    private readonly int _size = size;

    protected override bool CanSpawnStructureAtCoords(int chunkX, int chunkZ)
    {
        const int spacing = 32;
        const int separation = 8;
        int originalChunkX = chunkX;
        int originalChunkZ = chunkZ;

        if (chunkX < 0)
        {
            chunkX -= spacing - 1;
        }

        if (chunkZ < 0)
        {
            chunkZ -= spacing - 1;
        }

        int regionX = chunkX / spacing;
        int regionZ = chunkZ / spacing;
        JavaRandom random = SetStructureSeed(regionX, regionZ, 10387312);
        regionX *= spacing;
        regionZ *= spacing;
        regionX += random.NextInt(spacing - separation);
        regionZ += random.NextInt(spacing - separation);

        return originalChunkX == regionX &&
               originalChunkZ == regionZ &&
               World.Dimension.BiomeSource.AreBiomesViable(originalChunkX * 16 + 8, originalChunkZ * 16 + 8, 0, VillageSpawnBiomes);
    }

    protected override StructureStart GetStructureStart(int chunkX, int chunkZ)
    {
        return new StructureVillageStart(World, Random, chunkX, chunkZ, _size);
    }
}
