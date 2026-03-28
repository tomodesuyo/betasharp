using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageHouse3 : ComponentVillage
{
    private int _averageGroundLevel = -1;

    public ComponentVillageHouse3(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentVillageHouse3? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 9, 7, 12, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageHouse3(componentType, random, bounds, facing)
            : null;
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (_averageGroundLevel < 0)
        {
            _averageGroundLevel = GetAverageGroundLevel(world, bounds);
            if (_averageGroundLevel < 0)
            {
                return true;
            }

            BoundingBox.Offset(0, _averageGroundLevel - BoundingBox.MaxY + 6, 0);
        }

        FillWithBlocks(world, bounds, 1, 1, 1, 7, 4, 4, 0, 0, false);
        FillWithBlocks(world, bounds, 2, 1, 6, 8, 4, 10, 0, 0, false);
        FillWithBlocks(world, bounds, 2, 0, 5, 8, 0, 10, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 0, 1, 7, 0, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 0, 0, 0, 3, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 8, 0, 0, 8, 3, 10, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 0, 0, 7, 2, 0, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 0, 5, 2, 1, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 2, 0, 6, 2, 3, 10, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 3, 0, 10, 7, 3, 10, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 2, 0, 7, 3, 0, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 2, 5, 2, 3, 5, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 4, 1, 8, 4, 1, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 4, 4, 3, 4, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 5, 2, 8, 5, 3, Block.Planks.id, Block.Planks.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 0, 4, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 0, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 4, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 4, 4, bounds);

        int stairNorth = GetMetadataWithOffset(Block.WoodenStairs.id, 3);
        int stairSouth = GetMetadataWithOffset(Block.WoodenStairs.id, 2);
        for (int roofOffset = -1; roofOffset <= 2; ++roofOffset)
        {
            for (int x = 0; x <= 8; ++x)
            {
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairNorth, x, 4 + roofOffset, roofOffset, bounds);
                if ((roofOffset > -1 || x <= 1) && (roofOffset > 0 || x <= 3) && (roofOffset > 1 || x <= 4 || x >= 6))
                {
                    PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairSouth, x, 4 + roofOffset, 5 - roofOffset, bounds);
                }
            }
        }

        FillWithBlocks(world, bounds, 3, 4, 5, 3, 4, 10, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 7, 4, 2, 7, 4, 10, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 4, 5, 4, 4, 5, 10, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 6, 5, 4, 6, 5, 10, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 5, 6, 3, 5, 6, 10, Block.Planks.id, Block.Planks.id, false);

        int stairEast = GetMetadataWithOffset(Block.WoodenStairs.id, 0);
        for (int z = 4; z >= 1; --z)
        {
            PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, z, 2 + z, 7 - z, bounds);
            for (int x = 8 - z; x <= 10; ++x)
            {
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairEast, z, 2 + z, x, bounds);
            }
        }

        int stairWest = GetMetadataWithOffset(Block.WoodenStairs.id, 1);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 6, 6, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 7, 5, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairWest, 6, 6, 4, bounds);

        for (int y = 6; y <= 8; ++y)
        {
            for (int z = 5; z <= 10; ++z)
            {
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairWest, y, 12 - y, z, bounds);
            }
        }

        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 0, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 0, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 4, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 5, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 6, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 8, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 8, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 8, 2, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 8, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 8, 2, 9, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 2, 2, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 2, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 2, 8, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 2, 2, 9, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 4, 4, 10, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 5, 4, 10, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 6, 4, 10, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 5, 5, 10, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 3, 1, bounds);
        PlaceDoorAtCurrentPosition(world, bounds, random, 2, 1, 0, 1);

        if (GetBlockIdAtCurrentPosition(world, 2, 0, -1, bounds) == 0 && GetBlockIdAtCurrentPosition(world, 2, -1, -1, bounds) != 0)
        {
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 2, 0, -1, bounds);
        }

        for (int z = 0; z < 5; ++z)
        {
            for (int x = 0; x < 9; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 7, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        for (int z = 5; z < 11; ++z)
        {
            for (int x = 2; x < 9; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 7, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        SpawnVillagers(world, bounds, 4, 1, 2, 2);
        return true;
    }
}
