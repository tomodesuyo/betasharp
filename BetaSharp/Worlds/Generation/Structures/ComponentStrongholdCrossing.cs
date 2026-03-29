using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdCrossing : ComponentStronghold
{
    private readonly StrongholdDoor _doorType;
    private readonly bool _lowerLeft;
    private readonly bool _upperLeft;
    private readonly bool _lowerRight;
    private readonly bool _upperRight;

    public ComponentStrongholdCrossing(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
        _lowerLeft = random.NextBoolean();
        _upperLeft = random.NextBoolean();
        _lowerRight = random.NextBoolean();
        _upperRight = random.NextInt(3) > 0;
    }

    public static ComponentStrongholdCrossing? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -4, -3, 0, 10, 9, 11, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdCrossing(componentType, random, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        int left = 3;
        int right = 5;
        if (Facing == 1 || Facing == 2)
        {
            left = 8 - left;
            right = 8 - right;
        }

        GetNextComponentNormal((ComponentStrongholdStairs2)component, components, random, 5, 1);
        if (_lowerLeft)
        {
            GetNextComponentX((ComponentStrongholdStairs2)component, components, random, left, 1);
        }

        if (_upperLeft)
        {
            GetNextComponentX((ComponentStrongholdStairs2)component, components, random, right, 7);
        }

        if (_lowerRight)
        {
            GetNextComponentZ((ComponentStrongholdStairs2)component, components, random, left, 1);
        }

        if (_upperRight)
        {
            GetNextComponentZ((ComponentStrongholdStairs2)component, components, random, right, 7);
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 9, 8, 10, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 4, 3, 0);
        if (_lowerLeft)
        {
            FillWithBlocks(world, bounds, 0, 3, 1, 0, 5, 3, 0, 0, false);
        }

        if (_lowerRight)
        {
            FillWithBlocks(world, bounds, 9, 3, 1, 9, 5, 3, 0, 0, false);
        }

        if (_upperLeft)
        {
            FillWithBlocks(world, bounds, 0, 5, 7, 0, 7, 9, 0, 0, false);
        }

        if (_upperRight)
        {
            FillWithBlocks(world, bounds, 9, 5, 7, 9, 7, 9, 0, 0, false);
        }

        FillWithBlocks(world, bounds, 5, 1, 10, 7, 3, 10, 0, 0, false);
        FillWithRandomizedBlocks(world, bounds, 1, 2, 1, 8, 2, 6, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 4, 1, 5, 4, 4, 9, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 8, 1, 5, 8, 4, 9, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 1, 4, 7, 3, 4, 9, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 1, 3, 5, 3, 3, 6, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithBlocks(world, bounds, 1, 3, 4, 3, 3, 4, Block.Slab.id, Block.Slab.id, false);
        FillWithBlocks(world, bounds, 1, 4, 6, 3, 4, 6, Block.Slab.id, Block.Slab.id, false);
        FillWithRandomizedBlocks(world, bounds, 5, 1, 7, 7, 1, 8, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithBlocks(world, bounds, 5, 1, 9, 7, 1, 9, Block.Slab.id, Block.Slab.id, false);
        FillWithBlocks(world, bounds, 5, 2, 7, 7, 2, 7, Block.Slab.id, Block.Slab.id, false);
        FillWithBlocks(world, bounds, 4, 5, 7, 4, 5, 9, Block.Slab.id, Block.Slab.id, false);
        FillWithBlocks(world, bounds, 8, 5, 7, 8, 5, 9, Block.Slab.id, Block.Slab.id, false);
        FillWithBlocks(world, bounds, 5, 5, 7, 7, 5, 9, Block.DoubleSlab.id, Block.DoubleSlab.id, false);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 6, 5, 6, bounds);
        return true;
    }
}
