using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class EndPortalFrameRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int meta = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 0.0F, 1.0F, 13.0F / 16.0F, 1.0F) }).DrawBlock(block, pos);
        if ((meta & 4) != 0)
        {
            (ctx with { OverrideBounds = new Box(5.0F / 16.0F, 13.0F / 16.0F, 5.0F / 16.0F, 11.0F / 16.0F, 1.0F, 11.0F / 16.0F) }).DrawBlock(block, pos);
        }

        return true;
    }
}
