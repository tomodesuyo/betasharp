using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal sealed class BlockBarrier : Block
{
    public BlockBarrier(int id) : base(id, Material.Piston)
    {
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => true;

    public override BlockRendererType getRenderType() => BlockRendererType.Entity;

    public override int getDroppedItemId(int blockMeta) => 0;
}
