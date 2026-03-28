using BetaSharp.Blocks;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class LilyPadRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int textureId = ctx.OverrideTexture >= 0 ? ctx.OverrideTexture : block.getTexture(0, ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z));
        int texU = (textureId & 15) << 4;
        int texV = textureId & 240;
        double minU = texU / 256.0;
        double maxU = (texU + 15.99F) / 256.0;
        double minV = texV / 256.0;
        double maxV = (texV + 15.99F) / 256.0;

        long hash = pos.x * 3129871L ^ pos.z * 116129781L ^ pos.y;
        hash = hash * hash * 42317861L + hash * 11L;
        int rotation = (int)(hash >> 16 & 3L);

        float luminance = block.getLuminance(ctx.Lighting, pos.x, pos.y, pos.z);
        int color = block.getColorMultiplier(ctx.BlockReader, pos.x, pos.y, pos.z);
        float red = (color >> 16 & 255) / 255.0F * luminance;
        float green = (color >> 8 & 255) / 255.0F * luminance;
        float blue = (color & 255) / 255.0F * luminance;
        float x = pos.x + 0.5F;
        float y = pos.y + 0.015625F;
        float z = pos.z + 0.5F;
        float offsetX = (rotation & 1) * 0.5F * (1 - rotation / 2 % 2 * 2);
        float offsetZ = (rotation + 1 & 1) * 0.5F * (1 - (rotation + 1) / 2 % 2 * 2);

        ctx.Tess.setColorOpaque_F(red, green, blue);
        ctx.Tess.addVertexWithUV(x + offsetX - offsetZ, y, z + offsetX + offsetZ, minU, minV);
        ctx.Tess.addVertexWithUV(x + offsetX + offsetZ, y, z - offsetX + offsetZ, maxU, minV);
        ctx.Tess.addVertexWithUV(x - offsetX + offsetZ, y, z - offsetX - offsetZ, maxU, maxV);
        ctx.Tess.addVertexWithUV(x - offsetX - offsetZ, y, z + offsetX - offsetZ, minU, maxV);

        ctx.Tess.setColorOpaque_F(red * 0.5F, green * 0.5F, blue * 0.5F);
        ctx.Tess.addVertexWithUV(x - offsetX - offsetZ, y, z + offsetX - offsetZ, minU, maxV);
        ctx.Tess.addVertexWithUV(x - offsetX + offsetZ, y, z - offsetX - offsetZ, maxU, maxV);
        ctx.Tess.addVertexWithUV(x + offsetX + offsetZ, y, z - offsetX + offsetZ, maxU, minV);
        ctx.Tess.addVertexWithUV(x + offsetX - offsetZ, y, z + offsetX + offsetZ, minU, minV);
        return true;
    }
}
