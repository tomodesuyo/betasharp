using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class FenceGateRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int meta = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        int dir = meta & 3;
        bool open = (meta & 4) != 0;
        bool alongZ = dir == 0 || dir == 2;

        if (alongZ)
        {
            (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 7.0F / 16.0F, 2.0F / 16.0F, 1.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
            (ctx with { OverrideBounds = new Box(14.0F / 16.0F, 0.0F, 7.0F / 16.0F, 1.0F, 1.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
            if (!open)
            {
                (ctx with { OverrideBounds = new Box(2.0F / 16.0F, 3.0F / 16.0F, 7.0F / 16.0F, 14.0F / 16.0F, 6.0F / 16.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
                (ctx with { OverrideBounds = new Box(2.0F / 16.0F, 10.0F / 16.0F, 7.0F / 16.0F, 14.0F / 16.0F, 13.0F / 16.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
            }
            else
            {
                (ctx with { OverrideBounds = new Box(0.0F, 3.0F / 16.0F, 0.0F, 2.0F / 16.0F, 13.0F / 16.0F, 7.0F / 16.0F) }).DrawBlock(block, pos);
                (ctx with { OverrideBounds = new Box(14.0F / 16.0F, 3.0F / 16.0F, 9.0F / 16.0F, 1.0F, 13.0F / 16.0F, 1.0F) }).DrawBlock(block, pos);
            }
        }
        else
        {
            (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 0.0F, 0.0F, 9.0F / 16.0F, 1.0F, 2.0F / 16.0F) }).DrawBlock(block, pos);
            (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 0.0F, 14.0F / 16.0F, 9.0F / 16.0F, 1.0F, 1.0F) }).DrawBlock(block, pos);
            if (!open)
            {
                (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 3.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 6.0F / 16.0F, 14.0F / 16.0F) }).DrawBlock(block, pos);
                (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 10.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 13.0F / 16.0F, 14.0F / 16.0F) }).DrawBlock(block, pos);
            }
            else
            {
                (ctx with { OverrideBounds = new Box(0.0F, 3.0F / 16.0F, 0.0F, 7.0F / 16.0F, 13.0F / 16.0F, 2.0F / 16.0F) }).DrawBlock(block, pos);
                (ctx with { OverrideBounds = new Box(9.0F / 16.0F, 3.0F / 16.0F, 14.0F / 16.0F, 1.0F, 13.0F / 16.0F, 1.0F) }).DrawBlock(block, pos);
            }
        }

        return true;
    }
}
