using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockWood : Block
{
    public BlockWood(int id) : base(id, 4, Material.Wood)
    {
    }

    public override int getTexture(int side, int meta)
    {
        return (meta & 3) switch
        {
            1 => 198,
            2 => 214,
            3 => 199,
            _ => 4
        };
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta;
    }
}
