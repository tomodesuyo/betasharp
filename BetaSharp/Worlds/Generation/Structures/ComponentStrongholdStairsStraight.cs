using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdStairsStraight : ComponentStronghold
{
    private readonly StrongholdDoor _doorType;

    public ComponentStrongholdStairsStraight(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
    }

    public static ComponentStrongholdStairsStraight? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -7, 0, 5, 11, 8, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdStairsStraight(componentType, random, bounds, facing) : null;
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

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 4, 10, 7, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, _doorType, 1, 7, 0);
        PlaceDoor(world, random, bounds, StrongholdDoor.Opening, 1, 1, 7);
        int stairsMeta = GetMetadataWithOffset(Block.CobblestoneStairs.id, 2);

        for (int i = 0; i < 6; ++i)
        {
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, stairsMeta, 1, 6 - i, 1 + i, bounds);
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, stairsMeta, 2, 6 - i, 1 + i, bounds);
            PlaceBlockAtCurrentPosition(world, Block.CobblestoneStairs.id, stairsMeta, 3, 6 - i, 1 + i, bounds);
            if (i < 5)
            {
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 5 - i, 1 + i, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 5 - i, 1 + i, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 5 - i, 1 + i, bounds);
            }
        }

        return true;
    }
}
