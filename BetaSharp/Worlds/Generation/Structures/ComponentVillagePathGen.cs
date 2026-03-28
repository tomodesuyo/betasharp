using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class ComponentVillagePathGen : ComponentVillageRoadPiece
{
    private readonly int _pathLength;

    public ComponentVillagePathGen(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing)
        : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
        _pathLength = Math.Max(bounds.GetXSize(), bounds.GetZSize());
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        ComponentVillageStartPiece startPiece = (ComponentVillageStartPiece)component;
        bool branched = false;

        for (int distance = random.NextInt(5); distance < _pathLength - 8; distance += 2 + random.NextInt(5))
        {
            StructureComponent? next = GetNextComponentNN(startPiece, components, random, 0, distance);
            if (next != null)
            {
                distance += Math.Max(next.GetBoundingBox().GetXSize(), next.GetBoundingBox().GetZSize());
                branched = true;
            }
        }

        for (int distance = random.NextInt(5); distance < _pathLength - 8; distance += 2 + random.NextInt(5))
        {
            StructureComponent? next = GetNextComponentPP(startPiece, components, random, 0, distance);
            if (next != null)
            {
                distance += Math.Max(next.GetBoundingBox().GetXSize(), next.GetBoundingBox().GetZSize());
                branched = true;
            }
        }

        if (branched && random.NextInt(3) > 0)
        {
            switch (Facing)
            {
                case 0:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, BoundingBox.MaxZ - 2, 1, GetComponentType());
                    break;
                case 1:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, GetComponentType());
                    break;
                case 2:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, BoundingBox.MinZ, 1, GetComponentType());
                    break;
                case 3:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MaxX - 2, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, GetComponentType());
                    break;
            }
        }

        if (branched && random.NextInt(3) > 0)
        {
            switch (Facing)
            {
                case 0:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, BoundingBox.MaxZ - 2, 3, GetComponentType());
                    break;
                case 1:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, GetComponentType());
                    break;
                case 2:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, BoundingBox.MinZ, 3, GetComponentType());
                    break;
                case 3:
                    StructureVillagePieces.GetNextStructureComponentVillagePath(startPiece, components, random, BoundingBox.MaxX - 2, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, GetComponentType());
                    break;
            }
        }
    }

    public static StructureBoundingBox? FindValidPlacement(ComponentVillageStartPiece startPiece, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing)
    {
        for (int size = 7 * (random.NextInt(3) + 3); size >= 7; size -= 7)
        {
            StructureBoundingBox bounds = StructureBoundingBox.Create(x, y, z, 0, 0, 0, 3, 3, size, facing);
            if (StructureComponent.FindIntersecting(components, bounds) == null)
            {
                return bounds;
            }
        }

        return null;
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        for (int x = BoundingBox.MinX; x <= BoundingBox.MaxX; ++x)
        {
            for (int z = BoundingBox.MinZ; z <= BoundingBox.MaxZ; ++z)
            {
                if (!bounds.Contains(x, 64, z))
                {
                    continue;
                }

                int y = world.Reader.GetTopSolidBlockY(x, z) - 1;
                world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Gravel.id, 0, false);
            }
        }

        return true;
    }
}
