using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public abstract class StructureComponent
{
    protected StructureBoundingBox BoundingBox = null!;
    protected int Facing = -1;
    protected int ComponentType;

    protected StructureComponent(int componentType)
    {
        ComponentType = componentType;
    }

    public virtual void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
    }

    public abstract bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds);

    public StructureBoundingBox GetBoundingBox() => BoundingBox;

    public int GetComponentType() => ComponentType;

    public static StructureComponent? FindIntersecting(List<StructureComponent> components, StructureBoundingBox bounds)
    {
        for (int i = 0; i < components.Count; ++i)
        {
            StructureComponent component = components[i];
            if (component.GetBoundingBox() != null && component.GetBoundingBox().Intersects(bounds))
            {
                return component;
            }
        }

        return null;
    }

    protected bool IsLiquidInStructureBoundingBox(IWorldContext world, StructureBoundingBox bounds)
    {
        int minX = Math.Max(BoundingBox.MinX - 1, bounds.MinX);
        int minY = Math.Max(BoundingBox.MinY - 1, bounds.MinY);
        int minZ = Math.Max(BoundingBox.MinZ - 1, bounds.MinZ);
        int maxX = Math.Min(BoundingBox.MaxX + 1, bounds.MaxX);
        int maxY = Math.Min(BoundingBox.MaxY + 1, bounds.MaxY);
        int maxZ = Math.Min(BoundingBox.MaxZ + 1, bounds.MaxZ);

        for (int x = minX; x <= maxX; ++x)
        {
            for (int z = minZ; z <= maxZ; ++z)
            {
                int blockId = world.Reader.GetBlockId(x, minY, z);
                if (blockId > 0 && Block.Blocks[blockId].material.IsFluid)
                {
                    return true;
                }

                blockId = world.Reader.GetBlockId(x, maxY, z);
                if (blockId > 0 && Block.Blocks[blockId].material.IsFluid)
                {
                    return true;
                }
            }
        }

        for (int x = minX; x <= maxX; ++x)
        {
            for (int y = minY; y <= maxY; ++y)
            {
                int blockId = world.Reader.GetBlockId(x, y, minZ);
                if (blockId > 0 && Block.Blocks[blockId].material.IsFluid)
                {
                    return true;
                }

                blockId = world.Reader.GetBlockId(x, y, maxZ);
                if (blockId > 0 && Block.Blocks[blockId].material.IsFluid)
                {
                    return true;
                }
            }
        }

        for (int z = minZ; z <= maxZ; ++z)
        {
            for (int y = minY; y <= maxY; ++y)
            {
                int blockId = world.Reader.GetBlockId(minX, y, z);
                if (blockId > 0 && Block.Blocks[blockId].material.IsFluid)
                {
                    return true;
                }

                blockId = world.Reader.GetBlockId(maxX, y, z);
                if (blockId > 0 && Block.Blocks[blockId].material.IsFluid)
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected int GetXWithOffset(int x, int z)
    {
        return Facing switch
        {
            0 or 2 => BoundingBox.MinX + x,
            1 => BoundingBox.MaxX - z,
            3 => BoundingBox.MinX + z,
            _ => x,
        };
    }

    protected int GetYWithOffset(int y)
    {
        return Facing == -1 ? y : y + BoundingBox.MinY;
    }

    protected int GetZWithOffset(int x, int z)
    {
        return Facing switch
        {
            0 => BoundingBox.MinZ + z,
            1 or 3 => BoundingBox.MinZ + x,
            2 => BoundingBox.MaxZ - z,
            _ => z,
        };
    }

    protected int GetMetadataWithOffset(int blockId, int meta)
    {
        if (blockId == Block.Rail.id)
        {
            if (Facing == 1 || Facing == 3)
            {
                return meta == 1 ? 0 : 1;
            }
        }
        else if (blockId == Block.Door.id || blockId == Block.IronDoor.id)
        {
            if (Facing == 0)
            {
                if (meta == 0) return 2;
                if (meta == 2) return 0;
            }
            else
            {
                if (Facing == 1) return meta + 1 & 3;
                if (Facing == 3) return meta + 3 & 3;
            }
        }
        else
        {
            if (blockId == Block.CobblestoneStairs.id || blockId == Block.WoodenStairs.id || blockId == Block.StoneBrickStairs.id || blockId == Block.NetherBrickStairs.id || blockId == Block.SandstoneStairs.id)
            {
                if (Facing == 0)
                {
                    if (meta == 2) return 3;
                    if (meta == 3) return 2;
                }
                else if (Facing == 1)
                {
                    if (meta == 0) return 2;
                    if (meta == 1) return 3;
                    if (meta == 2) return 0;
                    if (meta == 3) return 1;
                }
                else if (Facing == 3)
                {
                    if (meta == 0) return 2;
                    if (meta == 1) return 3;
                    if (meta == 2) return 1;
                    if (meta == 3) return 0;
                }
            }
            else if (blockId == Block.Ladder.id)
            {
                if (Facing == 0)
                {
                    if (meta == 2) return 3;
                    if (meta == 3) return 2;
                }
                else if (Facing == 1)
                {
                    if (meta == 2) return 4;
                    if (meta == 3) return 5;
                    if (meta == 4) return 2;
                    if (meta == 5) return 3;
                }
                else if (Facing == 3)
                {
                    if (meta == 2) return 5;
                    if (meta == 3) return 4;
                    if (meta == 4) return 2;
                    if (meta == 5) return 3;
                }
            }
            else if (blockId == Block.Button.id)
            {
                if (Facing == 0)
                {
                    if (meta == 3) return 4;
                    if (meta == 4) return 3;
                }
                else if (Facing == 1)
                {
                    if (meta == 3) return 1;
                    if (meta == 4) return 2;
                    if (meta == 2) return 3;
                    if (meta == 1) return 4;
                }
                else if (Facing == 3)
                {
                    if (meta == 3) return 2;
                    if (meta == 4) return 1;
                    if (meta == 2) return 3;
                    if (meta == 1) return 4;
                }
            }
        }

        return meta;
    }

    protected virtual int GetStructureBlockId(int blockId, int meta) => blockId;

    protected virtual int GetStructureBlockMeta(int blockId, int meta) => meta;

    protected void PlaceBlockAtCurrentPosition(IWorldContext world, int blockId, int meta, int x, int y, int z, StructureBoundingBox bounds)
    {
        int worldX = GetXWithOffset(x, z);
        int worldY = GetYWithOffset(y);
        int worldZ = GetZWithOffset(x, z);
        if (bounds.Contains(worldX, worldY, worldZ))
        {
            int placedBlockId = GetStructureBlockId(blockId, meta);
            int placedMeta = GetStructureBlockMeta(blockId, meta);
            if (placedBlockId == Block.Torch.id && placedMeta == 0)
            {
                placedMeta = ResolveStructureTorchMetadata(world.Reader, worldX, worldY, worldZ);
            }

            world.Writer.SetBlockWithoutNotifyingNeighbors(worldX, worldY, worldZ, placedBlockId, placedMeta, false);
        }
    }

    private static int ResolveStructureTorchMetadata(IBlockReader reader, int x, int y, int z)
    {
        if (reader.ShouldSuffocate(x - 1, y, z))
        {
            return 1;
        }

        if (reader.ShouldSuffocate(x + 1, y, z))
        {
            return 2;
        }

        if (reader.ShouldSuffocate(x, y, z - 1))
        {
            return 3;
        }

        if (reader.ShouldSuffocate(x, y, z + 1))
        {
            return 4;
        }

        if (reader.ShouldSuffocate(x, y - 1, z) || reader.GetBlockId(x, y - 1, z) == Block.Fence.id)
        {
            return 5;
        }

        return 5;
    }

    protected int GetBlockIdAtCurrentPosition(IWorldContext world, int x, int y, int z, StructureBoundingBox bounds)
    {
        int worldX = GetXWithOffset(x, z);
        int worldY = GetYWithOffset(y);
        int worldZ = GetZWithOffset(x, z);
        return bounds.Contains(worldX, worldY, worldZ) ? world.Reader.GetBlockId(worldX, worldY, worldZ) : 0;
    }

    protected void FillWithBlocks(IWorldContext world, StructureBoundingBox bounds, int minX, int minY, int minZ, int maxX, int maxY, int maxZ, int boundaryBlockId, int insideBlockId, bool existingOnly)
    {
        for (int y = minY; y <= maxY; ++y)
        {
            for (int x = minX; x <= maxX; ++x)
            {
                for (int z = minZ; z <= maxZ; ++z)
                {
                    if (!existingOnly || GetBlockIdAtCurrentPosition(world, x, y, z, bounds) != 0)
                    {
                        PlaceBlockAtCurrentPosition(world, y != minY && y != maxY && x != minX && x != maxX && z != minZ && z != maxZ ? insideBlockId : boundaryBlockId, 0, x, y, z, bounds);
                    }
                }
            }
        }
    }

    protected void FillWithRandomizedBlocks(IWorldContext world, StructureBoundingBox bounds, JavaRandom random, float chance, int minX, int minY, int minZ, int maxX, int maxY, int maxZ, int boundaryBlockId, int insideBlockId, bool existingOnly)
    {
        for (int y = minY; y <= maxY; ++y)
        {
            for (int x = minX; x <= maxX; ++x)
            {
                for (int z = minZ; z <= maxZ; ++z)
                {
                    if (random.NextFloat() <= chance && (!existingOnly || GetBlockIdAtCurrentPosition(world, x, y, z, bounds) != 0))
                    {
                        PlaceBlockAtCurrentPosition(world, y != minY && y != maxY && x != minX && x != maxX && z != minZ && z != maxZ ? insideBlockId : boundaryBlockId, 0, x, y, z, bounds);
                    }
                }
            }
        }
    }

    protected void FillWithRandomizedBlocks(IWorldContext world, StructureBoundingBox bounds, int minX, int minY, int minZ, int maxX, int maxY, int maxZ, bool existingOnly, JavaRandom random, StructurePieceBlockSelector selector)
    {
        for (int y = minY; y <= maxY; ++y)
        {
            for (int x = minX; x <= maxX; ++x)
            {
                for (int z = minZ; z <= maxZ; ++z)
                {
                    if (existingOnly && GetBlockIdAtCurrentPosition(world, x, y, z, bounds) == 0)
                    {
                        continue;
                    }

                    bool wall = y == minY || y == maxY || x == minX || x == maxX || z == minZ || z == maxZ;
                    selector.SelectBlocks(random, x, y, z, wall);
                    PlaceBlockAtCurrentPosition(world, selector.SelectedBlockId, selector.SelectedBlockMeta, x, y, z, bounds);
                }
            }
        }
    }

    protected void RandomlyPlaceBlock(IWorldContext world, StructureBoundingBox bounds, JavaRandom random, float chance, int x, int y, int z, int blockId, int meta)
    {
        if (random.NextFloat() < chance)
        {
            PlaceBlockAtCurrentPosition(world, blockId, meta, x, y, z, bounds);
        }
    }

    protected void RandomlyFillSphere(IWorldContext world, StructureBoundingBox bounds, int minX, int minY, int minZ, int maxX, int maxY, int maxZ, int blockId, bool existingOnly)
    {
        float sizeX = maxX - minX + 1;
        float sizeY = maxY - minY + 1;
        float sizeZ = maxZ - minZ + 1;
        float centerX = minX + sizeX / 2.0F;
        float centerZ = minZ + sizeZ / 2.0F;

        for (int y = minY; y <= maxY; ++y)
        {
            float normY = (y - minY) / sizeY;
            for (int x = minX; x <= maxX; ++x)
            {
                float normX = (x - centerX) / (sizeX * 0.5F);
                for (int z = minZ; z <= maxZ; ++z)
                {
                    float normZ = (z - centerZ) / (sizeZ * 0.5F);
                    if ((!existingOnly || GetBlockIdAtCurrentPosition(world, x, y, z, bounds) != 0) && normX * normX + normY * normY + normZ * normZ <= 1.05F)
                    {
                        PlaceBlockAtCurrentPosition(world, blockId, 0, x, y, z, bounds);
                    }
                }
            }
        }
    }

    protected void ClearCurrentPositionBlocksUpwards(IWorldContext world, int x, int y, int z, StructureBoundingBox bounds)
    {
        int worldX = GetXWithOffset(x, z);
        int worldY = GetYWithOffset(y);
        int worldZ = GetZWithOffset(x, z);
        if (!bounds.Contains(worldX, worldY, worldZ))
        {
            return;
        }

        while (!world.Reader.IsAir(worldX, worldY, worldZ) && worldY < 127)
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(worldX, worldY, worldZ, 0, 0, false);
            ++worldY;
        }
    }

    protected void FillCurrentPositionBlocksDownwards(IWorldContext world, int blockId, int meta, int x, int y, int z, StructureBoundingBox bounds)
    {
        int worldX = GetXWithOffset(x, z);
        int worldY = GetYWithOffset(y);
        int worldZ = GetZWithOffset(x, z);
        if (!bounds.Contains(worldX, worldY, worldZ))
        {
            return;
        }

        while ((world.Reader.IsAir(worldX, worldY, worldZ) || world.Reader.GetMaterial(worldX, worldY, worldZ).IsFluid) && worldY > 1)
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(worldX, worldY, worldZ, GetStructureBlockId(blockId, meta), GetStructureBlockMeta(blockId, meta), false);
            --worldY;
        }
    }

    protected void CreateTreasureChestAtCurrentPosition(IWorldContext world, StructureBoundingBox bounds, JavaRandom random, int x, int y, int z, StructurePieceTreasure[] loot, int count)
    {
        int worldX = GetXWithOffset(x, z);
        int worldY = GetYWithOffset(y);
        int worldZ = GetZWithOffset(x, z);
        if (!bounds.Contains(worldX, worldY, worldZ) || world.Reader.GetBlockId(worldX, worldY, worldZ) == Block.Chest.id)
        {
            return;
        }

        world.Writer.SetBlock(worldX, worldY, worldZ, Block.Chest.id, 0, true);
        BlockEntityChest? chest = world.Entities.GetBlockEntity<BlockEntityChest>(worldX, worldY, worldZ);
        if (chest != null)
        {
            FillTreasureChestWithLoot(random, loot, chest, count);
        }
    }

    private static void FillTreasureChestWithLoot(JavaRandom random, StructurePieceTreasure[] loot, BlockEntityChest chest, int count)
    {
        for (int i = 0; i < count; ++i)
        {
            StructurePieceTreasure entry = ChooseTreasure(random, loot);
            int stackCount = entry.MinItemStack + random.NextInt(entry.MaxItemStack - entry.MinItemStack + 1);
            Item item = Item.ITEMS[entry.ItemId];
            if (item.getMaxCount() >= stackCount)
            {
                chest.setStack(random.NextInt(chest.size()), new ItemStack(entry.ItemId, stackCount, entry.ItemMetadata));
            }
            else
            {
                for (int j = 0; j < stackCount; ++j)
                {
                    chest.setStack(random.NextInt(chest.size()), new ItemStack(entry.ItemId, 1, entry.ItemMetadata));
                }
            }
        }
    }

    private static StructurePieceTreasure ChooseTreasure(JavaRandom random, StructurePieceTreasure[] loot)
    {
        int totalWeight = 0;
        for (int i = 0; i < loot.Length; ++i)
        {
            totalWeight += loot[i].Weight;
        }

        int value = random.NextInt(totalWeight);
        for (int i = 0; i < loot.Length; ++i)
        {
            value -= loot[i].Weight;
            if (value < 0)
            {
                return loot[i];
            }
        }

        return loot[^1];
    }
}
