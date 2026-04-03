using BetaSharp.Entities;

namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeGenHell : Biome
{
    public BiomeGenHell()
    {
        MonsterList.Clear();
        CreatureList.Clear();
        WaterCreatureList.Clear();

        MonsterList.Add(new SpawnListEntry(w => new EntityGhast(w), 4, 4), 50);
        MonsterList.Add(new SpawnListEntry(w => new EntityPigZombie(w), 4, 4), 100);
        MonsterList.Add(new SpawnListEntry(w => new EntityMagmaCube(w), 4, 4), 1);
    }
}
