using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdCorridor : ComponentStronghold
{
    private readonly int _length;

    public ComponentStrongholdCorridor(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
        _length = facing is not 0 and not 2 ? bounds.GetXSize() : bounds.GetZSize();
    }

    public static StructureBoundingBox? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -1, -1, 0, 5, 5, 4, facing);
        StructureComponent? intersecting = FindIntersecting(components, bounds);
        if (intersecting == null)
        {
            return null;
        }

        if (intersecting.GetBoundingBox().MinY == bounds.MinY)
        {
            for (int length = 3; length >= 1; --length)
            {
                bounds = StructureBoundingBox.Create(x, y, z, -1, -1, 0, 5, 5, length - 1, facing);
                if (!intersecting.GetBoundingBox().Intersects(bounds))
                {
                    return StructureBoundingBox.Create(x, y, z, -1, -1, 0, 5, 5, length, facing);
                }
            }
        }

        return null;
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        for (int z = 0; z < _length; ++z)
        {
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 0, 0, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 0, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 0, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 0, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 4, 0, z, bounds);

            for (int y = 1; y <= 3; ++y)
            {
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 0, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, 0, 0, 1, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, 0, 0, 2, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, 0, 0, 3, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 4, y, z, bounds);
            }

            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 0, 4, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 4, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 4, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 4, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 4, 4, z, bounds);
        }

        return true;
    }
}
