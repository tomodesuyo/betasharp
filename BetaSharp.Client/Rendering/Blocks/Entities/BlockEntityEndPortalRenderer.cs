using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;

namespace BetaSharp.Client.Rendering.Blocks.Entities;

public sealed class BlockEntityEndPortalRenderer : BlockEntitySpecialRenderer
{
    public override void renderTileEntityAt(BlockEntity blockEntity, double x, double y, double z, float tickDelta)
    {
        GLManager.GL.Disable(GLEnum.Lighting);
        GLManager.GL.Disable(GLEnum.CullFace);
        GLManager.GL.DepthMask(false);

        float surfaceY = (float)(y + 12.0D / 16.0D);
        double time = Environment.TickCount64 / 1000.0D;
        Tessellator tess = Tessellator.instance;
        Random random = new(31100);

        for (int pass = 0; pass < 16; ++pass)
        {
            bindTextureByName(pass == 0 ? "/misc/tunnel.png" : "/misc/particlefield.png");
            GLManager.GL.Enable(GLEnum.Blend);
            GLManager.GL.BlendFunc(pass == 0 ? GLEnum.SrcAlpha : GLEnum.One, pass == 0 ? GLEnum.OneMinusSrcAlpha : GLEnum.One);

            GLManager.GL.MatrixMode(GLEnum.Texture);
            GLManager.GL.PushMatrix();
            GLManager.GL.LoadIdentity();
            float layer = 16 - pass;
            float scale = 1.0F / (layer + 1.0F);
            float brightness = 1.0F / (layer + 1.0F);
            if (pass == 0)
            {
                scale = 0.125F;
                brightness = 0.1F;
            }
            else if (pass == 1)
            {
                scale = 0.5F;
            }

            GLManager.GL.Translate(0.0F, (float)(time % 700.0D) / 700.0F, 0.0F);
            GLManager.GL.Scale(scale, scale, scale);
            GLManager.GL.Translate(0.5F, 0.5F, 0.0F);
            GLManager.GL.Rotate((pass * pass * 4321 + pass * 9) * 2.0F, 0.0F, 0.0F, 1.0F);
            GLManager.GL.Translate(-0.5F, -0.5F, 0.0F);
            GLManager.GL.Translate((float)-(x + 0.5D), (float)-(z + 0.5D), 0.0F);

            float red = random.NextSingle() * 0.5F + 0.1F;
            float green = random.NextSingle() * 0.5F + 0.4F;
            float blue = random.NextSingle() * 0.5F + 0.5F;
            if (pass == 0)
            {
                red = 1.0F;
                green = 1.0F;
                blue = 1.0F;
            }

            tess.startDrawingQuads();
            tess.setColorRGBA_F(red * brightness, green * brightness, blue * brightness, 1.0F);
            tess.addVertexWithUV(x, surfaceY, z, x, z);
            tess.addVertexWithUV(x, surfaceY, z + 1.0D, x, z + 1.0D);
            tess.addVertexWithUV(x + 1.0D, surfaceY, z + 1.0D, x + 1.0D, z + 1.0D);
            tess.addVertexWithUV(x + 1.0D, surfaceY, z, x + 1.0D, z);
            tess.draw();

            GLManager.GL.PopMatrix();
            GLManager.GL.MatrixMode(GLEnum.Modelview);
        }

        GLManager.GL.Disable(GLEnum.Blend);
        GLManager.GL.Enable(GLEnum.CullFace);
        GLManager.GL.DepthMask(true);
        GLManager.GL.Enable(GLEnum.Lighting);
    }
}
