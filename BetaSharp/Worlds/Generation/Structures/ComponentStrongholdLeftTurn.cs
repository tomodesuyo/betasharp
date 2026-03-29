using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal class ComponentStrongholdLeftTurn : ComponentStronghold
{
    protected readonly StrongholdDoor DoorType;

    public ComponentStrongholdLeftTurn(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        DoorType = GetRandomDoor(random);
        BoundingBox = bounds;
    }

    public static ComponentStrongholdLeftTurn? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -1, 0, 5, 5, 5, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdLeftTurn(componentType, random, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        if (Facing is not 2 and not 3)
        {
            GetNextComponentZ((ComponentStrongholdStairs2)component, components, random, 1, 1);
        }
        else
        {
            GetNextComponentX((ComponentStrongholdStairs2)component, components, random, 1, 1);
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 4, 4, 4, true, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, DoorType, 1, 1, 0);
        if (Facing is not 2 and not 3)
        {
            FillWithBlocks(world, bounds, 4, 1, 1, 4, 3, 3, 0, 0, false);
        }
        else
        {
            FillWithBlocks(world, bounds, 0, 1, 1, 0, 3, 3, 0, 0, false);
        }

        return true;
    }
}
