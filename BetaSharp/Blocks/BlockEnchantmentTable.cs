using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockEnchantmentTable : Block
{
    public BlockEnchantmentTable(int id) : base(id, 166, Material.Stone)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 0.75F, 1.0F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;
}
