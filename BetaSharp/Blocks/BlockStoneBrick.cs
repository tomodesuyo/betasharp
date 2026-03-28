using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockStoneBrick : Block
{
    public BlockStoneBrick(int id) : base(id, 54, Material.Stone)
    {
    }

    public override int getTexture(int side, int meta)
    {
        return (meta & 3) switch
        {
            1 => 100,
            2 => 101,
            3 => 213,
            _ => 54,
        };
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta & 3;
    }
}
