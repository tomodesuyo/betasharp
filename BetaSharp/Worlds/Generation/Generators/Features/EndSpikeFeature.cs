using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class EndSpikeFeature : Feature
{
    public override bool Generate(IWorldContext level, JavaRandom rand, int x, int y, int z)
    {
        if (level.Reader.GetBlockId(x, y, z) != 0 || level.Reader.GetBlockId(x, y - 1, z) != Block.EndStone.id)
        {
            return false;
        }

        int height = rand.NextInt(32) + 6;
        int radius = rand.NextInt(4) + 1;

        for (int checkX = x - radius; checkX <= x + radius; ++checkX)
        {
            for (int checkZ = z - radius; checkZ <= z + radius; ++checkZ)
            {
                int dx = checkX - x;
                int dz = checkZ - z;
                if (dx * dx + dz * dz <= radius * radius + 1 &&
                    level.Reader.GetBlockId(checkX, y - 1, checkZ) != Block.EndStone.id)
                {
                    return false;
                }
            }
        }

        for (int spikeY = y; spikeY < y + height && spikeY < 128; ++spikeY)
        {
            for (int spikeX = x - radius; spikeX <= x + radius; ++spikeX)
            {
                for (int spikeZ = z - radius; spikeZ <= z + radius; ++spikeZ)
                {
                    int dx = spikeX - x;
                    int dz = spikeZ - z;
                    if (dx * dx + dz * dz <= radius * radius + 1)
                    {
                        level.Writer.SetBlock(spikeX, spikeY, spikeZ, Block.Obsidian.id, 0, false);
                    }
                }
            }
        }

        int topY = Math.Min(127, y + height);
        EntityEnderCrystal crystal = new(level);
        crystal.setPositionAndAngles(x + 0.5D, topY, z + 0.5D, rand.NextFloat() * 360.0F, 0.0F);
        level.SpawnEntity(crystal);
        level.Writer.SetBlockWithoutNotifyingNeighbors(x, topY, z, Block.Bedrock.id, 0, false);
        return true;
    }
}
