using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public class ComponentVillageWell : ComponentVillage
{
    private readonly bool _isWell = true;
    private int _averageGroundLevel = -1;

    public ComponentVillageWell(int componentType, JavaRandom random, int x, int z)
        : base(componentType)
    {
        Facing = random.NextInt(4);
        BoundingBox = new StructureBoundingBox(x, 64, z, x + 6 - 1, 78, z + 6 - 1);
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        ComponentVillageStartPiece startPiece = (ComponentVillageStartPiece)component;
        StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX - 1, BoundingBox.MaxY - 4, BoundingBox.MinZ + 1, 1, GetComponentType());
        StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MaxX + 1, BoundingBox.MaxY - 4, BoundingBox.MinZ + 1, 3, GetComponentType());
        StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX + 1, BoundingBox.MaxY - 4, BoundingBox.MinZ - 1, 2, GetComponentType());
        StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX + 1, BoundingBox.MaxY - 4, BoundingBox.MaxZ + 1, 0, GetComponentType());
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

        if (_isWell)
        {
        }

        FillWithBlocks(world, bounds, 1, 0, 1, 4, 12, 4, Block.Cobblestone.id, Block.FlowingWater.id, false);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 12, 2, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 3, 12, 2, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 2, 12, 3, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 3, 12, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 13, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 14, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 4, 13, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 4, 14, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 13, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 1, 14, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 4, 13, 4, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Fence.id, 0, 4, 14, 4, bounds);
        FillWithBlocks(world, bounds, 1, 15, 1, 4, 15, 4, Block.Cobblestone.id, Block.Cobblestone.id, false);

        for (int x = 0; x <= 5; ++x)
        {
            for (int z = 0; z <= 5; ++z)
            {
                if (z == 0 || z == 5 || x == 0 || x == 5)
                {
                    PlaceBlockAtCurrentPosition(world, Block.Gravel.id, 0, z, 11, x, bounds);
                    ClearCurrentPositionBlocksUpwards(world, z, 12, x, bounds);
                }
            }
        }

        return true;
    }
}
