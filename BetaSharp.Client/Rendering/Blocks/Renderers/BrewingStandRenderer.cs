using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;
using BetaSharp.Client.Rendering.Blocks;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class BrewingStandRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        (ctx with { OverrideBounds = new Box(7.0F / 16.0F, 0.0F, 7.0F / 16.0F, 9.0F / 16.0F, 14.0F / 16.0F, 9.0F / 16.0F) }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(9.0F / 16.0F, 0.0F, 5.0F / 16.0F, 15.0F / 16.0F, 2.0F / 16.0F, 11.0F / 16.0F), OverrideTexture = 156 }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(2.0F / 16.0F, 0.0F, 1.0F / 16.0F, 0.5F, 2.0F / 16.0F, 7.0F / 16.0F), OverrideTexture = 156 }).DrawBlock(block, pos);
        (ctx with { OverrideBounds = new Box(2.0F / 16.0F, 0.0F, 9.0F / 16.0F, 0.5F, 2.0F / 16.0F, 15.0F / 16.0F), OverrideTexture = 156 }).DrawBlock(block, pos);

        int color = block.getColorMultiplier(ctx.BlockReader, pos.x, pos.y, pos.z);
        float red = (color >> 16 & 255) / 255.0F;
        float green = (color >> 8 & 255) / 255.0F;
        float blue = (color & 255) / 255.0F;
        int textureId = block.getTexture(0, 0);
        int texU = (textureId & 15) << 4;
        int texV = textureId & 240;
        double v0 = texV / 256.0D;
        double v1 = (texV + 15.99F) / 256.0D;
        int meta = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);

        ctx.Tess.setLight(15, 15);
        ctx.Tess.setColorOpaque_F(red, green, blue);
        for (int i = 0; i < 3; ++i)
        {
            double angle = i * Math.PI * 2.0D / 3.0D + Math.PI * 0.5D;
            double u0 = (texU + 8.0F) / 256.0D;
            double u1 = (texU + 15.99F) / 256.0D;
            if ((meta & 1 << i) != 0)
            {
                u0 = (texU + 7.99F) / 256.0D;
                u1 = texU / 256.0D;
            }

            double centerX = pos.x + 0.5D;
            double outerX = centerX + Math.Sin(angle) * 8.0D / 16.0D;
            double centerZ = pos.z + 0.5D;
            double outerZ = centerZ + Math.Cos(angle) * 8.0D / 16.0D;

            ctx.Tess.addVertexWithUV(centerX, pos.y + 1.0D, centerZ, u0, v0);
            ctx.Tess.addVertexWithUV(centerX, pos.y + 0.0D, centerZ, u0, v1);
            ctx.Tess.addVertexWithUV(outerX, pos.y + 0.0D, outerZ, u1, v1);
            ctx.Tess.addVertexWithUV(outerX, pos.y + 1.0D, outerZ, u1, v0);
            ctx.Tess.addVertexWithUV(outerX, pos.y + 1.0D, outerZ, u1, v0);
            ctx.Tess.addVertexWithUV(outerX, pos.y + 0.0D, outerZ, u1, v1);
            ctx.Tess.addVertexWithUV(centerX, pos.y + 0.0D, centerZ, u0, v1);
            ctx.Tess.addVertexWithUV(centerX, pos.y + 1.0D, centerZ, u0, v0);
        }

        return true;
    }
}
