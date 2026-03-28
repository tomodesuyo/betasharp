using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Generators.Features;

namespace BetaSharp.Worlds.Generation.Biomes;

internal sealed class BiomeGenJungle : Biome
{
    public BiomeGenJungle()
    {
        SetDecorator(treesPerChunk: 50, flowersPerChunk: 4, grassPerChunk: 25);
    }

    public override Feature GetRandomWorldGenForTrees(JavaRandom rand)
    {
        if (rand.NextInt(10) == 0)
        {
            return new LargeOakTreeFeature();
        }

        if (rand.NextInt(2) == 0)
        {
            return new ShrubFeature(3, 0);
        }

        return rand.NextInt(3) == 0
            ? new HugeJungleTreeFeature(10 + rand.NextInt(20), 3, 3)
            : new JungleTreeFeature(4 + rand.NextInt(7), 3, 3, true);
    }

    public override Feature GetRandomWorldGenForGrass(JavaRandom rand)
    {
        return rand.NextInt(4) == 0
            ? new GrassPatchFeature(Block.Grass.id, 2)
            : new GrassPatchFeature(Block.Grass.id, 1);
    }

    public override void Decorate(IWorldContext world, JavaRandom rand, int blockX, int blockZ)
    {
        base.Decorate(world, rand, blockX, blockZ);
        Feature vines = new VineFeature();

        for (int i = 0; i < 50; ++i)
        {
            int x = blockX + rand.NextInt(16) + 8;
            int z = blockZ + rand.NextInt(16) + 8;
            vines.Generate(world, rand, x, 64, z);
        }
    }
}
