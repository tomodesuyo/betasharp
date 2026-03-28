using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockStoneBrick : Block
{
    public BlockStoneBrick(int id) : base(id, 54, Material.Stone)
    {
    }

    public override int getTexture(int side, int meta)
    {
        int variant = meta & 3;
        return textureId + variant;
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta & 3;
    }
}
