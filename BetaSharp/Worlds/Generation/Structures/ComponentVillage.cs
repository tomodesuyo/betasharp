using BetaSharp.Util.Maths;
using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public abstract class ComponentVillage(int componentType) : StructureComponent(componentType)
{
    private int _villagersSpawned;

    protected StructureComponent? GetNextComponentNN(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int y, int z)
    {
        return Facing switch
        {
            0 or 2 => StructureVillagePieces.GetNextStructureComponent(startPiece, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + y, BoundingBox.MinZ + z, 1, GetComponentType()),
            1 or 3 => StructureVillagePieces.GetNextStructureComponent(startPiece, components, random, BoundingBox.MinX + z, BoundingBox.MinY + y, BoundingBox.MinZ - 1, 2, GetComponentType()),
            _ => null,
        };
    }

    protected StructureComponent? GetNextComponentPP(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int y, int z)
    {
        return Facing switch
        {
            0 or 2 => StructureVillagePieces.GetNextStructureComponent(startPiece, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + y, BoundingBox.MinZ + z, 3, GetComponentType()),
            1 or 3 => StructureVillagePieces.GetNextStructureComponent(startPiece, components, random, BoundingBox.MinX + z, BoundingBox.MinY + y, BoundingBox.MaxZ + 1, 0, GetComponentType()),
            _ => null,
        };
    }

    protected int GetAverageGroundLevel(IWorldContext world, StructureBoundingBox bounds)
    {
        int total = 0;
        int count = 0;

        for (int z = BoundingBox.MinZ; z <= BoundingBox.MaxZ; ++z)
        {
            for (int x = BoundingBox.MinX; x <= BoundingBox.MaxX; ++x)
            {
                if (!bounds.Contains(x, 64, z))
                {
                    continue;
                }

                total += Math.Max(world.Reader.GetTopSolidBlockY(x, z), 63);
                ++count;
            }
        }

        return count == 0 ? -1 : total / count;
    }

    protected static bool CanVillageGoDeeper(StructureBoundingBox? bounds) => bounds != null && bounds.MinY > 10;

    protected void SpawnVillagers(IWorldContext world, StructureBoundingBox bounds, int x, int y, int z, int count)
    {
        for (int i = _villagersSpawned; i < count; ++i)
        {
            int worldX = GetXWithOffset(x + i, z);
            int worldY = GetYWithOffset(y);
            int worldZ = GetZWithOffset(x + i, z);
            if (!bounds.Contains(worldX, worldY, worldZ))
            {
                break;
            }

            ++_villagersSpawned;
            EntityVillager villager = new(world, GetVillagerType(i));
            villager.setPositionAndAnglesKeepPrevAngles(worldX + 0.5D, worldY, worldZ + 0.5D, 0.0F, 0.0F);
            world.SpawnEntity(villager);
        }
    }

    protected virtual int GetVillagerType(int index) => 0;

    protected void PlaceDoorAtCurrentPosition(IWorldContext world, StructureBoundingBox bounds, JavaRandom random, int x, int y, int z, int meta)
    {
        int worldX = GetXWithOffset(x, z);
        int worldY = GetYWithOffset(y);
        int worldZ = GetZWithOffset(x, z);
        if (!bounds.Contains(worldX, worldY, worldZ))
        {
            return;
        }

        int doorMeta = GetMetadataWithOffset(Block.Door.id, meta);
        world.Writer.SetBlockWithoutNotifyingNeighbors(worldX, worldY, worldZ, Block.Door.id, doorMeta, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(worldX, worldY + 1, worldZ, Block.Door.id, 8, false);
    }
}
