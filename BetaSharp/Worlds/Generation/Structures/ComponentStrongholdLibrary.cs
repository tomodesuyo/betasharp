using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdLibrary : ComponentStronghold
{
    private static readonly StructurePieceTreasure[] s_chestLoot =
    [
        new(Item.Book.id, 0, 1, 3, 20),
        new(Item.Paper.id, 0, 2, 7, 20),
        new(Item.Map.id, 0, 1, 1, 1),
        new(Item.Compass.id, 0, 1, 1, 1),
    ];

    private readonly StrongholdDoor _doorType;
    private readonly bool _isLargeRoom;

    public ComponentStrongholdLibrary(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
        _isLargeRoom = bounds.GetYSize() > 6;
    }

    public static ComponentStrongholdLibrary? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -4, -1, 0, 14, 11, 15, facing);
        if (!CanStrongholdGoDeeper(bounds) || FindIntersecting(components, bounds) != null)
        {
            bounds = StructureBoundingBox.Create(x, y, z, -4, -1, 0, 14, 6, 15, facing);
            if (!CanStrongholdGoDeeper(bounds) || FindIntersecting(components, bounds) != null)
            {
                return null;
            }
        }

        return new ComponentStrongholdLibrary(componentType, random, bounds, facing);
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        int maxY = _isLargeRoom ? 11 : 6;
        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 13, maxY - 1, 14, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 4, 1, 0);
        FillWithRandomizedBlocks(world, bounds, random, 0.07F, 2, 1, 1, 11, 4, 13, Block.Cobweb.id, Block.Cobweb.id, false);

        for (int z = 1; z <= 13; ++z)
        {
            bool support = (z - 1) % 4 == 0;
            int blockId = support ? Block.Planks.id : Block.Bookshelf.id;
            FillWithBlocks(world, bounds, 1, 1, z, 1, 4, z, blockId, blockId, false);
            FillWithBlocks(world, bounds, 12, 1, z, 12, 4, z, blockId, blockId, false);
            if (support)
            {
                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 3, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 11, 3, z, bounds);
            }

            if (_isLargeRoom)
            {
                FillWithBlocks(world, bounds, 1, 6, z, 1, 9, z, blockId, blockId, false);
                FillWithBlocks(world, bounds, 12, 6, z, 12, 9, z, blockId, blockId, false);
            }
        }

        for (int z = 3; z < 12; z += 2)
        {
            FillWithBlocks(world, bounds, 3, 1, z, 4, 3, z, Block.Bookshelf.id, Block.Bookshelf.id, false);
            FillWithBlocks(world, bounds, 6, 1, z, 7, 3, z, Block.Bookshelf.id, Block.Bookshelf.id, false);
            FillWithBlocks(world, bounds, 9, 1, z, 10, 3, z, Block.Bookshelf.id, Block.Bookshelf.id, false);
        }

        if (_isLargeRoom)
        {
            FillWithBlocks(world, bounds, 1, 5, 1, 3, 5, 13, Block.Planks.id, Block.Planks.id, false);
            FillWithBlocks(world, bounds, 10, 5, 1, 12, 5, 13, Block.Planks.id, Block.Planks.id, false);
            FillWithBlocks(world, bounds, 4, 5, 1, 9, 5, 2, Block.Planks.id, Block.Planks.id, false);
            FillWithBlocks(world, bounds, 4, 5, 12, 9, 5, 13, Block.Planks.id, Block.Planks.id, false);
            PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 9, 5, 11, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 8, 5, 11, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Planks.id, 0, 9, 5, 10, bounds);
            FillWithBlocks(world, bounds, 3, 6, 2, 3, 6, 12, Block.Fence.id, Block.Fence.id, false);
            FillWithBlocks(world, bounds, 10, 6, 2, 10, 6, 10, Block.Fence.id, Block.Fence.id, false);
            FillWithBlocks(world, bounds, 4, 6, 2, 9, 6, 2, Block.Fence.id, Block.Fence.id, false);
            FillWithBlocks(world, bounds, 4, 6, 12, 8, 6, 12, Block.Fence.id, Block.Fence.id, false);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 9, 6, 11, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 8, 6, 11, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 9, 6, 10, bounds);
            int ladderMeta = GetMetadataWithOffset(Block.Ladder.id, 3);
            for (int y = 1; y <= 7; ++y)
            {
                PlaceBlockAtCurrentPosition(world, Block.Ladder.id, ladderMeta, 10, y, 13, bounds);
            }

            int fenceX = 7;
            int fenceZ = 7;
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX - 1, 9, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX, 9, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX - 1, 8, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX, 8, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX - 1, 7, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX, 7, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX - 2, 7, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX + 1, 7, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX - 1, 7, fenceZ - 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX - 1, 7, fenceZ + 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX, 7, fenceZ - 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, fenceX, 7, fenceZ + 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, fenceX - 2, 8, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, fenceX + 1, 8, fenceZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, fenceX - 1, 8, fenceZ - 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, fenceX - 1, 8, fenceZ + 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, fenceX, 8, fenceZ - 1, bounds);
            PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, fenceX, 8, fenceZ + 1, bounds);
        }

        CreateTreasureChestAtCurrentPosition(world, bounds, random, 3, 3, 5, s_chestLoot, 1 + random.NextInt(4));
        if (_isLargeRoom)
        {
            PlaceBlockAtCurrentPosition(world, 0, 0, 12, 9, 1, bounds);
            CreateTreasureChestAtCurrentPosition(world, bounds, random, 12, 8, 1, s_chestLoot, 1 + random.NextInt(4));
        }

        return true;
    }
}
