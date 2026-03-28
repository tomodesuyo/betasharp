using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockCauldron : Block
{
    public BlockCauldron(int id) : base(id, 154, Material.Metal)
    {
        setBoundingBox(0.125F, 0.0F, 0.125F, 0.875F, 1.0F, 0.875F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;
}
