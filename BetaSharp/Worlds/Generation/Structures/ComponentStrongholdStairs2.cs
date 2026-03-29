using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdStairs2 : StructureComponent
{
    public ComponentStrongholdPortalRoom? PortalRoom { get; set; }

    public ComponentStrongholdStairs2(int componentType, JavaRandom random, int x, int z) : base(componentType)
    {
        Facing = random.NextInt(4);
        BoundingBox = new StructureBoundingBox(x, 64, z, x + 4, 74, z + 4);
    }

    public override void BuildComponent(StructureComponent component, List<StructureComponent> components, JavaRandom random)
    {
        int anchorX;
        int anchorZ;

        switch (Facing)
        {
            case 0:
                anchorX = BoundingBox.MinX + 1;
                anchorZ = BoundingBox.MaxZ + 1;
                break;
            case 1:
                anchorX = BoundingBox.MinX - 1;
                anchorZ = BoundingBox.MinZ + 1;
                break;
            case 2:
                anchorX = BoundingBox.MinX + 1;
                anchorZ = BoundingBox.MinZ - 1;
                break;
            case 3:
                anchorX = BoundingBox.MaxX + 1;
                anchorZ = BoundingBox.MinZ + 1;
                break;
            default:
                return;
        }

        ComponentStrongholdPortalRoom? portalRoom = ComponentStrongholdPortalRoom.Create(anchorX, BoundingBox.MinY + 1, anchorZ, Facing, ComponentType + 1, components);
        if (portalRoom == null)
        {
            return;
        }

        components.Add(portalRoom);
        portalRoom.BuildComponent(this, components, random);
    }

    public override bool AddComponentParts(IWorldContext world, JavaRandom random, StructureBoundingBox bounds)
    {
        if (IsLiquidInStructureBoundingBox(world, bounds))
        {
            return false;
        }

        FillWithBlocks(world, bounds, 0, 0, 0, 4, 10, 4, Block.StoneBrick.id, 0, false);
        FillWithBlocks(world, bounds, 1, 7, 0, 3, 9, 0, 0, 0, false);
        FillWithBlocks(world, bounds, 1, 1, 4, 3, 3, 4, 0, 0, false);

        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 6, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 5, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 6, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 5, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 5, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 3, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 3, 4, 3, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 3, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 3, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 3, 3, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 2, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 1, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 2, 1, bounds);
        PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, 1, 1, 2, bounds);
        PlaceBlockAtCurrentPosition(world, Block.Slab.id, 0, 1, 1, 3, bounds);
        return true;
    }
}
