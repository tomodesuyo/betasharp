using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class WaterLilyFeature : Feature
{
    public override bool Generate(IWorldContext world, JavaRandom rand, int x, int y, int z)
    {
        for (int i = 0; i < 10; ++i)
        {
            int placeX = x + rand.NextInt(8) - rand.NextInt(8);
            int placeY = y + rand.NextInt(4) - rand.NextInt(4);
            int placeZ = z + rand.NextInt(8) - rand.NextInt(8);
            if (world.Reader.IsAir(placeX, placeY, placeZ) && Block.LilyPad.canPlaceAt(new CanPlaceAtContext(world, 1, placeX, placeY, placeZ)))
            {
                world.Writer.SetBlockWithoutNotifyingNeighbors(placeX, placeY, placeZ, Block.LilyPad.id, 0, false);
            }
        }

        return true;
    }
}
