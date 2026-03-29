using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelVillager : ModelBase
{
    private readonly ModelPart _head;
    private readonly ModelPart _body;
    private readonly ModelPart _arms;
    private readonly ModelPart _rightLeg;
    private readonly ModelPart _leftLeg;

    public ModelVillager() : this(0.0F, 0.0F)
    {
    }

    public ModelVillager(float expansion, float yOffset)
    {
        _head = new ModelPart(0, 0).setTextureSize(64, 64);
        _head.setTextureOffset(0, 0).addBox(-4.0F, -10.0F, -4.0F, 8, 10, 8, expansion);
        _head.setTextureOffset(24, 0).addBox(-1.0F, -3.0F, -6.0F, 2, 4, 2, expansion);
        _head.setRotationPoint(0.0F, yOffset, 0.0F);

        _body = new ModelPart(16, 20).setTextureSize(64, 64);
        _body.setTextureOffset(16, 20).addBox(-4.0F, 0.0F, -3.0F, 8, 12, 6, expansion);
        _body.setTextureOffset(0, 38).addBox(-4.0F, 0.0F, -3.0F, 8, 18, 6, expansion + 0.5F);
        _body.setRotationPoint(0.0F, yOffset, 0.0F);

        _arms = new ModelPart(44, 22).setTextureSize(64, 64);
        _arms.setTextureOffset(44, 22).addBox(-8.0F, -2.0F, -2.0F, 4, 8, 4, expansion);
        _arms.setTextureOffset(44, 22).addBox(4.0F, -2.0F, -2.0F, 4, 8, 4, expansion);
        _arms.setTextureOffset(40, 38).addBox(-4.0F, 2.0F, -2.0F, 8, 4, 4, expansion);
        _arms.setRotationPoint(0.0F, yOffset + 2.0F, 0.0F);

        _rightLeg = new ModelPart(0, 22).setTextureSize(64, 64);
        _rightLeg.addBox(-2.0F, 0.0F, -2.0F, 4, 12, 4, expansion);
        _rightLeg.setRotationPoint(-2.0F, 12.0F + yOffset, 0.0F);

        _leftLeg = new ModelPart(0, 22).setTextureSize(64, 64);
        _leftLeg.mirror = true;
        _leftLeg.addBox(-2.0F, 0.0F, -2.0F, 4, 12, 4, expansion);
        _leftLeg.setRotationPoint(2.0F, 12.0F + yOffset, 0.0F);
    }

    public override void render(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, tickDelta, yaw, pitch, scale);
        _head.render(scale);
        _body.render(scale);
        _rightLeg.render(scale);
        _leftLeg.render(scale);
        _arms.render(scale);
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        _head.rotateAngleY = yaw / (180.0F / (float)System.Math.PI);
        _head.rotateAngleX = pitch / (180.0F / (float)System.Math.PI);
        _arms.rotationPointY = 3.0F;
        _arms.rotationPointZ = -1.0F;
        _arms.rotateAngleX = -(12.0F / 16.0F);
        _rightLeg.rotateAngleX = MathHelper.Cos(limbAngle * 0.6662F) * 1.4F * limbDistance * 0.5F;
        _leftLeg.rotateAngleX = MathHelper.Cos(limbAngle * 0.6662F + (float)System.Math.PI) * 1.4F * limbDistance * 0.5F;
        _rightLeg.rotateAngleY = 0.0F;
        _leftLeg.rotateAngleY = 0.0F;
    }
}
