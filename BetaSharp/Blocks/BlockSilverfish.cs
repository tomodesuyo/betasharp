using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockSilverfish : Block
{
    public BlockSilverfish(int id) : base(id, 1, Material.Stone)
    {
    }

    public override int getDroppedItemId(int blockMeta)
    {
        return 0;
    }

    public override int getDroppedItemCount()
    {
        return 0;
    }
}
