namespace BetaSharp.Worlds.Generation.Structures;

public sealed class StructurePieceTreasure(int itemId, int itemMetadata, int minItemStack, int maxItemStack, int weight)
{
    public int ItemId { get; } = itemId;
    public int ItemMetadata { get; } = itemMetadata;
    public int MinItemStack { get; } = minItemStack;
    public int MaxItemStack { get; } = maxItemStack;
    public int Weight { get; } = weight;
}
