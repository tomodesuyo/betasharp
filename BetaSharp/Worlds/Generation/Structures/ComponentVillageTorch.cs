using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageTorch : ComponentVillage
{
    private int _averageGroundLevel = -1;

    public ComponentVillageTorch(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static StructureBoundingBox? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 3, 4, 2, facing);
        return StructureComponent.FindIntersecting(components, bounds) != null ? null : bounds;
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

            BoundingBox.Offset(0, _averageGroundLevel - BoundingBox.MaxY + 3, 0);
        }

        FillWithBlocks(world, bounds, 0, 0, 0, 2, 3, 1, 0, 0, false);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 0, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Wool.id, 15, 1, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 0, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 1, 3, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 2, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Torch.id, 0, 1, 3, -1, bounds);
        return true;
    }
}
