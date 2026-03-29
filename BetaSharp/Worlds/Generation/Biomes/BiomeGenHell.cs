using BetaSharp.Entities;

namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeGenHell : Biome
{
    public BiomeGenHell()
    {
        MonsterList.Clear();
        CreatureList.Clear();
        WaterCreatureList.Clear();

        MonsterList.Add(new SpawnListEntry(w => new EntityGhast(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntityPigZombie(w)), 10);
        MonsterList.Add(new SpawnListEntry(w => new EntityBlaze(w), 1, 2), 5);
        MonsterList.Add(new SpawnListEntry(w => new EntityWitherSkeleton(w), 1, 2), 4);
        MonsterList.Add(new SpawnListEntry(w => new EntityMagmaCube(w), 2, 4), 4);
    }
}
