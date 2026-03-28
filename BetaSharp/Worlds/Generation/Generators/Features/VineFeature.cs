using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class VineFeature : Feature
{
    public override bool Generate(IWorldContext world, JavaRandom rand, int x, int y, int z)
    {
        int startX = x;
        int startZ = z;

        while (y < 128)
        {
            if (world.Reader.IsAir(x, y, z))
            {
                for (int side = 2; side <= 5; ++side)
                {
                    if (Block.Vine.canPlaceAt(new CanPlaceAtContext(world, side, x, y, z)))
                    {
                        int meta = side switch
                        {
                            2 => 1,
                            3 => 4,
                            4 => 8,
                            5 => 2,
                            _ => 0
                        };
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Vine.id, meta, false);
                        break;
                    }
                }
            }
            else
            {
                x = startX + rand.NextInt(4) - rand.NextInt(4);
                z = startZ + rand.NextInt(4) - rand.NextInt(4);
            }

            ++y;
        }

        return true;
    }
}
