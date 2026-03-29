using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdPrison : ComponentStronghold
{
    private readonly StrongholdDoor _doorType;

    public ComponentStrongholdPrison(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
    }

    public static ComponentStrongholdPrison? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -1, 0, 9, 5, 11, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdPrison(componentType, random, bounds, facing) : null;
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

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 8, 4, 10, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 1, 1, 0);
        FillWithBlocks(world, bounds, 1, 1, 10, 3, 3, 10, 0, 0, false);
        FillWithRandomizedBlocks(world, bounds, 4, 1, 1, 4, 3, 1, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 4, 1, 3, 4, 3, 3, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 4, 1, 7, 4, 3, 7, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 4, 1, 9, 4, 3, 9, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithBlocks(world, bounds, 4, 1, 4, 4, 3, 6, Block.IronBars.id, Block.IronBars.id, false);
        FillWithBlocks(world, bounds, 5, 1, 5, 7, 3, 5, Block.IronBars.id, Block.IronBars.id, false);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 4, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 4, 3, 8, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronDoor.id, GetMetadataWithOffset(Block.IronDoor.id, 3), 4, 1, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronDoor.id, GetMetadataWithOffset(Block.IronDoor.id, 3) + 8, 4, 2, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronDoor.id, GetMetadataWithOffset(Block.IronDoor.id, 3), 4, 1, 8, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronDoor.id, GetMetadataWithOffset(Block.IronDoor.id, 3) + 8, 4, 2, 8, bounds);
        return true;
    }
}
