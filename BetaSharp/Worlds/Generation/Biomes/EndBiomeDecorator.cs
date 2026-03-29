using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Generators.Features;
using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Biomes;

internal sealed class EndBiomeDecorator : BiomeDecorator
{
    private readonly EndSpikeFeature _spikeFeature = new();

    public override void Decorate(IWorldContext world, JavaRandom random, Biome biome, int blockX, int blockZ)
    {
        if (random.NextInt(5) != 0)
        {
            return;
        }

        int x = blockX + random.NextInt(16) + 8;
        int z = blockZ + random.NextInt(16) + 8;
        int y = world.Reader.GetTopSolidBlockY(x, z);
        if (y > 0)
        {
            _spikeFeature.Generate(world, random, x, y, z);
        }
    }
}
