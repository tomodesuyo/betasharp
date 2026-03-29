using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class JungleTreeFeature(int minHeight, int logMeta, int leavesMeta, bool growVines) : Feature
{
    private readonly int _minHeight = minHeight;
    private readonly int _logMeta = logMeta;
    private readonly int _leavesMeta = leavesMeta;
    private readonly bool _growVines = growVines;

    public override bool Generate(IWorldContext world, JavaRandom rand, int x, int y, int z)
    {
        int treeHeight = rand.NextInt(3) + _minHeight;
        bool canPlace = true;
        if (y < 1 || y + treeHeight + 1 > 128)
        {
            return false;
        }

        for (int cy = y; cy <= y + treeHeight + 1 && canPlace; ++cy)
        {
            int checkRadius = cy == y ? 0 : 1;
            if (cy >= y + treeHeight - 1)
            {
                checkRadius = 2;
            }

            for (int cx = x - checkRadius; cx <= x + checkRadius && canPlace; ++cx)
            {
                for (int cz = z - checkRadius; cz <= z + checkRadius && canPlace; ++cz)
                {
                    if (cy < 0 || cy >= 128)
                    {
                        canPlace = false;
                        continue;
                    }

                    int blockId = world.Reader.GetBlockId(cx, cy, cz);
                    if (blockId != 0 && blockId != Block.Leaves.id && blockId != Block.GrassBlock.id && blockId != Block.Dirt.id && blockId != Block.Log.id)
                    {
                        canPlace = false;
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
            int leafRadius = 1 - relativeY / 2;

            for (int leafX = x - leafRadius; leafX <= x + leafRadius; ++leafX)
            {
                int offsetX = leafX - x;
                for (int leafZ = z - leafRadius; leafZ <= z + leafRadius; ++leafZ)
                {
                    int offsetZ = leafZ - z;
                    if ((Math.Abs(offsetX) != leafRadius || Math.Abs(offsetZ) != leafRadius || (rand.NextInt(2) != 0 && relativeY != 0)) &&
                        !Block.BlocksOpaque[world.Reader.GetBlockId(leafX, leafY, leafZ)])
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(leafX, leafY, leafZ, Block.Leaves.id, _leavesMeta, false);
                    }
                }
            }
        }

        for (int trunkY = 0; trunkY < treeHeight; ++trunkY)
        {
            int blockId = world.Reader.GetBlockId(x, y + trunkY, z);
            if (blockId == 0 || blockId == Block.Leaves.id)
            {
                world.Writer.SetBlockWithoutNotifyingNeighbors(x, y + trunkY, z, Block.Log.id, _logMeta, false);

                if (_growVines && trunkY > 0)
                {
                    if (rand.NextInt(3) > 0 && world.Reader.IsAir(x - 1, y + trunkY, z))
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x - 1, y + trunkY, z, Block.Vine.id, 8, false);
                    }

                    if (rand.NextInt(3) > 0 && world.Reader.IsAir(x + 1, y + trunkY, z))
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x + 1, y + trunkY, z, Block.Vine.id, 2, false);
                    }

                    if (rand.NextInt(3) > 0 && world.Reader.IsAir(x, y + trunkY, z - 1))
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y + trunkY, z - 1, Block.Vine.id, 1, false);
                    }

                    if (rand.NextInt(3) > 0 && world.Reader.IsAir(x, y + trunkY, z + 1))
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y + trunkY, z + 1, Block.Vine.id, 4, false);
                    }
                }
            }
        }

        if (_growVines)
        {
            for (int leafY = y - 3 + treeHeight; leafY <= y + treeHeight; ++leafY)
            {
                int relativeY = leafY - (y + treeHeight);
                int leafRadius = 2 - relativeY / 2;

                for (int leafX = x - leafRadius; leafX <= x + leafRadius; ++leafX)
                {
                    for (int leafZ = z - leafRadius; leafZ <= z + leafRadius; ++leafZ)
                    {
                        if (world.Reader.GetBlockId(leafX, leafY, leafZ) != Block.Leaves.id)
                        {
                            continue;
                        }

                        if (rand.NextInt(4) == 0 && world.Reader.IsAir(leafX - 1, leafY, leafZ))
                        {
                            GrowVines(world, leafX - 1, leafY, leafZ, 8);
                        }

                        if (rand.NextInt(4) == 0 && world.Reader.IsAir(leafX + 1, leafY, leafZ))
                        {
                            GrowVines(world, leafX + 1, leafY, leafZ, 2);
                        }

                        if (rand.NextInt(4) == 0 && world.Reader.IsAir(leafX, leafY, leafZ - 1))
                        {
                            GrowVines(world, leafX, leafY, leafZ - 1, 1);
                        }

                        if (rand.NextInt(4) == 0 && world.Reader.IsAir(leafX, leafY, leafZ + 1))
                        {
                            GrowVines(world, leafX, leafY, leafZ + 1, 4);
                        }
                    }
                }
            }

            if (treeHeight > 5 && rand.NextInt(5) == 0)
            {
                for (int level = 0; level < 2; ++level)
                {
                    for (int direction = 0; direction < 4; ++direction)
                    {
                        if (rand.NextInt(4 - level) != 0)
                        {
                            continue;
                        }

                        int cocoaAge = rand.NextInt(3);
                        int cocoaX = x + GetCocoaOffsetX(direction);
                        int cocoaY = y + treeHeight - 5 + level;
                        int cocoaZ = z + GetCocoaOffsetZ(direction);
                        if (world.Reader.IsAir(cocoaX, cocoaY, cocoaZ))
                        {
                            world.Writer.SetBlockWithoutNotifyingNeighbors(cocoaX, cocoaY, cocoaZ, Block.Cocoa.id, cocoaAge << 2 | direction, false);
                        }
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
        while (--y > 0 && remaining-- > 0 && world.Reader.IsAir(x, y, z))
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Vine.id, meta, false);
        }
    }

    private static int GetCocoaOffsetX(int direction) => direction switch
    {
        1 => -1,
        3 => 1,
        _ => 0
    };

    private static int GetCocoaOffsetZ(int direction) => direction switch
    {
        0 => 1,
        2 => -1,
        _ => 0
    };
}
