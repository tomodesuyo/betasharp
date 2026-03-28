using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentMineshaftCross : StructureComponent
{
    private readonly int _corridorDirection;
    private readonly bool _isTwoFloors;

    public ComponentMineshaftCross(int componentType, JavaRandom random, StructureBoundingBox bounds, int facing) : base(componentType)
    {
        _corridorDirection = facing;
        BoundingBox = bounds;
        _isTwoFloors = bounds.GetYSize() > 3;
    }

    public static StructureBoundingBox? FindValidPlacement(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing)
    {
        StructureBoundingBox bounds = new(x, y, z, x, y + 2, z);
        if (random.NextInt(4) == 0)
        {
            bounds.MaxY += 4;
        }

        switch (facing)
        {
            case 0:
                bounds.MinX = x - 1;
                bounds.MaxX = x + 3;
                bounds.MaxZ = z + 4;
                break;
            case 1:
                bounds.MinX = x - 4;
                bounds.MinZ = z - 1;
                bounds.MaxZ = z + 3;
                break;
            case 2:
                bounds.MinX = x - 1;
                bounds.MaxX = x + 3;
                bounds.MinZ = z - 4;
                break;
            case 3:
                bounds.MaxX = x + 4;
                bounds.MinZ = z - 1;
                bounds.MaxZ = z + 3;
                break;
        }

        return FindIntersecting(components, bounds) != null ? null : bounds;
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        int depth = GetComponentType();
        switch (_corridorDirection)
        {
            case 0:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, BoundingBox.MinZ + 1, 1, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, BoundingBox.MinZ + 1, 3, depth);
                break;
            case 1:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, BoundingBox.MinZ + 1, 1, depth);
                break;
            case 2:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY, BoundingBox.MinZ + 1, 1, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, BoundingBox.MinZ + 1, 3, depth);
                break;
            case 3:
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MinZ - 1, 2, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MaxZ + 1, 0, depth);
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY, BoundingBox.MinZ + 1, 3, depth);
                break;
        }

        if (_isTwoFloors)
        {
            if (random.NextBoolean())
            {
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY + 4, BoundingBox.MinZ - 1, 2, depth);
            }

            if (random.NextBoolean())
            {
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + 4, BoundingBox.MinZ + 1, 1, depth);
            }

            if (random.NextBoolean())
            {
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + 4, BoundingBox.MinZ + 1, 3, depth);
            }

            if (random.NextBoolean())
            {
                StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + 1, BoundingBox.MinY + 4, BoundingBox.MaxZ + 1, 0, depth);
            }
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        if (_isTwoFloors)
        {
            FillWithBlocks(world, bounds, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MinZ, BoundingBox.MaxX - 1, BoundingBox.MinY + 2, BoundingBox.MaxZ, 0, 0, false);
            FillWithBlocks(world, bounds, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MinZ + 1, BoundingBox.MaxX, BoundingBox.MinY + 2, BoundingBox.MaxZ - 1, 0, 0, false);
            FillWithBlocks(world, bounds, BoundingBox.MinX + 1, BoundingBox.MaxY - 2, BoundingBox.MinZ, BoundingBox.MaxX - 1, BoundingBox.MaxY, BoundingBox.MaxZ, 0, 0, false);
            FillWithBlocks(world, bounds, BoundingBox.MinX, BoundingBox.MaxY - 2, BoundingBox.MinZ + 1, BoundingBox.MaxX, BoundingBox.MaxY, BoundingBox.MaxZ - 1, 0, 0, false);
            FillWithBlocks(world, bounds, BoundingBox.MinX + 1, BoundingBox.MinY + 3, BoundingBox.MinZ + 1, BoundingBox.MaxX - 1, BoundingBox.MinY + 3, BoundingBox.MaxZ - 1, 0, 0, false);
        }
        else
        {
            FillWithBlocks(world, bounds, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MinZ, BoundingBox.MaxX - 1, BoundingBox.MaxY, BoundingBox.MaxZ, 0, 0, false);
            FillWithBlocks(world, bounds, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MinZ + 1, BoundingBox.MaxX, BoundingBox.MaxY, BoundingBox.MaxZ - 1, 0, 0, false);
        }

        FillWithBlocks(world, bounds, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MinZ + 1, BoundingBox.MinX + 1, BoundingBox.MaxY, BoundingBox.MinZ + 1, Block.Planks.id, 0, false);
        FillWithBlocks(world, bounds, BoundingBox.MinX + 1, BoundingBox.MinY, BoundingBox.MaxZ - 1, BoundingBox.MinX + 1, BoundingBox.MaxY, BoundingBox.MaxZ - 1, Block.Planks.id, 0, false);
        FillWithBlocks(world, bounds, BoundingBox.MaxX - 1, BoundingBox.MinY, BoundingBox.MinZ + 1, BoundingBox.MaxX - 1, BoundingBox.MaxY, BoundingBox.MinZ + 1, Block.Planks.id, 0, false);
        FillWithBlocks(world, bounds, BoundingBox.MaxX - 1, BoundingBox.MinY, BoundingBox.MaxZ - 1, BoundingBox.MaxX - 1, BoundingBox.MaxY, BoundingBox.MaxZ - 1, Block.Planks.id, 0, false);
        return true;
    }
}
