using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class BrewingStandRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 0.0F, 1.0F, 2.0F / 16.0F, 1.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 0.0F, 7.0F / 16.0F, 9.0F / 16.0F, 14.0F / 16.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(2.0F / 16.0F, 2.0F / 16.0F, 7.0F / 16.0F, 6.0F / 16.0F, 4.0F / 16.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(10.0F / 16.0F, 2.0F / 16.0F, 7.0F / 16.0F, 14.0F / 16.0F, 4.0F / 16.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 2.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 4.0F / 16.0F, 6.0F / 16.0F) }).DrawBlock(block, pos);
        return true;
    }
}
