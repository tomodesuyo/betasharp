using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageHouse4Garden : ComponentVillage
{
    private int _averageGroundLevel = -1;
    private readonly bool _isRoofAccessible;

    public ComponentVillageHouse4Garden(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
        _isRoofAccessible = random.NextBoolean();
    }

    public static ComponentVillageHouse4Garden? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 5, 6, 5, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageHouse4Garden(componentType, random, bounds, facing)
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

            BoundingBox.Offset(0, _averageGroundLevel - BoundingBox.MaxY + 5, 0);
        }

        FillWithBlocks(world, bounds, 0, 0, 0, 4, 0, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 4, 0, 4, 4, 4, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 1, 4, 1, 3, 4, 3, Block.Planks.id, Block.Planks.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 3, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 3, 4, bounds);
        FillWithBlocks(world, bounds, 0, 1, 1, 0, 3, 3, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 4, 1, 1, 4, 3, 3, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 1, 4, 3, 3, 4, Block.Planks.id, Block.Planks.id, false);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 1, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 1, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 1, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 2, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 3, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 3, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 3, 1, 0, bounds);

        if (GetBlockIdAtCurrentPosition(world, 2, 0, -1, bounds) == 0 && GetBlockIdAtCurrentPosition(world, 2, -1, -1, bounds) != 0)
        {
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 2, 0, -1, bounds);
        }

        FillWithBlocks(world, bounds, 1, 1, 1, 3, 3, 3, 0, 0, false);
        if (_isRoofAccessible)
        {
            for (int x = 0; x <= 4; ++x)
            {
                PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, x, 5, 0, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, x, 5, 4, bounds);
            }

            for (int z = 1; z <= 3; ++z)
            {
                PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 0, 5, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 4, 5, z, bounds);
            }

            int ladderMeta = GetMetadataWithOffset(Block.Ladder.id, 3);
            PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 3, 1, 3, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 3, 2, 3, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 3, 3, 3, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 3, 4, 3, bounds);
        }

        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 3, 1, bounds);

        for (int z = 0; z < 5; ++z)
        {
            for (int x = 0; x < 5; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 6, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        SpawnVillagers(world, bounds, 1, 1, 2, 1);
        return true;
    }
}
