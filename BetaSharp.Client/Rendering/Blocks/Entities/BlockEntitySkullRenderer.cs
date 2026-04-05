using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities;

namespace BetaSharp.Client.Rendering.Blocks.Entities;

public sealed class BlockEntitySkullRenderer : BlockEntitySpecialRenderer
{
    public override void renderTileEntityAt(BlockEntity blockEntity, double x, double y, double z, float tickDelta)
    {
        RenderSkull((BlockEntitySkull)blockEntity, x, y, z, tickDelta);
    }

    private void RenderSkull(BlockEntitySkull skull, double x, double y, double z, float tickDelta)
    {
        int meta = skull.getPushedBlockData() & 7;
        float rotation = meta == 1 ? skull.GetSkullRotation() * 360.0F / 16.0F : 0.0F;

        GLManager.GL.PushMatrix();
        GLManager.GL.Disable(GLEnum.CullFace);

        switch (meta)
        {
            case 2:
                GLManager.GL.Translate((float)x + 0.5F, (float)y + 0.25F, (float)z + 0.74F);
                break;
            case 3:
                GLManager.GL.Translate((float)x + 0.5F, (float)y + 0.25F, (float)z + 0.26F);
                rotation = 180.0F;
                break;
            case 4:
                GLManager.GL.Translate((float)x + 0.74F, (float)y + 0.25F, (float)z + 0.5F);
                rotation = 270.0F;
                break;
            case 5:
                GLManager.GL.Translate((float)x + 0.26F, (float)y + 0.25F, (float)z + 0.5F);
                rotation = 90.0F;
                break;
            default:
                GLManager.GL.Translate((float)x + 0.5F, (float)y, (float)z + 0.5F);
                break;
        }

        GLManager.GL.Enable(GLEnum.RescaleNormal);
        GLManager.GL.Scale(-1.0F, -1.0F, 1.0F);
        SkullRenderHelper.RenderSkull(bindTextureByName, skull.GetSkullType(), rotation, skull.GetAnimationProgress(tickDelta));
        GLManager.GL.Disable(GLEnum.RescaleNormal);
        GLManager.GL.Enable(GLEnum.CullFace);
        GLManager.GL.PopMatrix();
    }
}
