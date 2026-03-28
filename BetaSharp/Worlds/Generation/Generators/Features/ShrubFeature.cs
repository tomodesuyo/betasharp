using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class ShrubFeature(int logMeta, int leavesMeta) : Feature
{
    private readonly int _logMeta = logMeta;
    private readonly int _leavesMeta = leavesMeta;

    public override bool Generate(IWorldContext world, JavaRandom rand, int x, int y, int z)
    {
        while (true)
        {
            int blockId = world.Reader.GetBlockId(x, y, z);
            if ((blockId != 0 && blockId != Block.Leaves.id) || y <= 0)
            {
                int soilId = world.Reader.GetBlockId(x, y, z);
                if (soilId == Block.Dirt.id || soilId == Block.GrassBlock.id)
                {
                    ++y;
                    world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Log.id, _logMeta, false);

                    for (int canopyY = y; canopyY <= y + 2; ++canopyY)
                    {
                        int yOffset = canopyY - y;
                        int radius = 2 - yOffset;

                        for (int canopyX = x - radius; canopyX <= x + radius; ++canopyX)
                        {
                            int xOffset = canopyX - x;

                            for (int canopyZ = z - radius; canopyZ <= z + radius; ++canopyZ)
                            {
                                int zOffset = canopyZ - z;
                                if ((Math.Abs(xOffset) != radius || Math.Abs(zOffset) != radius || rand.NextInt(2) != 0)
                                    && !Block.BlocksOpaque[world.Reader.GetBlockId(canopyX, canopyY, canopyZ)])
                                {
                                    world.Writer.SetBlockWithoutNotifyingNeighbors(canopyX, canopyY, canopyZ, Block.Leaves.id, _leavesMeta, false);
                                }
                            }
                        }
                    }
                }

                return true;
            }

            --y;
        }
    }
}
