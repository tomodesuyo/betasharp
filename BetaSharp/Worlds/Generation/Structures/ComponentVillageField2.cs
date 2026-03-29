using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillageField2 : ComponentVillage
{
    private int _averageGroundLevel = -1;
    private readonly int _cropIdA;
    private readonly int _cropIdB;

    public ComponentVillageField2(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
        _cropIdA = PickCrop(random);
        _cropIdB = PickCrop(random);
    }

    public static ComponentVillageField2? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 7, 4, 9, facing);
        return CanVillageGoDeeper(bounds) && StructureComponent.FindIntersecting(components, bounds) == null
            ? new ComponentVillageField2(componentType, random, bounds, facing)
            : null;
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

        FillWithBlocks(world, bounds, 0, 1, 0, 6, 4, 8, 0, 0, false);
        FillWithBlocks(world, bounds, 1, 0, 1, 2, 0, 7, Block.Farmland.id, Block.Farmland.id, false);
        FillWithBlocks(world, bounds, 4, 0, 1, 5, 0, 7, Block.Farmland.id, Block.Farmland.id, false);
        FillWithBlocks(world, bounds, 0, 0, 0, 0, 0, 8, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 6, 0, 0, 6, 0, 8, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 1, 0, 0, 5, 0, 0, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 1, 0, 8, 5, 0, 8, Block.Log.id, Block.Log.id, false);
        FillWithBlocks(world, bounds, 3, 0, 1, 3, 0, 7, Block.FlowingWater.id, Block.FlowingWater.id, false);

        for (int z = 1; z <= 7; ++z)
        {
            PlaceRandomCrop(world, random, _cropIdA, 1, 1, z, bounds);
            PlaceRandomCrop(world, random, _cropIdA, 2, 1, z, bounds);
            PlaceRandomCrop(world, random, _cropIdB, 4, 1, z, bounds);
            PlaceRandomCrop(world, random, _cropIdB, 5, 1, z, bounds);
        }

        for (int z = 0; z < 9; ++z)
        {
            for (int x = 0; x < 7; ++x)
            {
                ClearCurrentPositionBlocksUpwards(world, x, 4, z, bounds);
                FillCurrentPositionBlocksDownwards(world, Block.Dirt.id, 0, x, -1, z, bounds);
            }
        }

        return true;
    }

    private static int PickCrop(JavaRandom random)
    {
        return random.NextInt(5) switch
        {
            0 => Block.Carrot.id,
            1 => Block.Potato.id,
            _ => Block.Wheat.id,
        };
    }

    private void PlaceRandomCrop(IWorldContext world, JavaRandom random, int cropId, int x, int y, int z, StructureBoundingBox bounds)
    {
        PlaceBlockAtCurrentPosition(world, cropId, 2 + random.NextInt(6), x, y, z, bounds);
    }
}
