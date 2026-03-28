using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageHouse2 : ComponentVillage
{
    private static readonly StructurePieceTreasure[] s_chestLoot =
    [
        new(Item.Diamond.id, 0, 1, 3, 3),
        new(Item.IronIngot.id, 0, 1, 5, 10),
        new(Item.GoldIngot.id, 0, 1, 3, 5),
        new(Item.Bread.id, 0, 1, 3, 15),
        new(Item.Apple.id, 0, 1, 3, 15),
        new(Item.IronPickaxe.id, 0, 1, 1, 5),
        new(Item.IronSword.id, 0, 1, 1, 5),
        new(Item.IronChestplate.id, 0, 1, 1, 5),
        new(Item.IronHelmet.id, 0, 1, 1, 5),
        new(Item.IronLeggings.id, 0, 1, 1, 5),
        new(Item.IronBoots.id, 0, 1, 1, 5),
        new(Block.Obsidian.id, 0, 3, 7, 5),
        new(Block.Sapling.id, 0, 3, 7, 5),
    ];

    private int _averageGroundLevel = -1;
    private bool _hasMadeChest;

    public ComponentVillageHouse2(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentVillageHouse2? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 10, 6, 7, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageHouse2(componentType, random, bounds, facing)
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

        FillWithBlocks(world, bounds, 0, 1, 0, 9, 4, 6, 0, 0, false);
        FillWithBlocks(world, bounds, 0, 0, 0, 9, 0, 6, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 4, 0, 9, 4, 6, Block.Cobblestone.id, Block.Cobblestone.id, false);
        FillWithBlocks(world, bounds, 0, 5, 0, 9, 5, 6, Block.Slab.id, Block.Slab.id, false);
        FillWithBlocks(world, bounds, 1, 5, 1, 8, 5, 5, 0, 0, false);
        FillWithBlocks(world, bounds, 1, 1, 0, 2, 3, 0, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 1, 0, 0, 4, 0, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 3, 1, 0, 3, 4, 0, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 0, 1, 6, 0, 4, 6, Block.Log.id, Block.Log.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 3, 3, 1, bounds);
        FillWithBlocks(world, bounds, 3, 1, 2, 3, 3, 2, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 4, 1, 3, 5, 3, 3, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 0, 1, 1, 0, 3, 5, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 1, 1, 6, 5, 3, 6, Block.Planks.id, Block.Planks.id, false);
        FillWithBlocks(world, bounds, 5, 1, 0, 5, 3, 0, Block.Fence.id, Block.Fence.id, false);
        FillWithBlocks(world, bounds, 9, 1, 0, 9, 3, 0, Block.Fence.id, Block.Fence.id, false);
        FillWithBlocks(world, bounds, 6, 1, 4, 9, 4, 6, Block.Cobblestone.id, Block.Cobblestone.id, false);
        PlaceBlockAtCurrentPosition(world, Block.FlowingLava.id, 0, 7, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.FlowingLava.id, 0, 8, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 9, 2, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 9, 2, 4, bounds);
        FillWithBlocks(world, bounds, 7, 2, 4, 8, 2, 5, 0, 0, false);
        PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 6, 1, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Furnace.id, 0, 6, 2, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Furnace.id, 0, 6, 3, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.DoubleSlab.id, 0, 8, 1, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 0, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 2, 2, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.GlassPane.id, 0, 4, 2, 6, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 2, 1, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenPressurePlate.id, 0, 2, 2, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 1, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, GetMetadataWithOffset(Block.WoodenStairs.id, 3), 2, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.WoodenStairs.id, GetMetadataWithOffset(Block.WoodenStairs.id, 1), 1, 1, 4, bounds);

        if (!_hasMadeChest)
        {
            int chestY = GetYWithOffset(1);
            int chestX = GetXWithOffset(5, 5);
            int chestZ = GetZWithOffset(5, 5);
            if (bounds.Contains(chestX, chestY, chestZ))
            {
                _hasMadeChest = true;
                CreateTreasureChestAtCurrentPosition(world, bounds, random, 5, 1, 5, s_chestLoot, 3 + random.NextInt(6));
            }
        }

        for (int x = 6; x <= 8; ++x)
        {
            if (GetBlockIdAtCurrentPosition(world, x, 0, -1, bounds) == 0 && GetBlockIdAtCurrentPosition(world, x, -1, -1, bounds) != 0)
            {
                PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, GetMetadataWithOffset(Block.CobblestoneStairs.id, 3), x, 0, -1, bounds);
            }
        }

        for (int z = 0; z < 7; ++z)
        {
            for (int x = 0; x < 10; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 6, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Cobblestone.id, 0, x, -1, z, bounds);
            }
        }

        SpawnVillagers(world, bounds, 7, 1, 1, 1);
        return true;
    }

    protected override int GetVillagerType(int index) => 3;
}
