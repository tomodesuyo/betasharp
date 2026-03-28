using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Chunks;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public abstract class MapGenStructure
{
    protected readonly Dictionary<long, StructureStart> StructureMap = [];
    protected readonly JavaRandom Random = new();
    protected IWorldContext World = null!;

    public void Generate(IChunkSource source, IWorldContext world, int chunkX, int chunkZ, byte[] blocks)
    {
        World = world;
        Random.SetSeed(world.Seed);
        long seedX = Random.NextLong();
        long seedZ = Random.NextLong();

        const int range = 8;
        for (int scanX = chunkX - range; scanX <= chunkX + range; ++scanX)
        {
            for (int scanZ = chunkZ - range; scanZ <= chunkZ + range; ++scanZ)
            {
                Random.SetSeed((long)scanX * seedX ^ (long)scanZ * seedZ ^ world.Seed);
                RecursiveGenerate(world, scanX, scanZ);
            }
        }
    }

    protected void RecursiveGenerate(IWorldContext world, int chunkX, int chunkZ)
    {
        long key = (uint)chunkX | ((long)(uint)chunkZ << 32);
        if (StructureMap.ContainsKey(key))
        {
            return;
        }

        Random.NextInt();
        if (CanSpawnStructureAtCoords(chunkX, chunkZ))
        {
            StructureMap[key] = GetStructureStart(chunkX, chunkZ);
        }
    }

    public bool GenerateStructuresInChunk(IWorldContext world, JavaRandom random, int chunkX, int chunkZ)
    {
        int minX = (chunkX << 4) + 8;
        int minZ = (chunkZ << 4) + 8;
        bool generated = false;

        foreach ((_, StructureStart start) in StructureMap)
        {
            if (start.IsSizeableStructure() && start.GetBoundingBox().Intersects(minX, minZ, minX + 15, minZ + 15))
            {
                start.GenerateStructure(world, random, new StructureBoundingBox(minX, minZ, minX + 15, minZ + 15));
                generated = true;
            }
        }

        return generated;
    }

    public bool HasStructureAt(int x, int y, int z)
    {
        foreach ((_, StructureStart start) in StructureMap)
        {
            if (!start.IsSizeableStructure() || !start.GetBoundingBox().Contains(x, y, z))
            {
                continue;
            }

            List<StructureComponent> components = start.GetComponents();
            for (int i = 0; i < components.Count; ++i)
            {
                if (components[i].GetBoundingBox().Contains(x, y, z))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vec3i? FindNearestStructure(IWorldContext world, int x, int y, int z)
    {
        World = world;
        Random.SetSeed(world.Seed);
        long seedX = Random.NextLong();
        long seedZ = Random.NextLong();
        long mixedX = (x >> 4) * seedX;
        long mixedZ = (z >> 4) * seedZ;
        Random.SetSeed(mixedX ^ mixedZ ^ world.Seed);
        RecursiveGenerate(world, x >> 4, z >> 4);

        List<Vec3i>? coords = GetCoordList();
        if (coords != null)
        {
            for (int i = 0; i < coords.Count; ++i)
            {
                Vec3i pos = coords[i];
                RecursiveGenerate(world, pos.X >> 4, pos.Z >> 4);
            }
        }

        double bestDistance = double.MaxValue;
        Vec3i? bestPos = null;

        foreach ((_, StructureStart start) in StructureMap)
        {
            if (!start.IsSizeableStructure())
            {
                continue;
            }

            List<StructureComponent> components = start.GetComponents();
            if (components.Count == 0)
            {
                continue;
            }

            StructureBoundingBox box = components[0].GetBoundingBox();
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

        if (bestPos != null)
        {
            return bestPos;
        }

        if (coords == null)
        {
            return null;
        }

        Vec3i? nearest = null;
        foreach (Vec3i pos in coords)
        {
            int dx = pos.X - x;
            int dy = pos.Y - y;
            int dz = pos.Z - z;
            double distance = dx * dx + dy * dy + dz * dz;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = pos;
            }
        }

        return nearest;
    }

    protected JavaRandom SetStructureSeed(int regionX, int regionZ, long salt)
    {
        JavaRandom random = new();
        random.SetSeed(regionX * 341873128712L + regionZ * 132897987541L + World.Seed + salt);
        return random;
    }

    protected abstract bool CanSpawnStructureAtCoords(int chunkX, int chunkZ);
    protected abstract StructureStart GetStructureStart(int chunkX, int chunkZ);
    protected virtual List<Vec3i>? GetCoordList() => null;
}
