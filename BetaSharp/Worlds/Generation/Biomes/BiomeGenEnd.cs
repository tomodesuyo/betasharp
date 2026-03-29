using BetaSharp.Blocks;
using BetaSharp.Entities;

namespace BetaSharp.Worlds.Generation.Biomes;

internal sealed class BiomeGenEnd : Biome
{
    public BiomeGenEnd()
    {
        MonsterList.Clear();
        CreatureList.Clear();
        WaterCreatureList.Clear();
        MonsterList.Add(new SpawnListEntry(w => new EntityEnderman(w), 4, 4), 10);
        TopBlockId = (byte)Block.EndStone.id;
        SoilBlockId = (byte)Block.EndStone.id;
        SetBiomeDecorator(new EndBiomeDecorator());
    }

    public override int GetSkyColorByTemp(float rand) => 0;
}
