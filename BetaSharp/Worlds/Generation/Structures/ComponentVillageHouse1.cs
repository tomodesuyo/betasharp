using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageHouse1 : ComponentVillage
{
    private int _averageGroundLevel = -1;

    public ComponentVillageHouse1(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentVillageHouse1? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 9, 9, 6, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageHouse1(componentType, random, bounds, facing)
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

            BoundingBox.Offset(0, _averageGroundLevel - BoundingBox.MaxY + 8, 0);
        }

        FillWithBlocks(world, bounds, 1, 1, 1, 7, 5, 4, 0, 0, false);
        FillWithBlocks(world, bounds, 0, 0, 0, 8, 0, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 5, 0, 8, 5, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 6, 1, 8, 6, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 7, 2, 8, 7, 3, Block.Cobblestone.id, Block.Cobblestone.id, false);

        int stairRight = GetMetadataWithOffset(Block.WoodenStairs.id, 3);
        int stairLeft = GetMetadataWithOffset(Block.WoodenStairs.id, 2);
        for (int roofOffset = -1; roofOffset <= 2; ++roofOffset)
        {
            for (int x = 0; x <= 8; ++x)
            {
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairRight, x, 6 + roofOffset, roofOffset, bounds);
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairLeft, x, 6 + roofOffset, 5 - roofOffset, bounds);
            }
        }

        FillWithBlocks(world, bounds, 0, 1, 0, 0, 1, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 1, 5, 8, 1, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 8, 1, 0, 8, 1, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 2, 1, 0, 7, 1, 0, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 2, 0, 0, 4, 0, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 2, 5, 0, 4, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 8, 2, 5, 8, 4, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 8, 2, 0, 8, 4, 0, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 2, 1, 0, 4, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 2, 5, 7, 4, 5, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 8, 2, 1, 8, 4, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 2, 0, 7, 4, 0, Block.Planks.id, Block.Planks.id, false);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 5, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 6, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 5, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 6, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 3, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 3, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 3, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 5, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 6, 2, 5, bounds);
        FillWithBlocks(world, bounds, 1, 4, 1, 7, 4, 1, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 4, 4, 7, 4, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 3, 4, 7, 3, 4, Block.Bookshelf.id, Block.Bookshelf.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 7, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, GetMetadataWithOffset(Block.WoodenStairs.id, 0), 7, 1, 3, bounds);

        int seatMeta = GetMetadataWithOffset(Block.WoodenStairs.id, 3);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, seatMeta, 6, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, seatMeta, 5, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, seatMeta, 4, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, seatMeta, 3, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 6, 1, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenPressurePlate.id, 0, 6, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 4, 1, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenPressurePlate.id, 0, 4, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.CraftingTable.id, 0, 7, 1, 1, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 1, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 1, 2, 0, bounds);
        PlaceDoorAtCurrentPosition(world, bounds, random, 1, 1, 0, 1);
        if (GetBlockIdAtCurrentPosition(world, 1, 0, -1, bounds) == 0 && GetBlockIdAtCurrentPosition(world, 1, -1, -1, bounds) != 0)
        {
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 1, 0, -1, bounds);
        }

        for (int z = 0; z < 6; ++z)
        {
            for (int x = 0; x < 9; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 9, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        SpawnVillagers(world, bounds, 2, 1, 2, 1);
        return true;
    }

    protected override int GetVillagerType(int index) => 1;
}
