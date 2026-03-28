using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal class SandPatchFeature : Feature
{
    private readonly int _radius;
    private readonly int _blockId;

    public SandPatchFeature(int radius, int blockId)
    {
        _radius = radius;
        _blockId = blockId;
    }

    public override bool Generate(IWorldContext level, JavaRandom rand, int x, int y, int z)
    {
        if (level.Reader.GetMaterial(x, y, z) != Material.Water)
        {
            return false;
        }

        int radius = rand.NextInt(_radius - 2) + 2;
        const int halfHeight = 2;

        for (int blockX = x - radius; blockX <= x + radius; ++blockX)
        {
            for (int blockZ = z - radius; blockZ <= z + radius; ++blockZ)
            {
                int dx = blockX - x;
                int dz = blockZ - z;
                if (dx * dx + dz * dz > radius * radius)
                {
                    continue;
                }

                for (int blockY = y - halfHeight; blockY <= y + halfHeight; ++blockY)
                {
                    int blockId = level.Reader.GetBlockId(blockX, blockY, blockZ);
                    if (blockId == Block.Dirt.id || blockId == Block.GrassBlock.id)
                    {
                        level.Writer.SetBlockWithoutNotifyingNeighbors(blockX, blockY, blockZ, _blockId, 0, false);
                    }
                }
            }
        }

        return true;
    }
}
