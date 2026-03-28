using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class CauldronRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 0.0F, 1.0F, 5.0F / 16.0F, 1.0F) }).DrawBlock(block, pos);

        float thickness = 2.0F / 16.0F;
        (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 0.0F, thickness, 1.0F, 1.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(1.0F - thickness, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, thickness) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 1.0F - thickness, 1.0F, 1.0F, 1.0F) }).DrawBlock(block, pos);

        FaceColors colors = FaceColors.AssignVertexColors(1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, true);
        Vec3D worldPos = new(pos.x, pos.y, pos.z);
        var planeCtx = ctx with { EnableAo = false, OverrideTexture = 139 };
        planeCtx.OverrideBounds = new Box(thickness, 4.0F / 16.0F, thickness, 1.0F - thickness, 4.0F / 16.0F, 1.0F - thickness);
        planeCtx.DrawTopFace(block, in worldPos, colors, 139);
        planeCtx.DrawBottomFace(block, in worldPos, colors, 139);

        int level = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        if (level > 0)
        {
            if (level > 3)
            {
                level = 3;
            }

            float waterY = (6.0F + level * 3.0F) / 16.0F;
            var waterCtx = ctx with { EnableAo = false, OverrideTexture = 205 };
            waterCtx.OverrideBounds = new Box(thickness, waterY, thickness, 1.0F - thickness, waterY, 1.0F - thickness);
            waterCtx.DrawTopFace(block, in worldPos, colors, 205);
        }

        return true;
    }
}
