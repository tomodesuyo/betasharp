using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelEnderCrystal : ModelBase
{
    private readonly ModelPart _glass;
    private readonly ModelPart _cube;
    private readonly ModelPart _base;

    public ModelEnderCrystal(float expansion)
    {
        _glass = new ModelPart(0, 0).setTextureSize(64, 32);
        _glass.addBox(-4.0F, -4.0F, -4.0F, 8, 8, 8, expansion);

        _cube = new ModelPart(32, 0).setTextureSize(64, 32);
        _cube.addBox(-4.0F, -4.0F, -4.0F, 8, 8, 8, expansion);

        _base = new ModelPart(0, 16).setTextureSize(64, 32);
        _base.addBox(-6.0F, 0.0F, -6.0F, 12, 4, 12, expansion);
    }

    public override void render(float limbAngle, float rotation, float bob, float yaw, float pitch, float scale)
    {
        GLManager.GL.PushMatrix();
        GLManager.GL.Scale(2.0F, 2.0F, 2.0F);
        GLManager.GL.Translate(0.0F, -0.5F, 0.0F);
        _base.render(scale);
        GLManager.GL.Rotate(rotation, 0.0F, 1.0F, 0.0F);
        GLManager.GL.Translate(0.0F, 0.8F + bob, 0.0F);
        GLManager.GL.Rotate(60.0F, 0.7071F, 0.0F, 0.7071F);
        _glass.render(scale);
        const float innerScale = 14.0F / 16.0F;
        GLManager.GL.Scale(innerScale, innerScale, innerScale);
        GLManager.GL.Rotate(60.0F, 0.7071F, 0.0F, 0.7071F);
        GLManager.GL.Rotate(rotation, 0.0F, 1.0F, 0.0F);
        _glass.render(scale);
        GLManager.GL.Scale(innerScale, innerScale, innerScale);
        GLManager.GL.Rotate(60.0F, 0.7071F, 0.0F, 0.7071F);
        GLManager.GL.Rotate(rotation, 0.0F, 1.0F, 0.0F);
        _cube.render(scale);
        GLManager.GL.PopMatrix();
    }
}
