using BetaSharp.Blocks;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public class FenceRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        BlockFence fence = (BlockFence)block;
        float postMin = 6.0F / 16.0F;
        float postMax = 10.0F / 16.0F;
        (ctx with { OverrideBounds = new Box(postMin, 0.0F, postMin, postMax, 1.0F, postMax) }).DrawBlock(block, pos);

        bool connectsWest = fence.canConnectFenceTo(ctx.BlockReader, pos.x - 1, pos.y, pos.z);
        bool connectsEast = fence.canConnectFenceTo(ctx.BlockReader, pos.x + 1, pos.y, pos.z);
        bool connectsNorth = fence.canConnectFenceTo(ctx.BlockReader, pos.x, pos.y, pos.z - 1);
        bool connectsSouth = fence.canConnectFenceTo(ctx.BlockReader, pos.x, pos.y, pos.z + 1);

        bool connectsX = connectsWest || connectsEast;
        bool connectsZ = connectsNorth || connectsSouth;

        if (!connectsX && !connectsZ)
        {
            connectsX = true;
        }

        float barDepthMin = 7.0F / 16.0F;
        float barDepthMax = 9.0F / 16.0F;
        float barMinX = connectsWest ? 0.0F : barDepthMin;
        float barMaxX = connectsEast ? 1.0F : barDepthMax;
        float barMinZ = connectsNorth ? 0.0F : barDepthMin;
        float barMaxZ = connectsSouth ? 1.0F : barDepthMax;

        float topBarMinY = 12.0F / 16.0F;
        float topBarMaxY = 15.0F / 16.0F;

        if (connectsX)
        {
            var topXCtx = ctx with
            {
                OverrideBounds = new Box(barMinX, topBarMinY, barDepthMin, barMaxX, topBarMaxY, barDepthMax)
            };
            topXCtx.DrawBlock(block, pos);
        }

        if (connectsZ)
        {
            var topZCtx = ctx with
            {
                OverrideBounds = new Box(barDepthMin, topBarMinY, barMinZ, barDepthMax, topBarMaxY, barMaxZ)
            };
            topZCtx.DrawBlock(block, pos);
        }

        float bottomBarMinY = 6.0F / 16.0F;
        float bottomBarMaxY = 9.0F / 16.0F;

        if (connectsX)
        {
            var bottomXCtx = ctx with
            {
                OverrideBounds = new Box(barMinX, bottomBarMinY, barDepthMin, barMaxX, bottomBarMaxY, barDepthMax)
            };
            bottomXCtx.DrawBlock(block, pos);
        }

        if (connectsZ)
        {
            var bottomZCtx = ctx with
            {
                OverrideBounds = new Box(barDepthMin, bottomBarMinY, barMinZ, barDepthMax, bottomBarMaxY, barMaxZ)
            };
            bottomZCtx.DrawBlock(block, pos);
        }

        return true;
    }
}
