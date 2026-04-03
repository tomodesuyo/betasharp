using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class WitherSkullEntityRenderer : EntityRenderer
{
    private readonly ModelPart _head;

    public WitherSkullEntityRenderer()
    {
        _head = new ModelPart(0, 35).setTextureSize(64, 64);
        _head.addBox(-4.0F, -8.0F, -4.0F, 8, 8, 8, 0.0F);
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        EntityWitherSkull skull = (EntityWitherSkull)target;

        GLManager.GL.PushMatrix();
        GLManager.GL.Disable(GLEnum.CullFace);
        GLManager.GL.Translate((float)x, (float)y, (float)z);
        GLManager.GL.Enable(GLEnum.RescaleNormal);
        GLManager.GL.Scale(-1.0F, -1.0F, 1.0F);

        float renderYaw = InterpolateAngle(skull.prevYaw, skull.yaw, tickDelta);
        float renderPitch = skull.prevPitch + (skull.pitch - skull.prevPitch) * tickDelta;
        _head.rotateAngleY = renderYaw * (float)Math.PI / 180.0F;
        _head.rotateAngleX = renderPitch * (float)Math.PI / 180.0F;

        loadTexture(skull.IsInvulnerable() ? "/mob/wither/wither_invulnerable.png" : "/mob/wither/wither.png");
        _head.renderWithRotation(0.0625F);

        GLManager.GL.Enable(GLEnum.CullFace);
        GLManager.GL.PopMatrix();
    }

    private static float InterpolateAngle(float start, float end, float delta)
    {
        float difference = end - start;
        while (difference < -180.0F)
        {
            difference += 360.0F;
        }

        while (difference >= 180.0F)
        {
            difference -= 360.0F;
        }

        return start + delta * difference;
    }
}
