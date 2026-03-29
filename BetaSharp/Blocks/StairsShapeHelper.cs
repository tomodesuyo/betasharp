using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

public static class StairsShapeHelper
{
    public static void GetSubBoxes(IBlockReader world, int x, int y, int z, List<Box> boxes, int? metadataOverride = null)
    {
        boxes.Clear();

        int meta = metadataOverride ?? world.GetBlockMeta(x, y, z);
        bool topHalf = (meta & 4) != 0;
        int direction = meta & 3;

        float slabMinY = topHalf ? 0.5F : 0.0F;
        float slabMaxY = topHalf ? 1.0F : 0.5F;
        float stepMinY = topHalf ? 0.0F : 0.5F;
        float stepMaxY = topHalf ? 0.5F : 1.0F;

        boxes.Add(new Box(0.0F, slabMinY, 0.0F, 1.0F, slabMaxY, 1.0F));

        bool allowSecondary = TryGetPrimaryStepBox(world, x, y, z, meta, direction, stepMinY, stepMaxY, out Box primary);
        boxes.Add(primary);

        if (allowSecondary && TryGetSecondaryStepBox(world, x, y, z, meta, direction, stepMinY, stepMaxY, out Box secondary))
        {
            boxes.Add(secondary);
        }
    }

    public static HitResult Raycast(IBlockReader world, int x, int y, int z, Vec3D startPos, Vec3D endPos, int? metadataOverride = null)
    {
        List<Box> boxes = [];
        GetSubBoxes(world, x, y, z, boxes, metadataOverride);

        Vec3D blockPos = new(x, y, z);
        Vec3D localStart = startPos - blockPos;
        Vec3D localEnd = endPos - blockPos;
        HitResult bestHit = new(HitResultType.MISS);
        double bestDistance = double.MaxValue;

        for (int i = 0; i < boxes.Count; ++i)
        {
            HitResult hit = boxes[i].Raycast(localStart, localEnd);
            if (hit.Type == HitResultType.MISS)
            {
                continue;
            }

            double distance = localStart.distanceTo(hit.Pos);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestHit = hit;
            }
        }

        if (bestHit.Type == HitResultType.MISS)
        {
            return bestHit;
        }

