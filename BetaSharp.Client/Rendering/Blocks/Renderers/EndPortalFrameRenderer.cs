using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class EndPortalFrameRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int meta = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        int facing = meta & 3;
        int topRotation = facing switch
        {
            0 => 3,
            1 => 2,
            3 => 1,
            _ => 0
        };
        var frameCtx = ctx with
        {
            OverrideBounds = new Box(0.0F, 0.0F, 0.0F, 1.0F, 13.0F / 16.0F, 1.0F),
            UvRotateTop = topRotation
        };
        frameCtx.DrawBlock(block, pos);

        if ((meta & 4) != 0)
        {
            var eyeCtx = ctx with
            {
                OverrideBounds = new Box(4.0F / 16.0F, 13.0F / 16.0F, 4.0F / 16.0F, 12.0F / 16.0F, 1.0F, 12.0F / 16.0F),
                OverrideTexture = 174,
                EnableAo = false
            };
            eyeCtx.DrawBlock(block, pos);

            FaceColors colors = FaceColors.AssignVertexColors(1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, 1.0F, true);
            Vec3D worldPos = new(pos.x, pos.y, pos.z);
            var insetTopCtx = ctx with
            {
                EnableAo = false,
                OverrideTexture = 174,
                OverrideBounds = new Box(4.0F / 16.0F, 15.0F / 16.0F, 4.0F / 16.0F, 12.0F / 16.0F, 15.0F / 16.0F, 12.0F / 16.0F),
                UvRotateTop = topRotation
            };
            insetTopCtx.DrawTopFace(block, in worldPos, colors, 174);
        }

        return true;
    }
}
