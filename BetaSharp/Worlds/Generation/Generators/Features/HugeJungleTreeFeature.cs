using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Generators.Features;

internal sealed class HugeJungleTreeFeature(int minHeight, int logMeta, int leavesMeta) : Feature
{
    private readonly int _minHeight = minHeight;
    private readonly int _logMeta = logMeta;
    private readonly int _leavesMeta = leavesMeta;

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
            int checkRadius = cy == y ? 1 : 2;
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
                    if (blockId != 0 && blockId != Block.Leaves.id && blockId != Block.GrassBlock.id && blockId != Block.Dirt.id && blockId != Block.Log.id && blockId != Block.Sapling.id)
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
        world.Writer.SetBlockWithoutNotifyingNeighbors(x + 1, y - 1, z, Block.Dirt.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y - 1, z + 1, Block.Dirt.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(x + 1, y - 1, z + 1, Block.Dirt.id, 0, false);

        PlaceLeafLayer(world, x, z, y + treeHeight, 2, rand);

        for (int branchBaseY = y + treeHeight - 2 - rand.NextInt(4); branchBaseY > y + treeHeight / 2; branchBaseY -= 2 + rand.NextInt(4))
        {
            float angle = rand.NextFloat() * MathF.PI * 2.0F;
            int branchX = x + MathHelper.Floor(0.5F + MathF.Cos(angle) * 4.0F);
            int branchZ = z + MathHelper.Floor(0.5F + MathF.Sin(angle) * 4.0F);
            PlaceLeafLayer(world, branchX, branchZ, branchBaseY, 0, rand);

            for (int i = 0; i < 5; ++i)
            {
                branchX = x + MathHelper.Floor(1.5F + MathF.Cos(angle) * i);
                branchZ = z + MathHelper.Floor(1.5F + MathF.Sin(angle) * i);
                int branchMeta = Math.Abs(MathF.Cos(angle)) >= Math.Abs(MathF.Sin(angle))
                    ? _logMeta | 4
                    : _logMeta | 8;
                world.Writer.SetBlockWithoutNotifyingNeighbors(branchX, branchBaseY - 3 + i / 2, branchZ, Block.Log.id, branchMeta, false);
            }
        }

        for (int trunkY = 0; trunkY < treeHeight; ++trunkY)
        {
            PlaceTrunkBlock(world, x, y + trunkY, z);
            if (trunkY < treeHeight - 1)
            {
                PlaceTrunkBlock(world, x + 1, y + trunkY, z);
                PlaceTrunkBlock(world, x + 1, y + trunkY, z + 1);
                PlaceTrunkBlock(world, x, y + trunkY, z + 1);

                if (trunkY > 0)
                {
                    TryPlaceVine(world, rand, x - 1, y + trunkY, z, 8);
                    TryPlaceVine(world, rand, x, y + trunkY, z - 1, 1);
                    TryPlaceVine(world, rand, x + 2, y + trunkY, z, 2);
                    TryPlaceVine(world, rand, x + 1, y + trunkY, z - 1, 1);
                    TryPlaceVine(world, rand, x + 2, y + trunkY, z + 1, 2);
                    TryPlaceVine(world, rand, x + 1, y + trunkY, z + 2, 4);
                    TryPlaceVine(world, rand, x - 1, y + trunkY, z + 1, 8);
                    TryPlaceVine(world, rand, x, y + trunkY, z + 2, 4);
                }
            }
        }

        return true;
    }

    private void PlaceTrunkBlock(IWorldContext world, int x, int y, int z)
    {
        int blockId = world.Reader.GetBlockId(x, y, z);
        if (blockId == 0 || blockId == Block.Leaves.id)
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Log.id, _logMeta, false);
        }
    }

    private static void TryPlaceVine(IWorldContext world, JavaRandom rand, int x, int y, int z, int meta)
    {
        if (rand.NextInt(3) > 0 && world.Reader.IsAir(x, y, z))
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Vine.id, meta, false);
        }
    }

    private void PlaceLeafLayer(IWorldContext world, int x, int z, int y, int canopyInset, JavaRandom rand)
    {
        const int radius = 2;
        for (int cy = y - radius; cy <= y; ++cy)
        {
            int dy = cy - y;
            int layerRadius = canopyInset + 1 - dy;
            for (int cx = x - layerRadius; cx <= x + layerRadius + 1; ++cx)
            {
                int dx = cx - x;
                for (int cz = z - layerRadius; cz <= z + layerRadius + 1; ++cz)
                {
                    int dz = cz - z;
                    bool outsideInnerCorners = (dx < 0 || dz < 0 || dx * dx + dz * dz <= layerRadius * layerRadius) &&
                                               (dx <= 0 && dz <= 0 || dx * dx + dz * dz <= (layerRadius + 1) * (layerRadius + 1)) &&
                                               (rand.NextInt(4) != 0 || dx * dx + dz * dz <= (layerRadius - 1) * (layerRadius - 1));
                    if (outsideInnerCorners && !Block.BlocksOpaque[world.Reader.GetBlockId(cx, cy, cz)])
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(cx, cy, cz, Block.Leaves.id, _leavesMeta, false);
                    }
                }
            }
        }
    }
}