        bestHit.BlockX = x;
        bestHit.BlockY = y;
        bestHit.BlockZ = z;
        bestHit.Pos += blockPos;
        return bestHit;
    }

    private static bool TryGetPrimaryStepBox(IBlockReader world, int x, int y, int z, int meta, int direction, float minY, float maxY, out Box box)
    {
        float minX = 0.0F;
        float maxX = 1.0F;
        float minZ = 0.0F;
        float maxZ = 0.5F;
        bool allowSecondary = true;
        int neighborId;
        int neighborMeta;
        int neighborDirection;

        switch (direction)
        {
            case 0:
                minX = 0.5F;
                maxZ = 1.0F;
                neighborId = world.GetBlockId(x + 1, y, z);
                neighborMeta = world.GetBlockMeta(x + 1, y, z);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 3 && !HasSameStair(world, x, y, z + 1, meta))
                    {
                        maxZ = 0.5F;
                        allowSecondary = false;
                    }
                    else if (neighborDirection == 2 && !HasSameStair(world, x, y, z - 1, meta))
                    {
                        minZ = 0.5F;
                        allowSecondary = false;
                    }
                }
                break;
            case 1:
                maxX = 0.5F;
                maxZ = 1.0F;
                neighborId = world.GetBlockId(x - 1, y, z);
                neighborMeta = world.GetBlockMeta(x - 1, y, z);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 3 && !HasSameStair(world, x, y, z + 1, meta))
                    {
                        maxZ = 0.5F;
                        allowSecondary = false;
                    }
                    else if (neighborDirection == 2 && !HasSameStair(world, x, y, z - 1, meta))
                    {
                        minZ = 0.5F;
                        allowSecondary = false;
                    }
                }
                break;
            case 2:
                minZ = 0.5F;
                maxZ = 1.0F;
                neighborId = world.GetBlockId(x, y, z + 1);
                neighborMeta = world.GetBlockMeta(x, y, z + 1);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 1 && !HasSameStair(world, x + 1, y, z, meta))
                    {
                        maxX = 0.5F;
                        allowSecondary = false;
                    }
                    else if (neighborDirection == 0 && !HasSameStair(world, x - 1, y, z, meta))
                    {
                        minX = 0.5F;
                        allowSecondary = false;
                    }
                }
                break;
            default:
                neighborId = world.GetBlockId(x, y, z - 1);
                neighborMeta = world.GetBlockMeta(x, y, z - 1);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 1 && !HasSameStair(world, x + 1, y, z, meta))
                    {
                        maxX = 0.5F;
                        allowSecondary = false;
                    }
                    else if (neighborDirection == 0 && !HasSameStair(world, x - 1, y, z, meta))
                    {
                        minX = 0.5F;
                        allowSecondary = false;
                    }
                }
                break;
        }

        box = new Box(minX, minY, minZ, maxX, maxY, maxZ);
        return allowSecondary;
    }

    private static bool TryGetSecondaryStepBox(IBlockReader world, int x, int y, int z, int meta, int direction, float minY, float maxY, out Box box)
    {
        float minX = 0.0F;
        float maxX = 0.5F;
        float minZ = 0.5F;
        float maxZ = 1.0F;
        bool found = false;
        int neighborId;
        int neighborMeta;
        int neighborDirection;

        switch (direction)
        {
            case 0:
                neighborId = world.GetBlockId(x - 1, y, z);
                neighborMeta = world.GetBlockMeta(x - 1, y, z);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 3 && !HasSameStair(world, x, y, z - 1, meta))
                    {
                        minZ = 0.0F;
                        maxZ = 0.5F;
                        found = true;
                    }
                    else if (neighborDirection == 2 && !HasSameStair(world, x, y, z + 1, meta))
                    {
                        found = true;
                    }
                }
                break;
            case 1:
                neighborId = world.GetBlockId(x + 1, y, z);
                neighborMeta = world.GetBlockMeta(x + 1, y, z);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    minX = 0.5F;
                    maxX = 1.0F;
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 3 && !HasSameStair(world, x, y, z - 1, meta))
                    {
                        minZ = 0.0F;
                        maxZ = 0.5F;
                        found = true;
                    }
                    else if (neighborDirection == 2 && !HasSameStair(world, x, y, z + 1, meta))
                    {
                        found = true;
                    }
                }
                break;
            case 2:
                neighborId = world.GetBlockId(x, y, z - 1);
                neighborMeta = world.GetBlockMeta(x, y, z - 1);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    minZ = 0.0F;
                    maxZ = 0.5F;
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 1 && !HasSameStair(world, x - 1, y, z, meta))
                    {
                        found = true;
                    }
                    else if (neighborDirection == 0 && !HasSameStair(world, x + 1, y, z, meta))
                    {
                        minX = 0.5F;
                        maxX = 1.0F;
                        found = true;
                    }
                }
                break;
            default:
                neighborId = world.GetBlockId(x, y, z + 1);
                neighborMeta = world.GetBlockMeta(x, y, z + 1);
                if (IsStairs(neighborId) && (meta & 4) == (neighborMeta & 4))
                {
                    neighborDirection = neighborMeta & 3;
                    if (neighborDirection == 1 && !HasSameStair(world, x - 1, y, z, meta))
                    {
                        found = true;
                    }
                    else if (neighborDirection == 0 && !HasSameStair(world, x + 1, y, z, meta))
                    {
                        minX = 0.5F;
                        maxX = 1.0F;
                        found = true;
                    }
                }
                break;
        }

        box = new Box(minX, minY, minZ, maxX, maxY, maxZ);
        return found;
    }

    private static bool HasSameStair(IBlockReader world, int x, int y, int z, int metadata)
    {
        return IsStairs(world.GetBlockId(x, y, z)) && world.GetBlockMeta(x, y, z) == metadata;
    }

    private static bool IsStairs(int blockId)
    {
        return blockId > 0 && Block.Blocks[blockId].getRenderType() == BlockRendererType.Stairs;
    }
}
