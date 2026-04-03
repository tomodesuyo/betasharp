using BetaSharp;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class MapGenNetherFortress : MapGenStructure
{
    private const int RegionSize = 16;
    private readonly WeightedRandomSelector<SpawnListEntry> _spawnList = new();

    public MapGenNetherFortress()
    {
        _spawnList.Add(new SpawnListEntry(w => new EntityBlaze(w), 2, 3), 10);
        _spawnList.Add(new SpawnListEntry(w => new EntityPigZombie(w), 4, 4), 5);
        _spawnList.Add(new SpawnListEntry(w => new EntityWitherSkeleton(w), 4, 4), 10);
        _spawnList.Add(new SpawnListEntry(w => new EntityMagmaCube(w), 4, 4), 3);
    }

    public WeightedRandomSelector<SpawnListEntry> GetSpawnList()
    {
        return _spawnList;
    }

    protected override bool CanSpawnStructureAtCoords(int chunkX, int chunkZ)
    {
        int regionX = chunkX >> 4;
        int regionZ = chunkZ >> 4;
        JavaRandom random = new(((long)(regionX ^ (regionZ << 4))) ^ World.Seed);
        random.NextInt();
        if (random.NextInt(3) != 0)
        {
            return false;
        }

        return chunkX == (regionX << 4) + 4 + random.NextInt(8)
            && chunkZ == (regionZ << 4) + 4 + random.NextInt(8);
    }

    protected override StructureStart GetStructureStart(int chunkX, int chunkZ)
    {
        return new StructureNetherFortressStart(World, Random, chunkX, chunkZ);
    }

    public override Vec3i? FindNearestStructure(IWorldContext world, int x, int y, int z)
    {
        World = world;
        Random.SetSeed(world.Seed);
        long seedX = Random.NextLong();
        long seedZ = Random.NextLong();

        int originChunkX = x >> 4;
        int originChunkZ = z >> 4;
        int originRegionX = originChunkX >> 4;
        int originRegionZ = originChunkZ >> 4;
        double bestDistance = double.MaxValue;
        Vec3i? bestPos = null;

        for (int ring = 0; ring <= 128; ++ring)
        {
            for (int regionX = originRegionX - ring; regionX <= originRegionX + ring; ++regionX)
            {
                for (int regionZ = originRegionZ - ring; regionZ <= originRegionZ + ring; ++regionZ)
                {
                    if (ring > 0
                        && regionX != originRegionX - ring
                        && regionX != originRegionX + ring
                        && regionZ != originRegionZ - ring
                        && regionZ != originRegionZ + ring)
                    {
                        continue;
                    }

                    if (!TryGetStructureChunk(regionX, regionZ, out int candidateChunkX, out int candidateChunkZ))
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
                    Vec3i pos = new(box.MinX + box.GetXSize() / 2, box.MinY, box.MinZ + box.GetZSize() / 2);
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
                double nextRingMinDistance = Math.Max(0, ring * RegionSize * 16 - 16);
                if (nextRingMinDistance * nextRingMinDistance > bestDistance)
                {
                    break;
                }
            }
        }

        return bestPos;
    }

    private bool TryGetStructureChunk(int regionX, int regionZ, out int chunkX, out int chunkZ)
    {
        JavaRandom random = new(((long)(regionX ^ (regionZ << 4))) ^ World.Seed);
        random.NextInt();
        if (random.NextInt(3) != 0)
        {
            chunkX = 0;
            chunkZ = 0;
            return false;
        }

        chunkX = (regionX << 4) + 4 + random.NextInt(8);
        chunkZ = (regionZ << 4) + 4 + random.NextInt(8);
        return true;
    }

    public bool IsFortressMonsterSpawnArea(IWorldContext world, int x, int y, int z)
    {
        World = world;
        return HasStructureAt(x, y, z)
               || (world.Reader.GetBlockId(x, y - 1, z) == BetaSharp.Blocks.Block.NetherBrick.id && HasStructureAt(x, y - 1, z));
    }
}
