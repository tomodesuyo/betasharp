using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelRabbit : ModelBase
{
    private float _jumpRotation;

    private readonly ModelPart _leftFoot;
    private readonly ModelPart _rightFoot;
    private readonly ModelPart _leftThigh;
    private readonly ModelPart _rightThigh;
    private readonly ModelPart _body;
    private readonly ModelPart _leftArm;
    private readonly ModelPart _rightArm;
    private readonly ModelPart _head;
    private readonly ModelPart _rightEar;
    private readonly ModelPart _leftEar;
    private readonly ModelPart _tail;
    private readonly ModelPart _nose;

    public ModelRabbit()
    {
        _leftFoot = new ModelPart(26, 24).setTextureSize(64, 32);
        _leftFoot.addBox(-1.0F, 5.5F, -3.7F, 2, 1, 7);
        _leftFoot.setRotationPoint(3.0F, 17.5F, 3.7F);

        _rightFoot = new ModelPart(8, 24).setTextureSize(64, 32);
        _rightFoot.addBox(-1.0F, 5.5F, -3.7F, 2, 1, 7);
        _rightFoot.setRotationPoint(-3.0F, 17.5F, 3.7F);

        _leftThigh = new ModelPart(30, 15).setTextureSize(64, 32);
        _leftThigh.addBox(-1.0F, 0.0F, 0.0F, 2, 4, 5);
        _leftThigh.setRotationPoint(3.0F, 17.5F, 3.7F);
        _leftThigh.rotateAngleX = -0.34906584F;

        _rightThigh = new ModelPart(16, 15).setTextureSize(64, 32);
        _rightThigh.addBox(-1.0F, 0.0F, 0.0F, 2, 4, 5);
        _rightThigh.setRotationPoint(-3.0F, 17.5F, 3.7F);
        _rightThigh.rotateAngleX = -0.34906584F;

        _body = new ModelPart(0, 0).setTextureSize(64, 32);
        _body.addBox(-3.0F, -2.0F, -10.0F, 6, 5, 10);
        _body.setRotationPoint(0.0F, 19.0F, 8.0F);
        _body.rotateAngleX = -0.34906584F;

        _leftArm = new ModelPart(8, 15).setTextureSize(64, 32);
        _leftArm.addBox(-1.0F, 0.0F, -1.0F, 2, 7, 2);
        _leftArm.setRotationPoint(3.0F, 17.0F, -1.0F);
        _leftArm.rotateAngleX = -0.17453292F;

        _rightArm = new ModelPart(0, 15).setTextureSize(64, 32);
        _rightArm.addBox(-1.0F, 0.0F, -1.0F, 2, 7, 2);
        _rightArm.setRotationPoint(-3.0F, 17.0F, -1.0F);
        _rightArm.rotateAngleX = -0.17453292F;

        _head = new ModelPart(32, 0).setTextureSize(64, 32);
        _head.addBox(-2.5F, -4.0F, -5.0F, 5, 4, 5);
        _head.setRotationPoint(0.0F, 16.0F, -1.0F);

        _rightEar = new ModelPart(52, 0).setTextureSize(64, 32);
        _rightEar.addBox(-2.5F, -9.0F, -1.0F, 2, 5, 1);
        _rightEar.setRotationPoint(0.0F, 16.0F, -1.0F);
        _rightEar.rotateAngleY = -0.2617994F;

        _leftEar = new ModelPart(58, 0).setTextureSize(64, 32);
        _leftEar.addBox(0.5F, -9.0F, -1.0F, 2, 5, 1);
        _leftEar.setRotationPoint(0.0F, 16.0F, -1.0F);
        _leftEar.rotateAngleY = 0.2617994F;

        _tail = new ModelPart(52, 6).setTextureSize(64, 32);
        _tail.addBox(-1.5F, -1.5F, 0.0F, 3, 3, 2);
        _tail.setRotationPoint(0.0F, 20.0F, 7.0F);
        _tail.rotateAngleX = -0.3490659F;

        _nose = new ModelPart(32, 9).setTextureSize(64, 32);
        _nose.addBox(-0.5F, -2.5F, -5.5F, 1, 1, 1);
        _nose.setRotationPoint(0.0F, 16.0F, -1.0F);
    }

    public override void render(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, age, yaw, pitch, scale);
        _leftFoot.render(scale);
        _rightFoot.render(scale);
        _leftThigh.render(scale);
        _rightThigh.render(scale);
        _body.render(scale);
        _leftArm.render(scale);
        _rightArm.render(scale);
        _head.render(scale);
        _rightEar.render(scale);
        _leftEar.render(scale);
        _tail.render(scale);
        _nose.render(scale);
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        float headPitch = pitch / (180.0F / (float)System.Math.PI);
        float headYaw = yaw / (180.0F / (float)System.Math.PI);

        _head.rotateAngleX = headPitch;
        _head.rotateAngleY = headYaw;
        _nose.rotateAngleX = headPitch;
        _nose.rotateAngleY = headYaw;
        _rightEar.rotateAngleX = headPitch;
        _rightEar.rotateAngleY = headYaw - 0.2617994F;
        _leftEar.rotateAngleX = headPitch;
        _leftEar.rotateAngleY = headYaw + 0.2617994F;

        _leftThigh.rotateAngleX = _rightThigh.rotateAngleX = (_jumpRotation * 50.0F - 21.0F) / (180.0F / (float)System.Math.PI);
        _leftFoot.rotateAngleX = _rightFoot.rotateAngleX = _jumpRotation * 50.0F / (180.0F / (float)System.Math.PI);
        _leftArm.rotateAngleX = _rightArm.rotateAngleX = (_jumpRotation * -40.0F - 11.0F) / (180.0F / (float)System.Math.PI);
    }

    public override void setLivingAnimations(EntityLiving entity, float limbAngle, float limbDistance, float tickDelta)
    {
        if (entity is EntityRabbit rabbit)
        {
            _jumpRotation = MathHelper.Sin(rabbit.GetJumpProgress(tickDelta) * (float)System.Math.PI);
        }
        else
        {
            _jumpRotation = 0.0F;
        }
    }
}
