using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentMineshaftRoom : StructureComponent
{
    private readonly List<StructureBoundingBox> _entrances = [];

    public ComponentMineshaftRoom(int componentType, JavaRandom random, int x, int z) : base(componentType)
    {
        BoundingBox = new StructureBoundingBox(x, 50, z, x + 7 + random.NextInt(6), 54 + random.NextInt(6), z + 7 + random.NextInt(6));
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        int depth = GetComponentType();
        int roomHeight = BoundingBox.GetYSize() - 4;
        if (roomHeight <= 0)
        {
            roomHeight = 1;
        }

        for (int x = 0; x < BoundingBox.GetXSize(); x += 4)
        {
            x += random.NextInt(BoundingBox.GetXSize());
            if (x + 3 > BoundingBox.GetXSize())
            {
                break;
            }

            StructureComponent? next = StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + x, BoundingBox.MinY + random.NextInt(roomHeight) + 1, BoundingBox.MinZ - 1, 2, depth);
            if (next != null)
            {
                StructureBoundingBox box = next.GetBoundingBox();
                _entrances.Add(new StructureBoundingBox(box.MinX, box.MinY, BoundingBox.MinZ, box.MaxX, box.MaxY, BoundingBox.MinZ + 1));
            }
        }

        for (int x = 0; x < BoundingBox.GetXSize(); x += 4)
        {
            x += random.NextInt(BoundingBox.GetXSize());
            if (x + 3 > BoundingBox.GetXSize())
            {
                break;
            }

            StructureComponent? next = StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX + x, BoundingBox.MinY + random.NextInt(roomHeight) + 1, BoundingBox.MaxZ + 1, 0, depth);
            if (next != null)
            {
                StructureBoundingBox box = next.GetBoundingBox();
                _entrances.Add(new StructureBoundingBox(box.MinX, box.MinY, BoundingBox.MaxZ - 1, box.MaxX, box.MaxY, BoundingBox.MaxZ));
            }
        }

        for (int z = 0; z < BoundingBox.GetZSize(); z += 4)
        {
            z += random.NextInt(BoundingBox.GetZSize());
            if (z + 3 > BoundingBox.GetZSize())
            {
                break;
            }

            StructureComponent? next = StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + random.NextInt(roomHeight) + 1, BoundingBox.MinZ + z, 1, depth);
            if (next != null)
            {
                StructureBoundingBox box = next.GetBoundingBox();
                _entrances.Add(new StructureBoundingBox(BoundingBox.MinX, box.MinY, box.MinZ, BoundingBox.MinX + 1, box.MaxY, box.MaxZ));
            }
        }

        for (int z = 0; z < BoundingBox.GetZSize(); z += 4)
        {
            z += random.NextInt(BoundingBox.GetZSize());
            if (z + 3 > BoundingBox.GetZSize())
            {
                break;
            }

            StructureComponent? next = StructureMineshaftPieces.GetNextComponent(component, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + random.NextInt(roomHeight) + 1, BoundingBox.MinZ + z, 3, depth);
            if (next != null)
            {
                StructureBoundingBox box = next.GetBoundingBox();
                _entrances.Add(new StructureBoundingBox(BoundingBox.MaxX - 1, box.MinY, box.MinZ, BoundingBox.MaxX, box.MaxY, box.MaxZ));
            }
        }
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithBlocks(world, bounds, BoundingBox.MinX, BoundingBox.MinY, BoundingBox.MinZ, BoundingBox.MaxX, BoundingBox.MinY, BoundingBox.MaxZ, Block.Dirt.id, 0, true);
        FillWithBlocks(world, bounds, BoundingBox.MinX, BoundingBox.MinY + 1, BoundingBox.MinZ, BoundingBox.MaxX, Math.Min(BoundingBox.MinY + 3, BoundingBox.MaxY), BoundingBox.MaxZ, 0, 0, false);

        for (int i = 0; i < _entrances.Count; ++i)
        {
            StructureBoundingBox entrance = _entrances[i];
            FillWithBlocks(world, bounds, entrance.MinX, entrance.MaxY - 2, entrance.MinZ, entrance.MaxX, entrance.MaxY, entrance.MaxZ, 0, 0, false);
        }

        RandomlyFillSphere(world, bounds, BoundingBox.MinX, BoundingBox.MinY + 4, BoundingBox.MinZ, BoundingBox.MaxX, BoundingBox.MaxY, BoundingBox.MaxZ, 0, false);
        return true;
    }
}
