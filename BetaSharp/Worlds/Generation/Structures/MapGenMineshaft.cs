using System;
using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class MapGenMineshaft : MapGenStructure
{
    protected override bool CanSpawnStructureAtCoords(int chunkX, int chunkZ)
    {
        return Random.NextInt(100) == 0 && Random.NextInt(80) < Math.Max(Math.Abs(chunkX), Math.Abs(chunkZ));
    }

    protected override StructureStart GetStructureStart(int chunkX, int chunkZ)
    {
        return new StructureMineshaftStart(World, new JavaRandom((chunkX * 341873128712L + chunkZ * 132897987541L) ^ World.Seed), chunkX, chunkZ);
    }
}
