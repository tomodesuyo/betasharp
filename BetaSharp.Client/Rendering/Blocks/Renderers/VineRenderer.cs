using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class VineRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int textureId = block.getTexture(0);
        if (ctx.OverrideTexture >= 0)
        {
            textureId = ctx.OverrideTexture;
        }

        int color = block.getColorMultiplier(ctx.BlockReader, pos.x, pos.y, pos.z);
        float brightness = block.getLuminance(ctx.Lighting, pos.x, pos.y, pos.z);
        float red = (color >> 16 & 255) / 255.0F * brightness;
        float green = (color >> 8 & 255) / 255.0F * brightness;
        float blue = (color & 255) / 255.0F * brightness;
        ctx.Tess.setColorOpaque_F(red, green, blue);

        int texU = (textureId & 15) << 4;
        int texV = textureId & 240;
        float minU = texU / 256.0f;
        float maxU = (texU + 15.99f) / 256.0f;
        float minV = texV / 256.0f;
        float maxV = (texV + 15.99f) / 256.0f;

        int metadata = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        float offset = 1.0F / 16.0F;

        if ((metadata & 2) != 0)
        {
            ctx.Tess.addVertexWithUV(pos.x + offset, pos.y + 1.0D, pos.z + 1.0D, minU, minV);
            ctx.Tess.addVertexWithUV(pos.x + offset, pos.y + 0.0D, pos.z + 1.0D, minU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + offset, pos.y + 0.0D, pos.z + 0.0D, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + offset, pos.y + 1.0D, pos.z + 0.0D, maxU, minV);
        }

        if ((metadata & 8) != 0)
        {
            ctx.Tess.addVertexWithUV(pos.x + 1.0D - offset, pos.y + 0.0D, pos.z + 1.0D, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D - offset, pos.y + 1.0D, pos.z + 1.0D, maxU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D - offset, pos.y + 1.0D, pos.z + 0.0D, minU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D - offset, pos.y + 0.0D, pos.z + 0.0D, minU, maxV);
        }

        if ((metadata & 4) != 0)
        {
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + 0.0D, pos.z + offset, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + 1.0D, pos.z + offset, maxU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 0.0D, pos.y + 1.0D, pos.z + offset, minU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 0.0D, pos.y + 0.0D, pos.z + offset, minU, maxV);
        }

        if ((metadata & 1) != 0)
        {
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + 1.0D, pos.z + 1.0D - offset, minU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + 0.0D, pos.z + 1.0D - offset, minU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 0.0D, pos.y + 0.0D, pos.z + 1.0D - offset, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 0.0D, pos.y + 1.0D, pos.z + 1.0D - offset, maxU, minV);
        }

        if (metadata == 0 && ctx.BlockReader.GetBlockId(pos.x, pos.y + 1, pos.z) == block.id)
        {
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + 1.0D - offset, pos.z + 1.0D, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + 1.0D - offset, pos.z + 0.0D, maxU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 0.0D, pos.y + 1.0D - offset, pos.z + 0.0D, minU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 0.0D, pos.y + 1.0D - offset, pos.z + 1.0D, minU, maxV);
        }

        return true;
    }
}
