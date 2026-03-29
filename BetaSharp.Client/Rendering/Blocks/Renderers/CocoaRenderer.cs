using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class CocoaRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int meta = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        int age = (meta & 12) >> 2;
        int direction = meta & 3;
        int textureId = block.getTexture(0, meta);
        int width = 4 + age * 2;
        int height = 5 + age * 2;
        int texU = (textureId & 15) << 4;
        int texV = textureId & 240;

        float luminance = block.getLuminance(ctx.Lighting, pos.x, pos.y, pos.z);
        ctx.Tess.setColorOpaque_F(luminance, luminance, luminance);

        double offsetX = 0.0D;
        double offsetZ = 0.0D;

        switch (direction)
        {
            case 0:
                offsetX = 8.0D - width / 2.0D;
                offsetZ = 15.0D - width;
                break;
            case 1:
                offsetX = 1.0D;
                offsetZ = 8.0D - width / 2.0D;
                break;
            case 2:
                offsetX = 8.0D - width / 2.0D;
                offsetZ = 1.0D;
                break;
            case 3:
                offsetX = 15.0D - width;
                offsetZ = 8.0D - width / 2.0D;
                break;
        }

        double minX = pos.x + offsetX / 16.0D;
        double maxX = pos.x + (offsetX + width) / 16.0D;
        double minY = pos.y + (12.0D - height) / 16.0D;
        double maxY = pos.y + 0.75D;
        double minZ = pos.z + offsetZ / 16.0D;
        double maxZ = pos.z + (offsetZ + width) / 16.0D;

        double sideMinU = (texU + 15.0D - width) / 256.0D;
        double sideMaxU = (texU + 15.0D - 0.01D) / 256.0D;
        double sideMinV = (texV + 4.0D) / 256.0D;
        double sideMaxV = (texV + 4.0D + height - 0.01D) / 256.0D;

        AddDoubleSidedQuad(ctx.Tess,
            new Vertex(minX, minY, minZ, sideMinU, sideMaxV),
            new Vertex(minX, minY, maxZ, sideMaxU, sideMaxV),
            new Vertex(minX, maxY, maxZ, sideMaxU, sideMinV),
            new Vertex(minX, maxY, minZ, sideMinU, sideMinV));

        AddDoubleSidedQuad(ctx.Tess,
            new Vertex(maxX, minY, maxZ, sideMinU, sideMaxV),
            new Vertex(maxX, minY, minZ, sideMaxU, sideMaxV),
            new Vertex(maxX, maxY, minZ, sideMaxU, sideMinV),
            new Vertex(maxX, maxY, maxZ, sideMinU, sideMinV));

        AddDoubleSidedQuad(ctx.Tess,
            new Vertex(maxX, minY, minZ, sideMinU, sideMaxV),
            new Vertex(minX, minY, minZ, sideMaxU, sideMaxV),
            new Vertex(minX, maxY, minZ, sideMaxU, sideMinV),
            new Vertex(maxX, maxY, minZ, sideMinU, sideMinV));

        AddDoubleSidedQuad(ctx.Tess,
            new Vertex(minX, minY, maxZ, sideMinU, sideMaxV),
            new Vertex(maxX, minY, maxZ, sideMaxU, sideMaxV),
            new Vertex(maxX, maxY, maxZ, sideMaxU, sideMinV),
            new Vertex(minX, maxY, maxZ, sideMinU, sideMinV));

        int capSize = age >= 2 ? width - 1 : width;
        double capMinU = texU / 256.0D;
        double capMaxU = (texU + capSize - 0.01D) / 256.0D;
        double capMinV = texV / 256.0D;
        double capMaxV = (texV + capSize - 0.01D) / 256.0D;

        AddDoubleSidedQuad(ctx.Tess,
            new Vertex(minX, maxY, maxZ, capMinU, capMaxV),
            new Vertex(maxX, maxY, maxZ, capMaxU, capMaxV),
            new Vertex(maxX, maxY, minZ, capMaxU, capMinV),
            new Vertex(minX, maxY, minZ, capMinU, capMinV));

        AddDoubleSidedQuad(ctx.Tess,
            new Vertex(minX, minY, minZ, capMinU, capMinV),
            new Vertex(maxX, minY, minZ, capMaxU, capMinV),
            new Vertex(maxX, minY, maxZ, capMaxU, capMaxV),
            new Vertex(minX, minY, maxZ, capMinU, capMaxV));

        double stemMinU = (texU + 12.0D) / 256.0D;
        double stemMaxU = (texU + 16.0D - 0.01D) / 256.0D;
        double stemMinV = texV / 256.0D;
        double stemMaxV = (texV + 4.0D - 0.01D) / 256.0D;

        double stemX = 8.0D;
        double stemZ = 0.0D;

        switch (direction)
        {
            case 0:
                stemX = 8.0D;
                stemZ = 12.0D;
                (stemMinU, stemMaxU) = (stemMaxU, stemMinU);
                break;
            case 1:
                stemX = 0.0D;
                stemZ = 8.0D;
                break;
            case 2:
                stemX = 8.0D;
                stemZ = 0.0D;
                break;
            case 3:
                stemX = 12.0D;
                stemZ = 8.0D;
                (stemMinU, stemMaxU) = (stemMaxU, stemMinU);
                break;
        }

        double stemMinX = pos.x + stemX / 16.0D;
        double stemMaxX = pos.x + (stemX + 4.0D) / 16.0D;
        double stemMinY = pos.y + 0.75D;
        double stemMaxY = pos.y + 1.0D;
        double stemMinZ = pos.z + stemZ / 16.0D;
        double stemMaxZ = pos.z + (stemZ + 4.0D) / 16.0D;

        if (direction is 1 or 3)
        {
            AddDoubleSidedQuad(ctx.Tess,
                new Vertex(stemMaxX, stemMinY, stemMinZ, stemMinU, stemMaxV),
                new Vertex(stemMinX, stemMinY, stemMinZ, stemMaxU, stemMaxV),
                new Vertex(stemMinX, stemMaxY, stemMinZ, stemMaxU, stemMinV),
                new Vertex(stemMaxX, stemMaxY, stemMinZ, stemMinU, stemMinV));
        }
        else
        {
            AddDoubleSidedQuad(ctx.Tess,
                new Vertex(stemMinX, stemMinY, stemMinZ, stemMaxU, stemMaxV),
                new Vertex(stemMinX, stemMinY, stemMaxZ, stemMinU, stemMaxV),
                new Vertex(stemMinX, stemMaxY, stemMaxZ, stemMinU, stemMinV),
                new Vertex(stemMinX, stemMaxY, stemMinZ, stemMaxU, stemMinV));
        }

        return true;
    }

    private static void AddDoubleSidedQuad(Tessellator tess, Vertex a, Vertex b, Vertex c, Vertex d)
    {
        tess.addVertexWithUV(a.X, a.Y, a.Z, a.U, a.V);
        tess.addVertexWithUV(b.X, b.Y, b.Z, b.U, b.V);
        tess.addVertexWithUV(c.X, c.Y, c.Z, c.U, c.V);
        tess.addVertexWithUV(d.X, d.Y, d.Z, d.U, d.V);

        tess.addVertexWithUV(d.X, d.Y, d.Z, d.U, d.V);
        tess.addVertexWithUV(c.X, c.Y, c.Z, c.U, c.V);
        tess.addVertexWithUV(b.X, b.Y, b.Z, b.U, b.V);
        tess.addVertexWithUV(a.X, a.Y, a.Z, a.U, a.V);
    }

    private readonly record struct Vertex(double X, double Y, double Z, double U, double V);
}
