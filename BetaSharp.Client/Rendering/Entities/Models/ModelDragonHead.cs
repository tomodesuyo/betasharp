using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelDragonHead : ModelBase
{
    private readonly ModelPart _head;
    private readonly ModelPart _jaw;

    public ModelDragonHead()
    {
        _head = new ModelPart(0, 0).setTextureSize(256, 256);
        _head.setTextureOffset(176, 44).addBox(-6.0F, -1.0F, -24.0F, 12, 5, 16);
        _head.setTextureOffset(112, 30).addBox(-8.0F, -8.0F, -10.0F, 16, 16, 16);
        _head.setTextureOffset(0, 0).addBox(-5.0F, -12.0F, -4.0F, 2, 4, 6);
        _head.setTextureOffset(112, 0).addBox(-5.0F, -3.0F, -22.0F, 2, 2, 4);
        _head.setTextureOffset(0, 0).addBox(3.0F, -12.0F, -4.0F, 2, 4, 6);
        _head.setTextureOffset(112, 0).addBox(3.0F, -3.0F, -22.0F, 2, 2, 4);

        _jaw = new ModelPart(176, 65).setTextureSize(256, 256);
        _jaw.addBox(-6.0F, 0.0F, -16.0F, 12, 4, 16);
        _jaw.setRotationPoint(0.0F, 4.0F, -8.0F);
        _head.AddChild(_jaw);
    }

    public void RenderHead(float animateTicks, float yawDegrees, float pitchDegrees, float scale)
    {
        _jaw.rotateAngleX = ((float)System.Math.Sin(animateTicks * (float)System.Math.PI * 0.2F) + 1.0F) * 0.2F;
        _head.rotateAngleY = yawDegrees * 0.017453292F;
        _head.rotateAngleX = pitchDegrees * 0.017453292F;

        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(0.0F, -0.374375F, 0.0F);
        GLManager.GL.Scale(0.75F, 0.75F, 0.75F);
        _head.render(scale);
        GLManager.GL.PopMatrix();
    }

    public override void render(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float scale)
    {
        RenderHead(limbSwing, netHeadYaw, headPitch, scale);
    }
}
