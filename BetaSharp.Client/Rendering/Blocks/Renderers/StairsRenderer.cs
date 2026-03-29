using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public class StairsRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        bool hasRendered = false;
        List<Box> stairBoxes = [];
        int? metadataOverride = ctx.BlockReader is ItemRenderBlockAccess ? 3 : null;
        StairsShapeHelper.GetSubBoxes(ctx.BlockReader, pos.x, pos.y, pos.z, stairBoxes, metadataOverride);

        for (int i = 0; i < stairBoxes.Count; ++i)
        {
            hasRendered |= (ctx with { OverrideBounds = stairBoxes[i] }).DrawBlock(block, pos);
        }

        return hasRendered;
    }
}
