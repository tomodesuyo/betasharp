using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdStraight : ComponentStronghold
{
    private readonly StrongholdDoor _doorType;
    private readonly bool _expandsX;
    private readonly bool _expandsZ;

    public ComponentStrongholdStraight(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        _doorType = GetRandomDoor(random);
        BoundingBox = bounds;
        _expandsX = random.NextInt(2) == 0;
        _expandsZ = random.NextInt(2) == 0;
    }

    public static ComponentStrongholdStraight? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -1, 0, 5, 5, 7, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdStraight(componentType, random, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        GetNextComponentNormal((ComponentStrongholdStairs2)component, components, random, 1, 1);
        if (_expandsX)
        {
            GetNextComponentX((ComponentStrongholdStairs2)component, components, random, 1, 2);
        }

        if (_expandsZ)
        {
            GetNextComponentZ((ComponentStrongholdStairs2)component, components, random, 1, 2);
        }
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
        RandomlyPlaceBlock(world, bounds, random, 0.1F, 1, 2, 1, Block.Torch.id, 0);
        RandomlyPlaceBlock(world, bounds, random, 0.1F, 3, 2, 1, Block.Torch.id, 0);
        RandomlyPlaceBlock(world, bounds, random, 0.1F, 1, 2, 5, Block.Torch.id, 0);
        RandomlyPlaceBlock(world, bounds, random, 0.1F, 3, 2, 5, Block.Torch.id, 0);

        if (_expandsX)
        {
            FillWithBlocks(world, bounds, 0, 1, 2, 0, 3, 4, 0, 0, false);
        }

        if (_expandsZ)
        {
            FillWithBlocks(world, bounds, 4, 1, 2, 4, 3, 4, 0, 0, false);
        }

        return true;
    }
}
