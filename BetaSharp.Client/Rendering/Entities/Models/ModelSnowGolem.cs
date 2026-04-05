using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelSnowGolem : ModelBase
{
    private readonly ModelPart _head;
    private readonly ModelPart _leftHand;
    private readonly ModelPart _rightHand;
    private readonly ModelPart _body;
    private readonly ModelPart _bottomBody;

    public ModelSnowGolem()
    {
        _head = new ModelPart(0, 0).setTextureSize(64, 64);
        _head.addBox(-4.0F, -8.0F, -4.0F, 8, 8, 8, -0.5F);
        _head.setRotationPoint(0.0F, 4.0F, 0.0F);

        _rightHand = new ModelPart(32, 0).setTextureSize(64, 64);
        _rightHand.addBox(-1.0F, 0.0F, -1.0F, 12, 2, 2, -0.5F);
        _rightHand.setRotationPoint(0.0F, 6.0F, 0.0F);

        _leftHand = new ModelPart(32, 0).setTextureSize(64, 64);
        _leftHand.addBox(-1.0F, 0.0F, -1.0F, 12, 2, 2, -0.5F);
        _leftHand.setRotationPoint(0.0F, 6.0F, 0.0F);

        _body = new ModelPart(0, 16).setTextureSize(64, 64);
        _body.addBox(-5.0F, -10.0F, -5.0F, 10, 10, 10, -0.5F);
        _body.setRotationPoint(0.0F, 13.0F, 0.0F);

        _bottomBody = new ModelPart(0, 36).setTextureSize(64, 64);
        _bottomBody.addBox(-6.0F, -12.0F, -6.0F, 12, 12, 12, -0.5F);
        _bottomBody.setRotationPoint(0.0F, 24.0F, 0.0F);
    }

    public override void render(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, tickDelta, yaw, pitch, scale);
        _body.render(scale);
        _bottomBody.render(scale);
        _head.render(scale);
        _rightHand.render(scale);
        _leftHand.render(scale);
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        _head.rotateAngleY = yaw * 0.017453292F;
        _head.rotateAngleX = pitch * 0.017453292F;
        _body.rotateAngleY = _head.rotateAngleY * 0.25F;

        float sin = MathHelper.Sin(_body.rotateAngleY);
        float cos = MathHelper.Cos(_body.rotateAngleY);
        _rightHand.rotateAngleZ = 1.0F;
        _leftHand.rotateAngleZ = -1.0F;
        _rightHand.rotateAngleY = _body.rotateAngleY;
        _leftHand.rotateAngleY = (float)Math.PI + _body.rotateAngleY;
        _rightHand.rotationPointX = cos * 5.0F;
        _rightHand.rotationPointZ = -sin * 5.0F;
        _leftHand.rotationPointX = -cos * 5.0F;
        _leftHand.rotationPointZ = sin * 5.0F;
    }

    public ModelPart HeadPart => _head;
}
