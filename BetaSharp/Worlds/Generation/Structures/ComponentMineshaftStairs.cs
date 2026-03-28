using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentMineshaftStairs : StructureComponent
{
    public ComponentMineshaftStairs(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        Facing = facing;
        BoundingBox = bounds;
    }

    public static StructureBoundingBox? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing)
    {
        StructureBoundingBox bounds = new(x, y - 5, z, x, y + 2, z);
        switch (facing)
        {
            case 0:
                bounds.MaxX = x + 2;
                bounds.MaxZ = z + 8;
                break;
            case 1:
                bounds.MinX = x - 8;
                bounds.MaxZ = z + 2;
                break;
            case 2:
                bounds.MaxX = x + 2;
                bounds.MinZ = z - 8;
                break;
            case 3:
                bounds.MaxX = x + 8;
                bounds.MaxZ = z + 2;
                break;
        }

        return FindIntersecting(components, bounds) != null ? null : bounds;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        int depth = GetComponentType();
        switch (Facing)
        {
            case 0:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, depth);
                break;
            case 1:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, BoundingBox.MinZ, 1, depth);
                break;
            case 2:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, depth);
                break;
            case 3:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, BoundingBox.MinZ, 3, depth);
                break;
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithBlocks(world, bounds, 0, 5, 0, 2, 7, 1, 0, 0, false);
        FillWithBlocks(world, bounds, 0, 0, 7, 2, 2, 8, 0, 0, false);
        for (int step = 0; step < 5; ++step)
        {
            FillWithBlocks(world, bounds, 0, 5 - step - (step < 4 ? 1 : 0), 2 + step, 2, 7 - step, 2 + step, 0, 0, false);
        }

        return true;
    }
}
