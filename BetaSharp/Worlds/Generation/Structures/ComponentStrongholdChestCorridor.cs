using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdChestCorridor : ComponentStronghold
{
    private static readonly StructurePieceTreasure[] s_chestLoot =
    [
        new(Item.EnderPearl.id, 0, 1, 1, 10),
        new(Item.Diamond.id, 0, 1, 3, 3),
        new(Item.IronIngot.id, 0, 1, 5, 10),
        new(Item.GoldIngot.id, 0, 1, 3, 5),
        new(Item.Redstone.id, 0, 4, 9, 5),
        new(Item.Bread.id, 0, 1, 3, 15),
        new(Item.Apple.id, 0, 1, 3, 15),
        new(Item.IronPickaxe.id, 0, 1, 1, 5),
        new(Item.IronSword.id, 0, 1, 1, 5),
        new(Item.IronChestplate.id, 0, 1, 1, 5),
        new(Item.IronHelmet.id, 0, 1, 1, 5),
        new(Item.IronLeggings.id, 0, 1, 1, 5),
        new(Item.IronBoots.id, 0, 1, 1, 5),
        new(Item.GoldenApple.id, 0, 1, 1, 1),
    ];

    private readonly StrongholdDoor _doorType;
    private bool _hasMadeChest;

    public ComponentStrongholdChestCorridor(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
    }

    public static ComponentStrongholdChestCorridor? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -1, 0, 5, 5, 7, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdChestCorridor(componentType, random, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        GetNextComponentNormal((ComponentStrongholdStairs2)component, components, random, 1, 1);
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 4, 4, 6, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 1, 1, 0);
        PlaceDoor(world, random, bounds, StrongholdDoor.Opening, 1, 1, 6);
        FillWithBlocks(world, bounds, 3, 1, 2, 3, 1, 4, Block.StoneBrick.id, Block.StoneBrick.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 5, 3, 1, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 5, 3, 1, 5, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 5, 3, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 5, 3, 2, 4, bounds);

        for (int z = 2; z <= 4; ++z)
        {
            PlaceBlockAtCurrentPosition(world, Block.Slab.id, 5, 2, 1, z, bounds);
        }

        if (!_hasMadeChest)
        {
            int chestY = GetYWithOffset(2);
            int chestX = GetXWithOffset(3, 3);
            int chestZ = GetZWithOffset(3, 3);
            if (bounds.Contains(chestX, chestY, chestZ))
            {
                _hasMadeChest = true;
                CreateTreasureChestAtCurrentPosition(world, bounds, random, 3, 2, 3, s_chestLoot, 2 + random.NextInt(2));
            }
        }

        return true;
    }
}
