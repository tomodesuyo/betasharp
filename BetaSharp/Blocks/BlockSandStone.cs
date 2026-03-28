using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockSandStone : Block
{
    public BlockSandStone(int id) : base(id, 192, Material.Stone)
    {
    }

    public override int getTexture(int side)
    {
        return side == 1 ? textureId - 16 : side == 0 ? textureId + 16 : textureId;
    }

    public override int getTexture(int side, int meta)
    {
        return side != 1 && (side != 0 || meta != 1 && meta != 2)
            ? side == 0 ? 208 : meta == 1 ? 229 : meta == 2 ? 230 : 192
            : 176;
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta & 3;
    }
}
