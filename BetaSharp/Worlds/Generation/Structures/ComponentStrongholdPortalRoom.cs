using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdPortalRoom : ComponentStronghold
{
    private bool _hasSpawner;

    public ComponentStrongholdPortalRoom(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static ComponentStrongholdPortalRoom? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int componentType)
    {
        StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, -4, -1, 0, 11, 8, 16, facing);
        return CanStrongholdGoDeeper(bounds) && FindIntersecting(components, bounds) == null ? new ComponentStrongholdPortalRoom(componentType, random, bounds, facing) : null;
    }

    public static ComponentStrongholdPortalRoom? Create(int x, int y, int z, int facing, int componentType, List<StructureComponent> components)
    {
        return FindValidPlacement(components, new JavaRandom(0L), x, y, z, facing, componentType);
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
        FillWithRandomizedBlocks(world, bounds, 0, 0, 0, 10, 7, 15, false, random, StructureStrongholdPieces.GetStrongholdStones());
        PlaceDoor(world, random, bounds, StrongholdDoor.Grates, 4, 1, 0);
        const int ceilingY = 6;
        FillWithRandomizedBlocks(world, bounds, 1, ceilingY, 1, 1, ceilingY, 14, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 9, ceilingY, 1, 9, ceilingY, 14, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 2, ceilingY, 1, 8, ceilingY, 2, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 2, ceilingY, 14, 8, ceilingY, 14, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 1, 1, 1, 2, 1, 4, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 8, 1, 1, 9, 1, 4, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithBlocks(world, bounds, 1, 1, 1, 1, 1, 3, Block.FlowingLava.id, Block.FlowingLava.id, false);
        FillWithBlocks(world, bounds, 9, 1, 1, 9, 1, 3, Block.FlowingLava.id, Block.FlowingLava.id, false);
        FillWithRandomizedBlocks(world, bounds, 3, 1, 8, 7, 1, 12, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithBlocks(world, bounds, 4, 1, 9, 6, 1, 11, Block.FlowingLava.id, Block.FlowingLava.id, false);

        for (int z = 3; z < 14; z += 2)
        {
            FillWithBlocks(world, bounds, 0, 3, z, 0, 4, z, Block.IronBars.id, Block.IronBars.id, false);
            FillWithBlocks(world, bounds, 10, 3, z, 10, 4, z, Block.IronBars.id, Block.IronBars.id, false);
        }

        for (int x = 2; x < 9; x += 2)
        {
            FillWithBlocks(world, bounds, x, 3, 15, x, 4, 15, Block.IronBars.id, Block.IronBars.id, false);
        }

        int stairsMeta = GetMetadataWithOffset(Block.StoneBrickStairs.id, 3);
        FillWithRandomizedBlocks(world, bounds, 4, 1, 5, 6, 1, 7, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 4, 2, 6, 6, 2, 7, false, random, StructureStrongholdPieces.GetStrongholdStones());
        FillWithRandomizedBlocks(world, bounds, 4, 3, 7, 6, 3, 7, false, random, StructureStrongholdPieces.GetStrongholdStones());

        for (int x = 4; x <= 6; ++x)
        {
            PlaceBlockAtCurrentPosition(world, Block.StoneBrickStairs.id, stairsMeta, x, 1, 4, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrickStairs.id, stairsMeta, x, 2, 5, bounds);
            PlaceBlockAtCurrentPosition(world, Block.StoneBrickStairs.id, stairsMeta, x, 3, 6, bounds);
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

        for (int x = 4; x <= 6; ++x)
        {
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, north + (random.NextFloat() > 0.9F ? 4 : 0), x, 3, 8, bounds);
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, south + (random.NextFloat() > 0.9F ? 4 : 0), x, 3, 12, bounds);
        }

        for (int z = 9; z <= 11; ++z)
        {
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, west + (random.NextFloat() > 0.9F ? 4 : 0), 3, 3, z, bounds);
            PlaceBlockAtCurrentPosition(world, Block.EndPortalFrame.id, east + (random.NextFloat() > 0.9F ? 4 : 0), 7, 3, z, bounds);
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
