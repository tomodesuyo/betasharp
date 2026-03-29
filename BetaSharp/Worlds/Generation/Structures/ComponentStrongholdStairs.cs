using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal class ComponentStrongholdStairs : ComponentStronghold
{
    private readonly bool _isSourcePiece;
    private readonly StrongholdDoor _doorType;

    public ComponentStrongholdStairs(int componentType, JavaRandom random, int x, int z) : base(componentType)
    {
        _isSourcePiece = true;
        Facing = random.NextInt(4);
        _doorType = StrongholdDoor.Opening;
        BoundingBox = new StructureBoundingBox(x, 64, z, x + 4, 74, z + 4);
    }

    public ComponentStrongholdStairs(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        _isSourcePiece = false;
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
    }

    public static ComponentStrongholdStairs? GetStrongholdStairsComponent(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -7, 0, 5, 11, 5, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdStairs(componentType, random, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        if (_isSourcePiece)
        {
            StructureStrongholdPieces.SetComponentType(typeof(ComponentStrongholdCrossing));
        }

        GetNextComponentNormal((ComponentStrongholdStairs2)component, components, random, 1, 1);
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 4, 10, 4, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 1, 7, 0);
        PlaceDoor(world, random, bounds, StrongholdDoor.Opening, 1, 1, 4);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 6, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 5, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 6, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 5, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 5, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 3, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 3, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 3, 3, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 1, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 1, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 1, 3, bounds);
        return true;
    }
}
