using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class PaneRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        bool north = CanConnectTo(ctx, block, pos.x, pos.y, pos.z - 1);
        bool south = CanConnectTo(ctx, block, pos.x, pos.y, pos.z + 1);
        bool west = CanConnectTo(ctx, block, pos.x - 1, pos.y, pos.z);
        bool east = CanConnectTo(ctx, block, pos.x + 1, pos.y, pos.z);
        bool any = north || south || west || east;

        int faceTexture = ctx.OverrideTexture >= 0 ? ctx.OverrideTexture : block.getTexture(0, ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z));
        int edgeTexture = ctx.OverrideTexture >= 0 ? ctx.OverrideTexture : block.getTextureId(ctx.BlockReader, pos.x, pos.y, pos.z, 2);

        if ((!west || !east) && any)
        {
            if (west && !east)
            {
                RenderXSegment(block, pos, ref ctx, 0.0F, 0.5F, faceTexture, edgeTexture);
            }
            else if (!west && east)
            {
                RenderXSegment(block, pos, ref ctx, 0.5F, 1.0F, faceTexture, edgeTexture);
            }
        }
        else
        {
            RenderXSegment(block, pos, ref ctx, 0.0F, 1.0F, faceTexture, edgeTexture);
        }

        if ((!north || !south) && any)
        {
            if (north && !south)
            {
                RenderZSegment(block, pos, ref ctx, 0.0F, 0.5F, faceTexture, edgeTexture);
            }
            else if (!north && south)
            {
                RenderZSegment(block, pos, ref ctx, 0.5F, 1.0F, faceTexture, edgeTexture);
            }
        }
        else
        {
            RenderZSegment(block, pos, ref ctx, 0.0F, 1.0F, faceTexture, edgeTexture);
        }

        return true;
    }

    private static void RenderXSegment(Block block, in BlockPos pos, ref BlockRenderContext ctx, float minX, float maxX, int faceTexture, int edgeTexture)
    {
        var renderCtx = ctx with { OverrideBounds = new Box(minX, 0.0F, 7.0F / 16.0F, maxX, 1.0F, 9.0F / 16.0F), EnableAo = false };
        var origin = new Vec3D(pos.x, pos.y, pos.z);
        FaceColors dummyColors = new();
        float luminance = block.getLuminance(ctx.Lighting, pos.x, pos.y, pos.z);

        renderCtx.Tess.setColorOpaque_F(0.5F * luminance, 0.5F * luminance, 0.5F * luminance);
        renderCtx.DrawBottomFace(block, origin, dummyColors, edgeTexture);
        renderCtx.Tess.setColorOpaque_F(luminance, luminance, luminance);
        renderCtx.DrawTopFace(block, origin, dummyColors, edgeTexture);

        renderCtx.Tess.setColorOpaque_F(0.8F * luminance, 0.8F * luminance, 0.8F * luminance);
        renderCtx.DrawEastFace(block, origin, dummyColors, faceTexture);
        renderCtx.DrawWestFace(block, origin, dummyColors, faceTexture);

        renderCtx.Tess.setColorOpaque_F(0.6F * luminance, 0.6F * luminance, 0.6F * luminance);
        renderCtx.DrawNorthFace(block, origin, dummyColors, edgeTexture);
        renderCtx.DrawSouthFace(block, origin, dummyColors, edgeTexture);
    }

    private static void RenderZSegment(Block block, in BlockPos pos, ref BlockRenderContext ctx, float minZ, float maxZ, int faceTexture, int edgeTexture)
    {
        var renderCtx = ctx with { OverrideBounds = new Box(7.0F / 16.0F, 0.0F, minZ, 9.0F / 16.0F, 1.0F, maxZ), EnableAo = false };
        var origin = new Vec3D(pos.x, pos.y, pos.z);
        FaceColors dummyColors = new();
        float luminance = block.getLuminance(ctx.Lighting, pos.x, pos.y, pos.z);

        renderCtx.Tess.setColorOpaque_F(0.5F * luminance, 0.5F * luminance, 0.5F * luminance);
        renderCtx.DrawBottomFace(block, origin, dummyColors, edgeTexture);
        renderCtx.Tess.setColorOpaque_F(luminance, luminance, luminance);
        renderCtx.DrawTopFace(block, origin, dummyColors, edgeTexture);

        renderCtx.Tess.setColorOpaque_F(0.8F * luminance, 0.8F * luminance, 0.8F * luminance);
        renderCtx.DrawEastFace(block, origin, dummyColors, edgeTexture);
        renderCtx.DrawWestFace(block, origin, dummyColors, edgeTexture);

        renderCtx.Tess.setColorOpaque_F(0.6F * luminance, 0.6F * luminance, 0.6F * luminance);
        renderCtx.DrawNorthFace(block, origin, dummyColors, faceTexture);
        renderCtx.DrawSouthFace(block, origin, dummyColors, faceTexture);
    }

    private static bool CanConnectTo(BlockRenderContext ctx, Block block, int x, int y, int z)
    {
        int blockId = ctx.BlockReader.GetBlockId(x, y, z);
        return blockId > 0 && (ctx.BlockReader.ShouldSuffocate(x, y, z) || blockId == block.id || blockId == Block.Glass.id);
    }
}
