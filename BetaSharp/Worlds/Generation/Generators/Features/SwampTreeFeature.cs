using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class SwampTreeFeature : Feature
{
    public override bool Generate(IWorldContext world, JavaRandom rand, int x, int y, int z)
    {
        int treeHeight = rand.NextInt(4) + 5;
        while (world.Reader.GetMaterial(x, y - 1, z) == Material.Water)
        {
            --y;
        }

        bool canPlace = true;
        if (y < 1 || y + treeHeight + 1 > 128)
        {
            return false;
        }

        for (int checkY = y; checkY <= y + 1 + treeHeight && canPlace; ++checkY)
        {
            int radius = 1;
            if (checkY == y)
            {
                radius = 0;
            }

            if (checkY >= y + 1 + treeHeight - 2)
            {
                radius = 3;
            }

            for (int checkX = x - radius; checkX <= x + radius && canPlace; ++checkX)
            {
                for (int checkZ = z - radius; checkZ <= z + radius && canPlace; ++checkZ)
                {
                    if (checkY < 0 || checkY >= 128)
                    {
                        canPlace = false;
                        continue;
                    }

                    int blockId = world.Reader.GetBlockId(checkX, checkY, checkZ);
                    if (blockId != 0 && blockId != Block.Leaves.id)
                    {
                        if (blockId != Block.FlowingWater.id && world.Reader.GetMaterial(checkX, checkY, checkZ) != Material.Water)
                        {
                            canPlace = false;
                        }
                        else if (checkY > y)
                        {
                            canPlace = false;
                        }
                    }
                }
            }
        }

        if (!canPlace)
        {
            return false;
        }

        int soilId = world.Reader.GetBlockId(x, y - 1, z);
        if ((soilId != Block.GrassBlock.id && soilId != Block.Dirt.id) || y >= 128 - treeHeight - 1)
        {
            return false;
        }

        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y - 1, z, Block.Dirt.id, 0, false);

        for (int leafY = y - 3 + treeHeight; leafY <= y + treeHeight; ++leafY)
        {
            int relativeY = leafY - (y + treeHeight);
            int radius = 2 - relativeY / 2;

            for (int leafX = x - radius; leafX <= x + radius; ++leafX)
            {
                int offsetX = leafX - x;
                for (int leafZ = z - radius; leafZ <= z + radius; ++leafZ)
                {
                    int offsetZ = leafZ - z;
                    if ((Math.Abs(offsetX) != radius || Math.Abs(offsetZ) != radius || (rand.NextInt(2) != 0 && relativeY != 0)) &&
                        !Block.BlocksOpaque[world.Reader.GetBlockId(leafX, leafY, leafZ)])
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(leafX, leafY, leafZ, Block.Leaves.id, 0, false);
                    }
                }
            }
        }

        for (int trunkY = 0; trunkY < treeHeight; ++trunkY)
        {
            int blockId = world.Reader.GetBlockId(x, y + trunkY, z);
            if (blockId == 0 || blockId == Block.Leaves.id || blockId == Block.FlowingWater.id || world.Reader.GetMaterial(x, y + trunkY, z) == Material.Water)
            {
                world.Writer.SetBlockWithoutNotifyingNeighbors(x, y + trunkY, z, Block.Log.id, 0, false);
            }
        }

        for (int leafY = y - 3 + treeHeight; leafY <= y + treeHeight; ++leafY)
        {
            int relativeY = leafY - (y + treeHeight);
            int radius = 2 - relativeY / 2;

            for (int leafX = x - radius; leafX <= x + radius; ++leafX)
            {
                for (int leafZ = z - radius; leafZ <= z + radius; ++leafZ)
                {
                    if (world.Reader.GetBlockId(leafX, leafY, leafZ) != Block.Leaves.id)
                    {
                        continue;
                    }

                    if (rand.NextInt(4) == 0 && world.Reader.GetBlockId(leafX - 1, leafY, leafZ) == 0)
                    {
                        GrowVines(world, leafX - 1, leafY, leafZ, 8);
                    }

                    if (rand.NextInt(4) == 0 && world.Reader.GetBlockId(leafX + 1, leafY, leafZ) == 0)
                    {
                        GrowVines(world, leafX + 1, leafY, leafZ, 2);
                    }

                    if (rand.NextInt(4) == 0 && world.Reader.GetBlockId(leafX, leafY, leafZ - 1) == 0)
                    {
                        GrowVines(world, leafX, leafY, leafZ - 1, 1);
                    }

                    if (rand.NextInt(4) == 0 && world.Reader.GetBlockId(leafX, leafY, leafZ + 1) == 0)
                    {
                        GrowVines(world, leafX, leafY, leafZ + 1, 4);
                    }
                }
            }
        }

        return true;
    }

    private static void GrowVines(IWorldContext world, int x, int y, int z, int meta)
    {
        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Vine.id, meta, false);
        int remaining = 4;

        while (--y > 0 && remaining-- > 0 && world.Reader.GetBlockId(x, y, z) == 0)
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Vine.id, meta, false);
        }
    }
}
