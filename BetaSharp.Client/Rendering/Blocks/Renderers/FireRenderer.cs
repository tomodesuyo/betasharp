using BetaSharp.Blocks;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public class FireRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int textureId = ctx.OverrideTexture >= 0 ? ctx.OverrideTexture : block.getTexture(0);
        float luminance = block.getLuminance(ctx.Lighting, pos.x, pos.y, pos.z);
        ctx.Tess.setColorOpaque_F(luminance, luminance, luminance);

        int texU = (textureId & 15) << 4;
        int texV = textureId & 240;
        double minU = texU / 256.0D;
        double maxU = (texU + 15.99F) / 256.0D;
        double minV = texV / 256.0D;
        double maxV = (texV + 15.99F) / 256.0D;
        float flameHeight = 1.4F;

        if (!ctx.BlockReader.ShouldSuffocate(pos.x, pos.y - 1, pos.z) && !Block.Fire.isFlammable(ctx.BlockReader, pos.x, pos.y - 1, pos.z))
        {
            float sideInset = 0.2F;
            float yOffset = 1.0F / 16.0F;

            if ((pos.x + pos.y + pos.z & 1) == 1)
            {
                minU = texU / 256.0D;
                maxU = (texU + 15.99F) / 256.0D;
                minV = (texV + 16) / 256.0D;
                maxV = (texV + 15.99F + 16.0F) / 256.0D;
            }

            if ((pos.x / 2 + pos.y / 2 + pos.z / 2 & 1) == 1)
            {
                (minU, maxU) = (maxU, minU);
            }

            if (Block.Fire.isFlammable(ctx.BlockReader, pos.x - 1, pos.y, pos.z))
            {
                ctx.Tess.addVertexWithUV(pos.x + sideInset, pos.y + flameHeight + yOffset, pos.z + 1, maxU, minV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z + 1, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + sideInset, pos.y + flameHeight + yOffset, pos.z, minU, minV);
                ctx.Tess.addVertexWithUV(pos.x + sideInset, pos.y + flameHeight + yOffset, pos.z, minU, minV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z + 1, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + sideInset, pos.y + flameHeight + yOffset, pos.z + 1, maxU, minV);
            }

            if (Block.Fire.isFlammable(ctx.BlockReader, pos.x + 1, pos.y, pos.z))
            {
                ctx.Tess.addVertexWithUV(pos.x + 1 - sideInset, pos.y + flameHeight + yOffset, pos.z, minU, minV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z + 1, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1 - sideInset, pos.y + flameHeight + yOffset, pos.z + 1, maxU, minV);
                ctx.Tess.addVertexWithUV(pos.x + 1 - sideInset, pos.y + flameHeight + yOffset, pos.z + 1, maxU, minV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z + 1, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1 - sideInset, pos.y + flameHeight + yOffset, pos.z, minU, minV);
            }

            if (Block.Fire.isFlammable(ctx.BlockReader, pos.x, pos.y, pos.z - 1))
            {
                ctx.Tess.addVertexWithUV(pos.x, pos.y + flameHeight + yOffset, pos.z + sideInset, maxU, minV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + flameHeight + yOffset, pos.z + sideInset, minU, minV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + flameHeight + yOffset, pos.z + sideInset, minU, minV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + flameHeight + yOffset, pos.z + sideInset, maxU, minV);
            }

            if (Block.Fire.isFlammable(ctx.BlockReader, pos.x, pos.y, pos.z + 1))
            {
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + flameHeight + yOffset, pos.z + 1 - sideInset, minU, minV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z + 1, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z + 1, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + flameHeight + yOffset, pos.z + 1 - sideInset, maxU, minV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + flameHeight + yOffset, pos.z + 1 - sideInset, maxU, minV);
                ctx.Tess.addVertexWithUV(pos.x, pos.y + yOffset, pos.z + 1, maxU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + yOffset, pos.z + 1, minU, maxV);
                ctx.Tess.addVertexWithUV(pos.x + 1, pos.y + flameHeight + yOffset, pos.z + 1 - sideInset, minU, minV);
            }

            if (Block.Fire.isFlammable(ctx.BlockReader, pos.x, pos.y + 1, pos.z))
            {
                minU = texU / 256.0D;
                maxU = (texU + 15.99F) / 256.0D;
                minV = texV / 256.0D;
                maxV = (texV + 15.99F) / 256.0D;

                int topY = pos.y + 1;
                float topInset = -0.2F;

                if ((pos.x + topY + pos.z & 1) == 0)
                {
                    ctx.Tess.addVertexWithUV(pos.x, topY + topInset, pos.z, maxU, minV);
                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY, pos.z, maxU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY, pos.z + 1.0D, minU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x, topY + topInset, pos.z + 1.0D, minU, minV);

                    minU = texU / 256.0D;
                    maxU = (texU + 15.99F) / 256.0D;
                    minV = (texV + 16) / 256.0D;
                    maxV = (texV + 15.99F + 16.0F) / 256.0D;

                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY + topInset, pos.z + 1.0D, maxU, minV);
                    ctx.Tess.addVertexWithUV(pos.x, topY, pos.z + 1.0D, maxU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x, topY, pos.z, minU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY + topInset, pos.z, minU, minV);
                }
                else
                {
                    ctx.Tess.addVertexWithUV(pos.x, topY + topInset, pos.z + 1.0D, maxU, minV);
                    ctx.Tess.addVertexWithUV(pos.x, topY, pos.z, maxU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY, pos.z, minU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY + topInset, pos.z + 1.0D, minU, minV);

                    minU = texU / 256.0D;
                    maxU = (texU + 15.99F) / 256.0D;
                    minV = (texV + 16) / 256.0D;
                    maxV = (texV + 15.99F + 16.0F) / 256.0D;

                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY + topInset, pos.z, maxU, minV);
                    ctx.Tess.addVertexWithUV(pos.x + 1.0D, topY, pos.z + 1.0D, maxU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x, topY, pos.z + 1.0D, minU, maxV);
                    ctx.Tess.addVertexWithUV(pos.x, topY + topInset, pos.z, minU, minV);
                }
            }
        }
        else
        {
            double x0 = pos.x + 0.7D;
            double x1 = pos.x + 0.3D;
            double z0 = pos.z + 0.7D;
            double z1 = pos.z + 0.3D;
            double x2 = pos.x + 0.2D;
            double x3 = pos.x + 0.8D;
            double z2 = pos.z + 0.2D;
            double z3 = pos.z + 0.8D;

            ctx.Tess.addVertexWithUV(x2, pos.y + flameHeight, pos.z + 1.0D, maxU, minV);
            ctx.Tess.addVertexWithUV(x0, pos.y, pos.z + 1.0D, maxU, maxV);
            ctx.Tess.addVertexWithUV(x0, pos.y, pos.z, minU, maxV);
            ctx.Tess.addVertexWithUV(x2, pos.y + flameHeight, pos.z, minU, minV);
            ctx.Tess.addVertexWithUV(x3, pos.y + flameHeight, pos.z, maxU, minV);
            ctx.Tess.addVertexWithUV(x1, pos.y, pos.z, maxU, maxV);
            ctx.Tess.addVertexWithUV(x1, pos.y, pos.z + 1.0D, minU, maxV);
            ctx.Tess.addVertexWithUV(x3, pos.y + flameHeight, pos.z + 1.0D, minU, minV);

            minU = texU / 256.0D;
            maxU = (texU + 15.99F) / 256.0D;
            minV = (texV + 16) / 256.0D;
            maxV = (texV + 31.99F) / 256.0D;

            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + flameHeight, z3, maxU, minV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y, z1, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x, pos.y, z1, minU, maxV);
            ctx.Tess.addVertexWithUV(pos.x, pos.y + flameHeight, z3, minU, minV);
            ctx.Tess.addVertexWithUV(pos.x, pos.y + flameHeight, z2, maxU, minV);
            ctx.Tess.addVertexWithUV(pos.x, pos.y, z0, maxU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y, z0, minU, maxV);
            ctx.Tess.addVertexWithUV(pos.x + 1.0D, pos.y + flameHeight, z2, minU, minV);

            double x4 = pos.x + 0.0D;
            double x5 = pos.x + 1.0D;
            double z4 = pos.z + 0.0D;
            double z5 = pos.z + 1.0D;
            double x6 = pos.x + 0.1D;
            double x7 = pos.x + 0.9D;
            double z6 = pos.z + 0.1D;
            double z7 = pos.z + 0.9D;

            minU = texU / 256.0D;
            maxU = (texU + 15.99F) / 256.0D;
            minV = texV / 256.0D;
            maxV = (texV + 15.99F) / 256.0D;

            ctx.Tess.addVertexWithUV(x6, pos.y + flameHeight, z4, minU, minV);
            ctx.Tess.addVertexWithUV(x4, pos.y, z4, minU, maxV);
            ctx.Tess.addVertexWithUV(x4, pos.y, z5, maxU, maxV);
            ctx.Tess.addVertexWithUV(x6, pos.y + flameHeight, z5, maxU, minV);
            ctx.Tess.addVertexWithUV(x7, pos.y + flameHeight, z5, minU, minV);
            ctx.Tess.addVertexWithUV(x5, pos.y, z5, minU, maxV);
            ctx.Tess.addVertexWithUV(x5, pos.y, z4, maxU, maxV);
            ctx.Tess.addVertexWithUV(x7, pos.y + flameHeight, z4, maxU, minV);

            minU = texU / 256.0D;
            maxU = (texU + 15.99F) / 256.0D;
            minV = (texV + 16) / 256.0D;
            maxV = (texV + 31.99F) / 256.0D;

            ctx.Tess.addVertexWithUV(x5, pos.y + flameHeight, z7, minU, minV);
            ctx.Tess.addVertexWithUV(x5, pos.y, z1, minU, maxV);
            ctx.Tess.addVertexWithUV(x4, pos.y, z1, maxU, maxV);
            ctx.Tess.addVertexWithUV(x4, pos.y + flameHeight, z7, maxU, minV);
            ctx.Tess.addVertexWithUV(x4, pos.y + flameHeight, z6, minU, minV);
            ctx.Tess.addVertexWithUV(x4, pos.y, z0, minU, maxV);
            ctx.Tess.addVertexWithUV(x5, pos.y, z0, maxU, maxV);
            ctx.Tess.addVertexWithUV(x5, pos.y + flameHeight, z6, maxU, minV);
        }

        return true;
    }
}
