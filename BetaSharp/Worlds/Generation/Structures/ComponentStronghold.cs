using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal abstract class ComponentStronghold : StructureComponent
{
    protected ComponentStronghold(int componentType) : base(componentType)
    {
    }

    protected void PlaceDoor(IWorldContext world, JavaRandom random, StructureBoundingBox bounds, StrongholdDoor door, int x, int y, int z)
    {
        switch (door)
        {
            case StrongholdDoor.Opening:
                FillWithBlocks(world, bounds, x, y, z, x + 2, y + 2, z, 0, 0, false);
                break;
            case StrongholdDoor.WoodDoor:
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 1, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 2, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 2, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 2, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Door.id, 0, x + 1, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Door.id, 8, x + 1, y + 1, z, bounds);
                break;
            case StrongholdDoor.Grates:
                PlaceBlockAtCurrentPosition(world, 0, 0, x + 1, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, 0, 0, x + 1, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x + 1, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x + 2, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x + 2, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronBars.id, 0, x + 2, y, z, bounds);
                break;
            case StrongholdDoor.IronDoor:
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 1, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 2, y + 2, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 2, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.StoneBrick.id, 0, x + 2, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronDoor.id, 0, x + 1, y, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.IronDoor.id, 8, x + 1, y + 1, z, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Button.id, GetMetadataWithOffset(Block.Button.id, 4), x + 2, y + 1, z + 1, bounds);
                PlaceBlockAtCurrentPosition(world, Block.Button.id, GetMetadataWithOffset(Block.Button.id, 3), x + 2, y + 1, z - 1, bounds);
                break;
        }
    }

    protected StrongholdDoor GetRandomDoor(JavaRandom random)
    {
        return random.NextInt(5) switch
        {
            2 => StrongholdDoor.WoodDoor,
            3 => StrongholdDoor.Grates,
            4 => StrongholdDoor.IronDoor,
            _ => StrongholdDoor.Opening,
        };
    }

    protected StructureComponent? GetNextComponentNormal(ComponentStrongholdStairs2 startPiece, List<StructureComponent> components, JavaRandom random, int x, int y)
    {
        return Facing switch
        {
            0 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MinX + x, BoundingBox.MinY + y, BoundingBox.MaxZ + 1, Facing, GetComponentType()),
            1 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + y, BoundingBox.MinZ + x, Facing, GetComponentType()),
            2 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MinX + x, BoundingBox.MinY + y, BoundingBox.MinZ - 1, Facing, GetComponentType()),
            3 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + y, BoundingBox.MinZ + x, Facing, GetComponentType()),
            _ => null,
        };
    }

    protected StructureComponent? GetNextComponentX(ComponentStrongholdStairs2 startPiece, List<StructureComponent> components, JavaRandom random, int y, int z)
    {
        return Facing switch
        {
            0 or 2 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MinX - 1, BoundingBox.MinY + y, BoundingBox.MinZ + z, 1, GetComponentType()),
            1 or 3 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MinX + z, BoundingBox.MinY + y, BoundingBox.MinZ - 1, 2, GetComponentType()),
            _ => null,
        };
    }

    protected StructureComponent? GetNextComponentZ(ComponentStrongholdStairs2 startPiece, List<StructureComponent> components, JavaRandom random, int y, int x)
    {
        return Facing switch
        {
            0 or 2 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MaxX + 1, BoundingBox.MinY + y, BoundingBox.MinZ + x, 3, GetComponentType()),
            1 or 3 => StructureStrongholdPieces.GetNextValidComponentAccess(startPiece, components, random, BoundingBox.MinX + x, BoundingBox.MinY + y, BoundingBox.MaxZ + 1, 0, GetComponentType()),
            _ => null,
        };
    }

    protected static bool CanStrongholdGoDeeper(StructureBoundingBox? bounds) => bounds != null && bounds.MinY > 10;
}
