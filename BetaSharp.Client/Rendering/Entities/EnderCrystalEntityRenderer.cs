using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class EnderCrystalEntityRenderer : EntityRenderer
{
    private readonly ModelEnderCrystal _model = new(0.0F);

    public EnderCrystalEntityRenderer()
    {
        ShadowRadius = 0.5F;
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        EntityEnderCrystal crystal = (EntityEnderCrystal)target;
        float rotation = crystal.InnerRotation + tickDelta;
        float bob = MathHelper.Sin(rotation * 0.2F) / 2.0F + 0.5F;
        bob += bob * bob;

        GLManager.GL.PushMatrix();
        GLManager.GL.Translate((float)x, (float)y, (float)z);
        loadTexture("/mob/enderdragon/crystal.png");
        _model.render(0.0F, rotation * 3.0F, bob * 0.2F, 0.0F, 0.0F, 1.0F / 16.0F);
        GLManager.GL.PopMatrix();
    }
}
