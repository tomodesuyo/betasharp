using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Blocks.Entities;

public class BlockEntityMobSpawnerRenderer : BlockEntitySpecialRenderer
{
    public void renderTileEntityMobSpawner(BlockEntityMobSpawner var1, double var2, double var4, double var6, float var8)
    {
        GLManager.GL.PushMatrix();
        GLManager.GL.Translate((float)var2 + 0.5F, (float)var4, (float)var6 + 0.5F);
        Entity? var9 = var1.GetDisplayEntity();

        if (var9 != null)
        {
            float var10 = 7.0F / 16.0F;
            GLManager.GL.Translate(0.0F, 0.4F, 0.0F);
            GLManager.GL.Rotate((float)(var1.LastRotation + (var1.Rotation - var1.LastRotation) * (double)var8) * 10.0F, 0.0F, 1.0F, 0.0F);
            GLManager.GL.Rotate(-30.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Translate(0.0F, -0.4F, 0.0F);
            GLManager.GL.Scale(var10, var10, var10);
            var9.setPositionAndAnglesKeepPrevAngles(var2, var4, var6, 0.0F, 0.0F);
            EntityRenderDispatcher.instance.renderEntityWithPosYaw(var9, 0.0D, 0.0D, 0.0D, 0.0F, var8);
        }

        GLManager.GL.PopMatrix();
    }

    public override void renderTileEntityAt(BlockEntity blockEntity, double x, double y, double z, float tickDelta)
    {
        renderTileEntityMobSpawner((BlockEntityMobSpawner)blockEntity, x, y, z, tickDelta);
    }
}
