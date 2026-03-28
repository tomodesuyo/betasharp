using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentMineshaftCorridor : StructureComponent
{
    private readonly bool _hasRails;
    private readonly bool _hasSpawner;
    private bool _spawnerPlaced;
    private readonly int _sectionCount;

    public ComponentMineshaftCorridor(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
        _hasRails = random.NextInt(3) == 0;
        _hasSpawner = !_hasRails && random.NextInt(23) == 0;
        _sectionCount = facing is 2 or 0 ? bounds.GetZSize() / 5 : bounds.GetXSize() / 5;
    }

    public static StructureBoundingBox? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing)
    {
        StructureBoundingBox bounds = new(x, y, z, x, y + 2, z);

        int sections;
        for (sections = random.NextInt(3) + 2; sections > 0; --sections)
        {
            int length = sections * 5;
            switch (facing)
            {
                case 0:
                    bounds.MaxX = x + 2;
                    bounds.MaxZ = z + (length - 1);
                    break;
                case 1:
                    bounds.MinX = x - (length - 1);
                    bounds.MaxZ = z + 2;
                    break;
                case 2:
                    bounds.MaxX = x + 2;
                    bounds.MinZ = z - (length - 1);
                    break;
                case 3:
                    bounds.MaxX = x + (length - 1);
                    bounds.MaxZ = z + 2;
                    break;
            }

            if (FindIntersecting(components, bounds) == null)
            {
                break;
            }
        }

        return sections > 0 ? bounds : null;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        int depth = GetComponentType();
        int branch = random.NextInt(4);
        switch (Facing)
        {
            case 0:
                if (branch <= 1)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MaxZ + 1, Facing, depth);
                }
                else if (branch == 2)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MaxZ - 3, 1, depth);
                }
                else
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MaxZ - 3, 3, depth);
                }
                break;
            case 1:
                if (branch <= 1)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ, Facing, depth);
                }
                else if (branch == 2)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ - 1, 2, depth);
                }
                else
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MaxZ + 1, 0, depth);
                }
                break;
            case 2:
                if (branch <= 1)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ - 1, Facing, depth);
                }
                else if (branch == 2)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ, 1, depth);
                }
                else
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ, 3, depth);
                }
                break;
            case 3:
                if (branch <= 1)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ, Facing, depth);
                }
                else if (branch == 2)
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX - 3, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MinZ - 1, 2, depth);
                }
                else
                {
                    StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX - 3, BoundingBox.MinY - 1 + random.NextInt(3), BoundingBox.MaxZ + 1, 0, depth);
                }
                break;
        }

        if (depth < 8)
        {
            if (Facing is not 2 and not 0)
            {
                for (int x = BoundingBox.MinX + 3; x + 3 <= BoundingBox.MaxX; x += 5)
                {
                    int value = random.NextInt(5);
                    if (value == 0)
                    {
                        StructureMineshaftPieces.GetNextComponent(component, components, random, x, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, depth + 1);
                    }
                    else if (value == 1)
                    {
                        StructureMineshaftPieces.GetNextComponent(component, components, random, x, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, depth + 1);
                    }
                }
            }
            else
            {
                for (int z = BoundingBox.MinZ + 3; z + 3 <= BoundingBox.MaxZ; z += 5)
                {
                    int value = random.NextInt(5);
                    if (value == 0)
                    {
                        StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, z, 1, depth + 1);
                    }
                    else if (value == 1)
                    {
                        StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, z, 3, depth + 1);
                    }
                }
            }
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        int maxZ = _sectionCount * 5 - 1;
        FillWithBlocks(world, bounds, 0, 0, 0, 2, 1, maxZ, 0, 0, false);
        FillWithRandomizedBlocks(world, bounds, random, 0.8F, 0, 2, 0, 2, 2, maxZ, 0, 0, false);
        if (_hasSpawner)
        {
            FillWithRandomizedBlocks(world, bounds, random, 0.6F, 0, 0, 0, 2, 1, maxZ, Block.Cobweb.id, 0, false);
        }

        for (int section = 0; section < _sectionCount; ++section)
        {
            int z = 2 + section * 5;
            FillWithBlocks(world, bounds, 0, 0, z, 0, 1, z, Block.Fence.id, 0, false);
            FillWithBlocks(world, bounds, 2, 0, z, 2, 1, z, Block.Fence.id, 0, false);
            if (random.NextInt(4) != 0)
            {
                FillWithBlocks(world, bounds, 0, 2, z, 2, 2, z, Block.Planks.id, 0, false);
            }
            else
            {
                FillWithBlocks(world, bounds, 0, 2, z, 0, 2, z, Block.Planks.id, 0, false);
                FillWithBlocks(world, bounds, 2, 2, z, 2, 2, z, Block.Planks.id, 0, false);
            }

            RandomlyPlaceBlock(world, bounds, random, 0.1F, 0, 2, z - 1, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.1F, 2, 2, z - 1, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.1F, 0, 2, z + 1, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.1F, 2, 2, z + 1, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.05F, 0, 2, z - 2, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.05F, 2, 2, z - 2, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.05F, 0, 2, z + 2, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.05F, 2, 2, z + 2, Block.Cobweb.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.05F, 1, 2, z - 1, Block.Torch.id, 0);
            RandomlyPlaceBlock(world, bounds, random, 0.05F, 1, 2, z + 1, Block.Torch.id, 0);

            if (random.NextInt(100) == 0)
            {
                CreateTreasureChestAtCurrentPosition(world, bounds, random, 2, 0, z - 1, StructureMineshaftPieces.GetTreasurePieces(), 3 + random.NextInt(4));
            }

            if (random.NextInt(100) == 0)
            {
                CreateTreasureChestAtCurrentPosition(world, bounds, random, 0, 0, z + 1, StructureMineshaftPieces.GetTreasurePieces(), 3 + random.NextInt(4));
            }

            if (_hasSpawner && !_spawnerPlaced)
            {
                int worldY = GetYWithOffset(0);
                int spawnerZ = z - 1 + random.NextInt(3);
                int worldX = GetXWithOffset(1, spawnerZ);
                int worldZ = GetZWithOffset(1, spawnerZ);
                if (bounds.Contains(worldX, worldY, worldZ))
                {
                    _spawnerPlaced = true;
                    world.Writer.SetBlock(worldX, worldY, worldZ, Block.Spawner.id, 0, true);
                    BlockEntityMobSpawner? spawner = world.Entities.GetBlockEntity<BlockEntityMobSpawner>(worldX, worldY, worldZ);
                    if (spawner != null)
                    {
                        spawner.SetSpawnedEntityId("Spider");
                    }
                }
            }
        }

        if (_hasRails)
        {
            for (int z = 0; z <= maxZ; ++z)
            {
                int blockId = GetBlockIdAtCurrentPosition(world, 1, -1, z, bounds);
                if (blockId > 0 && Block.Blocks[blockId].isOpaque())
                {
                    RandomlyPlaceBlock(world, bounds, random, 0.7F, 1, 0, z, Block.Rail.id, GetMetadataWithOffset(Block.Rail.id, 0));
                }
            }
        }

        return true;
    }
}
