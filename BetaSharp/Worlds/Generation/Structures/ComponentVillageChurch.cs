using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageChurch : ComponentVillage
{
    private int _averageGroundLevel = -1;

    public ComponentVillageChurch(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentVillageChurch? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 5, 12, 9, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageChurch(componentType, random, bounds, facing)
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

            BoundingBox.Offset(0, _averageGroundLevel - BoundingBox.MaxY + 11, 0);
        }

        FillWithBlocks(world, bounds, 1, 1, 1, 3, 3, 7, 0, 0, false);
        FillWithBlocks(world, bounds, 1, 5, 1, 3, 9, 3, 0, 0, false);
        FillWithBlocks(world, bounds, 1, 0, 0, 3, 0, 8, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 1, 0, 3, 10, 0, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 1, 1, 0, 10, 3, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 4, 1, 1, 4, 10, 3, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 0, 4, 0, 4, 7, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 4, 0, 4, 4, 4, 7, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 1, 8, 3, 4, 8, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 5, 4, 3, 10, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 1, 5, 5, 3, 5, 7, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 9, 0, 4, 9, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 4, 0, 4, 4, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 0, 11, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 11, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 2, 11, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 2, 11, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 1, 1, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 1, 1, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 2, 1, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 3, 1, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 3, 1, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 1, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 2, 1, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 3, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 1), 1, 2, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 0), 3, 2, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 6, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 7, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 6, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 7, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 6, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 7, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 6, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 7, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 3, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 3, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 3, 8, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 4, 7, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 1, 4, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 3, 4, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 4, 5, bounds);

        int ladderMeta = GetMetadataWithOffset(Block.Ladder.id, 4);
        for (int y = 1; y <= 9; ++y)
        {
            PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 3, y, 3, bounds);
        }

        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 2, 0, bounds);
        PlaceDoorAtCurrentPosition(world, bounds, random, 2, 1, 0, 1);
        if (GetBlockIdAtCurrentPosition(world, 2, 0, -1, bounds) == 0 && GetBlockIdAtCurrentPosition(world, 2, -1, -1, bounds) != 0)
        {
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), 2, 0, -1, bounds);
        }

        for (int z = 0; z < 9; ++z)
        {
            for (int x = 0; x < 5; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 12, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        SpawnVillagers(world, bounds, 2, 1, 2, 1);
        return true;
    }

    protected override int GetVillagerType(int index) => 2;
}
