using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Generation.Generators.Features;

namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeGenForest : Biome
{
    public BiomeGenForest()
    {
        CreatureList.Add(new SpawnListEntry(w => new EntityWolf(w)), 5);
        SetDecorator(treesPerChunk: 10, grassPerChunk: 2);
    }

    public override Feature GetRandomWorldGenForTrees(JavaRandom rand) =>
        rand.NextInt(5) == 0 ? new ForestTreeFeature() :
        rand.NextInt(10) == 0 ? new LargeOakTreeFeature() :
        new OakTreeFeature();
}
