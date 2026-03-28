using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public class StairsRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        bool hasRendered = false;
        int metadata = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        int direction = metadata & 3;
        float minY = 0.0F;
        float maxY = 0.5F;
        float stepMinY = 0.5F;
        float stepMaxY = 1.0F;

        if (ctx.BlockReader is ItemRenderBlockAccess)
        {
            direction = 3;
        }
        else if ((metadata & 4) != 0)
        {
            minY = 0.5F;
            maxY = 1.0F;
            stepMinY = 0.0F;
            stepMaxY = 0.5F;
        }

        hasRendered |= (ctx with { OverrideBounds = new Box(0.0F, minY, 0.0F, 1.0F, maxY, 1.0F) }).DrawBlock(block, pos);

        switch (direction)
        {
            case 0:
                hasRendered |= (ctx with { OverrideBounds = new Box(0.5F, stepMinY, 0.0F, 1.0F, stepMaxY, 1.0F) }).DrawBlock(block, pos);
                break;
            case 1:
                hasRendered |= (ctx with { OverrideBounds = new Box(0.0F, stepMinY, 0.0F, 0.5F, stepMaxY, 1.0F) }).DrawBlock(block, pos);
                break;
            case 2:
                hasRendered |= (ctx with { OverrideBounds = new Box(0.0F, stepMinY, 0.5F, 1.0F, stepMaxY, 1.0F) }).DrawBlock(block, pos);
                break;
            case 3:
                hasRendered |= (ctx with { OverrideBounds = new Box(0.0F, stepMinY, 0.0F, 1.0F, stepMaxY, 0.5F) }).DrawBlock(block, pos);
                break;
        }

        return hasRendered;
    }
}
