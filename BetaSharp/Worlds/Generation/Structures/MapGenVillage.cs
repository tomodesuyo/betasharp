using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Biomes;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class MapGenVillage(int size) : MapGenStructure
{
    private const int Spacing = 32;
    private const int Separation = 8;

    public static readonly ICollection<Biome> VillageSpawnBiomes =
    [
        Biome.Plains,
        Biome.Desert,
        Biome.Savanna,
    ];

    private readonly int _size = size;

    protected override bool CanSpawnStructureAtCoords(int chunkX, int chunkZ)
    {
        int originalChunkX = chunkX;
        int originalChunkZ = chunkZ;

        if (chunkX < 0)
        {
            chunkX -= Spacing - 1;
        }

        if (chunkZ < 0)
        {
            chunkZ -= Spacing - 1;
        }

        int regionX = chunkX / Spacing;
        int regionZ = chunkZ / Spacing;
        JavaRandom random = SetStructureSeed(regionX, regionZ, 10387312);
        regionX *= Spacing;
        regionZ *= Spacing;
        regionX += random.NextInt(Spacing - Separation);
        regionZ += random.NextInt(Spacing - Separation);

        return originalChunkX == regionX &&
               originalChunkZ == regionZ &&
               World.Dimension.BiomeSource.AreBiomesViable(originalChunkX * 16 + 8, originalChunkZ * 16 + 8, 0, VillageSpawnBiomes);
    }

    public override Vec3i? FindNearestStructure(IWorldContext world, int x, int y, int z)
    {
        World = world;
        Random.SetSeed(world.Seed);
        long seedX = Random.NextLong();
        long seedZ = Random.NextLong();

        int originChunkX = x >> 4;
        int originChunkZ = z >> 4;
        int originRegionX = FloorDiv(originChunkX, Spacing);
        int originRegionZ = FloorDiv(originChunkZ, Spacing);
        double bestDistance = double.MaxValue;
        Vec3i? bestPos = null;

        for (int ring = 0; ring <= 128; ++ring)
        {
            for (int regionX = originRegionX - ring; regionX <= originRegionX + ring; ++regionX)
            {
                for (int regionZ = originRegionZ - ring; regionZ <= originRegionZ + ring; ++regionZ)
                {
                    if (ring > 0 &&
                        regionX != originRegionX - ring &&
                        regionX != originRegionX + ring &&
                        regionZ != originRegionZ - ring &&
                        regionZ != originRegionZ + ring)
                    {
                        continue;
                    }

                    if (!TryGetVillageChunk(regionX, regionZ, out int candidateChunkX, out int candidateChunkZ))
                    {
                        continue;
                    }

                    EnsureStructureStart(world, candidateChunkX, candidateChunkZ, seedX, seedZ);

                    long key = (uint)candidateChunkX | ((long)(uint)candidateChunkZ << 32);
                    if (!StructureMap.TryGetValue(key, out StructureStart? start) || !start.IsSizeableStructure() || start.GetComponents().Count == 0)
                    {
                        continue;
                    }

                    StructureBoundingBox box = start.GetComponents()[0].GetBoundingBox();
                    Vec3i pos = new(box.MinX + (box.MaxX - box.MinX) / 2, box.MinY, box.MinZ + (box.MaxZ - box.MinZ) / 2);
                    int dx = pos.X - x;
                    int dy = pos.Y - y;
                    int dz = pos.Z - z;
                    double distance = dx * dx + dy * dy + dz * dz;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPos = pos;
                    }
                }
            }

            if (bestPos != null)
            {
                double nextRingMinDistance = Math.Max(0, ring * Spacing * 16 - 16);
                if (nextRingMinDistance * nextRingMinDistance > bestDistance)
                {
                    break;
                }
            }
        }

        return bestPos;
    }

    protected override StructureStart GetStructureStart(int chunkX, int chunkZ)
    {
        return new StructureVillageStart(World, Random, chunkX, chunkZ, _size);
    }

    private bool TryGetVillageChunk(int regionX, int regionZ, out int chunkX, out int chunkZ)
    {
        JavaRandom random = SetStructureSeed(regionX, regionZ, 10387312);
        chunkX = regionX * Spacing + random.NextInt(Spacing - Separation);
        chunkZ = regionZ * Spacing + random.NextInt(Spacing - Separation);
        return World.Dimension.BiomeSource.AreBiomesViable(chunkX * 16 + 8, chunkZ * 16 + 8, 0, VillageSpawnBiomes);
    }

    private static int FloorDiv(int value, int divisor)
    {
        if (value >= 0)
        {
            return value / divisor;
        }

        return -((-value + divisor - 1) / divisor);
    }
}
