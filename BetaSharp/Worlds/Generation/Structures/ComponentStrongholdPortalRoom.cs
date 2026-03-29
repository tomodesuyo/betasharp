using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdPortalRoom : StructureComponent
{
    private bool _hasSpawner;

    public ComponentStrongholdPortalRoom(int componentType, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentStrongholdPortalRoom? Create(int x, int y, int z, int facing, int componentType, List<StructureComponent> components)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -4, -1, 0, 11, 8, 16, facing);
        return bounds.MinY > 10 && FindIntersecting(components, bounds) == null ? new ComponentStrongholdPortalRoom(componentType, bounds, facing) : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        if (component is ComponentStrongholdStairs2 startPiece)
        {
            startPiece.PortalRoom = this;
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithBlocks(world, bounds, 0, 0, 0, 10, 7, 15, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 1, 1, 1, 9, 6, 14, 0, 0, false);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 4, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 4, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 4, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 5, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, 0, 0, 5, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 5, 3, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 6, 1, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 6, 2, 0, bounds);
        PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, 6, 3, 0, bounds);

        int ceilingY = 6;
        FillWithBlocks(world, bounds, 1, ceilingY, 1, 1, ceilingY, 14, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 9, ceilingY, 1, 9, ceilingY, 14, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 2, ceilingY, 1, 8, ceilingY, 2, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 2, ceilingY, 14, 8, ceilingY, 14, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 1, 1, 1, 2, 1, 4, Block.Lava.id, 0, false);
        FillWithBlocks(world, bounds, 8, 1, 1, 9, 1, 4, Block.Lava.id, 0, false);
        FillWithBlocks(world, bounds, 3, 1, 8, 7, 1, 12, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 4, 1, 9, 6, 1, 11, Block.Lava.id, 0, false);

        for (int localZ = 3; localZ < 14; localZ += 2)
        {
            FillWithBlocks(world, bounds, 0, 3, localZ, 0, 4, localZ, Block.IronBars.id, 0, false);
            FillWithBlocks(world, bounds, 10, 3, localZ, 10, 4, localZ, Block.IronBars.id, 0, false);
        }

        for (int localX = 2; localX < 9; localX += 2)
        {
            FillWithBlocks(world, bounds, localX, 3, 15, localX, 4, 15, Block.IronBars.id, 0, false);
        }

        int stairsMeta = GetMetadataWithOffset(Block.StoneBrickStairs.id, 3);
        FillWithBlocks(world, bounds, 4, 1, 5, 6, 1, 7, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 4, 2, 6, 6, 2, 7, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 4, 3, 7, 6, 3, 7, Block.StoneBrick.id, 0, false);
        for (int localX = 4; localX <= 6; ++localX)
        {
            PlaceBlockAtCurrentPosition(world, Block.StoneBrickStairs.id, stairsMeta, localX, 1, 4, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrickStairs.id, stairsMeta, localX, 2, 5, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrickStairs.id, stairsMeta, localX, 3, 6, bounds);
        }

        int north = 2;
        int south = 0;
        int west = 3;
        int east = 1;
        switch (Facing)
        {
            case 0:
                north = 0;
                south = 2;
                break;
            case 1:
                north = 1;
                south = 3;
                west = 0;
                east = 2;
                break;
            case 3:
                north = 3;
                south = 1;
                west = 0;
                east = 2;
                break;
        }

        for (int localX = 4; localX <= 6; ++localX)
        {
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, north + (random.NextFloat() > 0.9F ? 4 : 0), localX, 3, 8, bounds);
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, south + (random.NextFloat() > 0.9F ? 4 : 0), localX, 3, 12, bounds);
        }

        for (int localZ = 9; localZ <= 11; ++localZ)
        {
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, west + (random.NextFloat() > 0.9F ? 4 : 0), 3, 3, localZ, bounds);
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, east + (random.NextFloat() > 0.9F ? 4 : 0), 7, 3, localZ, bounds);
        }

        if (!_hasSpawner)
        {
            int spawnerY = GetYWithOffset(3);
            int spawnerX = GetXWithOffset(5, 6);
            int spawnerZ = GetZWithOffset(5, 6);
            if (bounds.Contains(spawnerX, spawnerY, spawnerZ))
            {
                _hasSpawner = true;
                world.Writer.SetBlockWithoutNotifyingNeighbors(spawnerX, spawnerY, spawnerZ, Block.Spawner.id, 0, false);
                if (world.Entities.GetBlockEntity<BlockEntityMobSpawner>(spawnerX, spawnerY, spawnerZ) is { } spawner)
                {
                    spawner.SetSpawnedEntityId("Silverfish");
                }
            }
        }

        return true;
    }
}
