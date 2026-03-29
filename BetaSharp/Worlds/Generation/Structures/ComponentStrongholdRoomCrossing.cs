using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdRoomCrossing : ComponentStronghold
{
    private static readonly StructurePieceTreasure[] s_chestLoot =
    [
        new(Item.IronIngot.id, 0, 1, 5, 10),
        new(Item.GoldIngot.id, 0, 1, 3, 5),
        new(Item.Redstone.id, 0, 4, 9, 5),
        new(Item.Coal.id, 0, 3, 8, 10),
        new(Item.Bread.id, 0, 1, 3, 15),
        new(Item.Apple.id, 0, 1, 3, 15),
        new(Item.IronPickaxe.id, 0, 1, 1, 1),
    ];

    private readonly StrongholdDoor _doorType;
    private readonly int _roomType;

    public ComponentStrongholdRoomCrossing(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
        _roomType = random.NextInt(5);
    }

    public static ComponentStrongholdRoomCrossing? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -4, -1, 0, 11, 7, 11, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdRoomCrossing(componentType, random, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        GetNextComponentNormal((ComponentStrongholdStairs2)component, components, random, 4, 1);
        GetNextComponentX((ComponentStrongholdStairs2)component, components, random, 1, 4);
        GetNextComponentZ((ComponentStrongholdStairs2)component, components, random, 1, 4);
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 10, 6, 10, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 4, 1, 0);
        FillWithBlocks(world, bounds, 4, 1, 10, 6, 3, 10, 0, 0, false);
        FillWithBlocks(world, bounds, 0, 1, 4, 0, 3, 6, 0, 0, false);
        FillWithBlocks(world, bounds, 10, 1, 4, 10, 3, 6, 0, 0, false);

        switch (_roomType)
        {
            case 0:
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 5, 1, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 5, 2, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 5, 3, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 4, 3, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 6, 3, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 5, 3, 4, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 5, 3, 6, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 4, 1, 4, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 4, 1, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 4, 1, 6, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 6, 1, 4, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 6, 1, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 6, 1, 6, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 5, 1, 4, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 5, 1, 6, bounds);
                break;
            case 1:
                for (int i = 0; i < 5; ++i)
                {
                    PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 1, 3 + i, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 7, 1, 3 + i, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3 + i, 1, 3, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3 + i, 1, 7, bounds);
                }

                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 5, 1, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 5, 2, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 5, 3, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.FlowingWater.id, 0, 5, 4, 5, bounds);
                break;
            case 2:
                for (int i = 1; i <= 9; ++i)
                {
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 1, 3, i, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 9, 3, i, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, i, 3, 1, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, i, 3, 9, bounds);
                }

                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 5, 1, 4, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 5, 1, 6, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 5, 3, 4, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 5, 3, 6, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 1, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 6, 1, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, 3, 5, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 6, 3, 5, bounds);

                for (int y = 1; y <= 3; ++y)
                {
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, y, 4, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 6, y, 4, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 4, y, 6, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Cobblestone.id, 0, 6, y, 6, bounds);
                }

                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 5, 3, 5, bounds);

                for (int z = 2; z <= 8; ++z)
                {
                    PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 2, 3, z, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 3, 3, z, bounds);
                    if (z <= 3 || z >= 7)
                    {
                        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 4, 3, z, bounds);
                        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 5, 3, z, bounds);
                        PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 6, 3, z, bounds);
                    }

                    PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 7, 3, z, bounds);
                    PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 3, z, bounds);
                }

                int ladderMeta = GetMetadataWithOffset(Block.Ladder.id, 4);
                PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 9, 1, 3, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 9, 2, 3, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 9, 3, 3, bounds);
                CreateTreasureChestAtCurrentPosition(world, bounds, random, 3, 4, 8, s_chestLoot, 1 + random.NextInt(4));
                break;
        }

        return true;
    }
}
