using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageHall : ComponentVillage
{
    private int _averageGroundLevel = -1;

    public ComponentVillageHall(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentVillageHall? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 9, 7, 11, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageHall(componentType, random, bounds, facing)
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
        FillWithBlocks(world, bounds, 2, 0, 6, 8, 0, 10, Block.Dirt.id, Block.Dirt.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 6, 0, 6, bounds);
        FillWithBlocks(world, bounds, 2, 1, 6, 2, 1, 10, Block.Fence.id, Block.Fence.id, false);
        FillWithBlocks(world, bounds, 8, 1, 6, 8, 1, 10, Block.Fence.id, Block.Fence.id, false);
        FillWithBlocks(world, bounds, 3, 1, 10, 7, 1, 10, Block.Fence.id, Block.Fence.id, false);
        FillWithBlocks(world, bounds, 1, 0, 1, 7, 0, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 0, 0, 0, 3, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 8, 0, 0, 8, 3, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 0, 0, 7, 1, 0, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 0, 5, 7, 1, 5, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 2, 0, 7, 3, 0, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 2, 5, 7, 3, 5, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 4, 1, 8, 4, 1, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 4, 4, 8, 4, 4, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 5, 2, 8, 5, 3, Block.Planks.id, Block.Planks.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 0, 4, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 0, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 4, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 4, 3, bounds);

        int stairRight = GetMetadataWithOffset(Block.WoodenStairs.id, 3);
        int stairLeft = GetMetadataWithOffset(Block.WoodenStairs.id, 2);
        for (int roofOffset = -1; roofOffset <= 2; ++roofOffset)
        {
            for (int x = 0; x <= 8; ++x)
            {
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairRight, x, 4 + roofOffset, roofOffset, bounds);
                PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, stairLeft, x, 4 + roofOffset, 5 - roofOffset, bounds);
            }
        }

        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 0, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 0, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 8, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Log.id, 0, 8, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 8, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 3, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 5, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 6, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 2, 1, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenPressurePlate.id, 0, 2, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 1, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, GetMetadataWithOffset(Block.WoodenStairs.id, 3), 2, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, GetMetadataWithOffset(Block.WoodenStairs.id, 1), 1, 1, 3, bounds);
        FillWithBlocks(world, bounds, 5, 0, 1, 7, 0, 3, Block.DoubleSlab.id, Block.DoubleSlab.id, false);
        PlaceBlockAtCurrentPosition(world, Block.DoubleSlab.id, 0, 6, 1, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.DoubleSlab.id, 0, 6, 1, 2, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 3, 1, bounds);
        PlaceDoorAtCurrentPosition(world, bounds, random, 2, 1, 0, 1);
        if (GetBlockIdAtCurrentPosition(world, 2, 0, -1, bounds) == 0 && GetBlockIdAtCurrentPosition(world, 2, -1, -1, bounds) != 0)
        {
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 2, 0, -1, bounds);
        }

        PlaceBlockAtCurrentPosition(world, 0, 0, 6, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 6, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 6, 3, 4, bounds);
        PlaceDoorAtCurrentPosition(world, bounds, random, 6, 1, 5, 1);

        for (int z = 0; z < 5; ++z)
        {
            for (int x = 0; x < 9; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 7, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        SpawnVillagers(world, bounds, 4, 1, 2, 2);
        return true;
    }

    protected override int GetVillagerType(int index) => index == 0 ? 4 : 0;
}
