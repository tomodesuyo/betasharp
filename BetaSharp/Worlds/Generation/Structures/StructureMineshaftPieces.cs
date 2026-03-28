using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

internal static class StructureMineshaftPieces
{
    private static readonly StructurePieceTreasure[] LootArray =
    [
        new(Item.IronIngot.id, 0, 1, 5, 10),
        new(Item.GoldIngot.id, 0, 1, 3, 5),
        new(Item.Redstone.id, 0, 4, 9, 5),
        new(Item.Dye.id, 4, 4, 9, 5),
        new(Item.Diamond.id, 0, 1, 2, 3),
        new(Item.Coal.id, 0, 3, 8, 10),
        new(Item.Bread.id, 0, 1, 3, 15),
        new(Item.IronPickaxe.id, 0, 1, 1, 1),
        new(Block.Rail.id, 0, 4, 8, 1)
    ];

    private static StructureComponent? GetRandomComponent(List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int depth)
    {
        int value = random.NextInt(100);
        StructureBoundingBox? box;
        if (value >= 80)
        {
            box = ComponentMineshaftCross.FindValidPlacement(components, random, x, y, z, facing);
            if (box != null)
            {
                return new ComponentMineshaftCross(depth, random, box, facing);
            }
        }
        else if (value >= 70)
        {
            box = ComponentMineshaftStairs.FindValidPlacement(components, random, x, y, z, facing);
            if (box != null)
            {
                return new ComponentMineshaftStairs(depth, random, box, facing);
            }
        }
        else
        {
            box = ComponentMineshaftCorridor.FindValidPlacement(components, random, x, y, z, facing);
            if (box != null)
            {
                return new ComponentMineshaftCorridor(depth, random, box, facing);
            }
        }

        return null;
    }

    private static StructureComponent? GetNextMineshaftComponent(StructureComponent start, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int depth)
    {
        if (depth > 8)
        {
            return null;
        }

        if (Math.Abs(x - start.GetBoundingBox().MinX) > 80 || Math.Abs(z - start.GetBoundingBox().MinZ) > 80)
        {
            return null;
        }

        StructureComponent? component = GetRandomComponent(components, random, x, y, z, facing, depth + 1);
        if (component != null)
        {
            components.Add(component);
            component.BuildComponent(start, components, random);
        }

        return component;
    }

    public static StructureComponent? GetNextComponent(StructureComponent start, List<StructureComponent> components, JavaRandom random, int x, int y, int z, int facing, int depth)
    {
        return GetNextMineshaftComponent(start, components, random, x, y, z, facing, depth);
    }

    public static StructurePieceTreasure[] GetTreasurePieces() => LootArray;
}
