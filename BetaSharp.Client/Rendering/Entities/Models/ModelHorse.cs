using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelHorse : ModelBase
{
    private const float Deg2Rad = (float)System.Math.PI / 180.0F;
    private const float HalfPi = (float)System.Math.PI / 2.0F;
    private const float ThreeHalvesPi = (float)System.Math.PI * 1.5F;

    private readonly ModelPart _head;
    private readonly ModelPart _upperMouth;
    private readonly ModelPart _lowerMouth;
    private readonly ModelPart _leftEar;
    private readonly ModelPart _rightEar;
    private readonly ModelPart _neck;
    private readonly ModelPart _faceRopes;
    private readonly ModelPart _mane;
    private readonly ModelPart _body;
    private readonly ModelPart _tailBase;
    private readonly ModelPart _tailMiddle;
    private readonly ModelPart _tailTip;
    private readonly ModelPart _backLeftLeg;
    private readonly ModelPart _backLeftShin;
    private readonly ModelPart _backLeftHoof;
    private readonly ModelPart _backRightLeg;
    private readonly ModelPart _backRightShin;
    private readonly ModelPart _backRightHoof;
    private readonly ModelPart _frontLeftLeg;
    private readonly ModelPart _frontLeftShin;
    private readonly ModelPart _frontLeftHoof;
    private readonly ModelPart _frontRightLeg;
    private readonly ModelPart _frontRightShin;
    private readonly ModelPart _frontRightHoof;
    private readonly ModelPart _saddleBottom;
    private readonly ModelPart _saddleFront;
    private readonly ModelPart _saddleBack;
    private readonly ModelPart _leftSaddleRope;
    private readonly ModelPart _leftSaddleMetal;
    private readonly ModelPart _rightSaddleRope;
    private readonly ModelPart _rightSaddleMetal;
    private readonly ModelPart _leftFaceMetal;
    private readonly ModelPart _rightFaceMetal;
    private readonly ModelPart _leftRein;
    private readonly ModelPart _rightRein;

    private bool _showSaddle;
    private bool _showReins;

    public ModelHorse()
    {
        _body = CreatePart(0, 34);
        _body.addBox(-5.0F, -8.0F, -19.0F, 10, 10, 24);
        _body.setRotationPoint(0.0F, 11.0F, 9.0F);

        _tailBase = CreatePart(44, 0);
        _tailBase.addBox(-1.0F, -1.0F, 0.0F, 2, 2, 3);
        _tailBase.setRotationPoint(0.0F, 3.0F, 14.0F);
        SetRotation(_tailBase, -1.134464F, 0.0F, 0.0F);

        _tailMiddle = CreatePart(38, 7);
        _tailMiddle.addBox(-1.5F, -2.0F, 3.0F, 3, 4, 7);
        _tailMiddle.setRotationPoint(0.0F, 3.0F, 14.0F);
        SetRotation(_tailMiddle, -1.134464F, 0.0F, 0.0F);

        _tailTip = CreatePart(24, 3);
        _tailTip.addBox(-1.5F, -4.5F, 9.0F, 3, 4, 7);
        _tailTip.setRotationPoint(0.0F, 3.0F, 14.0F);
        SetRotation(_tailTip, -1.40215F, 0.0F, 0.0F);

        _backLeftLeg = CreatePart(78, 29);
        _backLeftLeg.addBox(-2.5F, -2.0F, -2.5F, 4, 9, 5);
        _backLeftLeg.setRotationPoint(4.0F, 9.0F, 11.0F);

        _backLeftShin = CreatePart(78, 43);
        _backLeftShin.addBox(-2.0F, 0.0F, -1.5F, 3, 5, 3);
        _backLeftShin.setRotationPoint(4.0F, 16.0F, 11.0F);

        _backLeftHoof = CreatePart(78, 51);
        _backLeftHoof.addBox(-2.5F, 5.1F, -2.0F, 4, 3, 4);
        _backLeftHoof.setRotationPoint(4.0F, 16.0F, 11.0F);

        _backRightLeg = CreatePart(96, 29);
        _backRightLeg.addBox(-1.5F, -2.0F, -2.5F, 4, 9, 5);
        _backRightLeg.setRotationPoint(-4.0F, 9.0F, 11.0F);

        _backRightShin = CreatePart(96, 43);
        _backRightShin.addBox(-1.0F, 0.0F, -1.5F, 3, 5, 3);
        _backRightShin.setRotationPoint(-4.0F, 16.0F, 11.0F);

        _backRightHoof = CreatePart(96, 51);
        _backRightHoof.addBox(-1.5F, 5.1F, -2.0F, 4, 3, 4);
        _backRightHoof.setRotationPoint(-4.0F, 16.0F, 11.0F);

        _frontLeftLeg = CreatePart(44, 29);
        _frontLeftLeg.addBox(-1.9F, -1.0F, -2.1F, 3, 8, 4);
        _frontLeftLeg.setRotationPoint(4.0F, 9.0F, -8.0F);

        _frontLeftShin = CreatePart(44, 41);
        _frontLeftShin.addBox(-1.9F, 0.0F, -1.6F, 3, 5, 3);
        _frontLeftShin.setRotationPoint(4.0F, 16.0F, -8.0F);

        _frontLeftHoof = CreatePart(44, 51);
        _frontLeftHoof.addBox(-2.4F, 5.1F, -2.1F, 4, 3, 4);
        _frontLeftHoof.setRotationPoint(4.0F, 16.0F, -8.0F);

        _frontRightLeg = CreatePart(60, 29);
        _frontRightLeg.addBox(-1.1F, -1.0F, -2.1F, 3, 8, 4);
        _frontRightLeg.setRotationPoint(-4.0F, 9.0F, -8.0F);

        _frontRightShin = CreatePart(60, 41);
        _frontRightShin.addBox(-1.1F, 0.0F, -1.6F, 3, 5, 3);
        _frontRightShin.setRotationPoint(-4.0F, 16.0F, -8.0F);

        _frontRightHoof = CreatePart(60, 51);
        _frontRightHoof.addBox(-1.6F, 5.1F, -2.1F, 4, 3, 4);
        _frontRightHoof.setRotationPoint(-4.0F, 16.0F, -8.0F);

        _head = CreatePart(0, 0);
        _head.addBox(-2.5F, -10.0F, -1.5F, 5, 5, 7);
        _head.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_head, 0.5235988F, 0.0F, 0.0F);

        _upperMouth = CreatePart(24, 18);
        _upperMouth.addBox(-2.0F, -10.0F, -7.0F, 4, 3, 6);
        _upperMouth.setRotationPoint(0.0F, 3.95F, -10.0F);
        SetRotation(_upperMouth, 0.5235988F, 0.0F, 0.0F);

        _lowerMouth = CreatePart(24, 27);
        _lowerMouth.addBox(-2.0F, -7.0F, -6.5F, 4, 2, 5);
        _lowerMouth.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_lowerMouth, 0.5235988F, 0.0F, 0.0F);

        _leftEar = CreatePart(0, 0);
        _leftEar.addBox(0.45F, -12.0F, 4.0F, 2, 3, 1);
        _leftEar.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_leftEar, 0.5235988F, 0.0F, 0.0F);

        _rightEar = CreatePart(0, 0);
        _rightEar.addBox(-2.45F, -12.0F, 4.0F, 2, 3, 1);
        _rightEar.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_rightEar, 0.5235988F, 0.0F, 0.0F);

        _neck = CreatePart(0, 12);
        _neck.addBox(-2.05F, -9.8F, -2.0F, 4, 14, 8);
        _neck.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_neck, 0.5235988F, 0.0F, 0.0F);

        _saddleBottom = CreatePart(80, 0);
        _saddleBottom.addBox(-5.0F, 0.0F, -3.0F, 10, 1, 8);
        _saddleBottom.setRotationPoint(0.0F, 2.0F, 2.0F);

        _saddleFront = CreatePart(106, 9);
        _saddleFront.addBox(-1.5F, -1.0F, -3.0F, 3, 1, 2);
        _saddleFront.setRotationPoint(0.0F, 2.0F, 2.0F);

        _saddleBack = CreatePart(80, 9);
        _saddleBack.addBox(-4.0F, -1.0F, 3.0F, 8, 1, 2);
        _saddleBack.setRotationPoint(0.0F, 2.0F, 2.0F);

        _leftSaddleMetal = CreatePart(74, 0);
        _leftSaddleMetal.addBox(-0.5F, 6.0F, -1.0F, 1, 2, 2);
        _leftSaddleMetal.setRotationPoint(5.0F, 3.0F, 2.0F);

        _leftSaddleRope = CreatePart(70, 0);
        _leftSaddleRope.addBox(-0.5F, 0.0F, -0.5F, 1, 6, 1);
        _leftSaddleRope.setRotationPoint(5.0F, 3.0F, 2.0F);

        _rightSaddleMetal = CreatePart(74, 4);
        _rightSaddleMetal.addBox(-0.5F, 6.0F, -1.0F, 1, 2, 2);
        _rightSaddleMetal.setRotationPoint(-5.0F, 3.0F, 2.0F);

        _rightSaddleRope = CreatePart(80, 0);
        _rightSaddleRope.addBox(-0.5F, 0.0F, -0.5F, 1, 6, 1);
        _rightSaddleRope.setRotationPoint(-5.0F, 3.0F, 2.0F);

        _leftFaceMetal = CreatePart(74, 13);
        _leftFaceMetal.addBox(1.5F, -8.0F, -4.0F, 1, 2, 2);
        _leftFaceMetal.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_leftFaceMetal, 0.5235988F, 0.0F, 0.0F);

        _rightFaceMetal = CreatePart(74, 13);
        _rightFaceMetal.addBox(-2.5F, -8.0F, -4.0F, 1, 2, 2);
        _rightFaceMetal.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_rightFaceMetal, 0.5235988F, 0.0F, 0.0F);

        _leftRein = CreatePart(44, 10);
        _leftRein.addBox(2.6F, -6.0F, -6.0F, 0, 3, 16);
        _leftRein.setRotationPoint(0.0F, 4.0F, -10.0F);

        _rightRein = CreatePart(44, 5);
        _rightRein.addBox(-2.6F, -6.0F, -6.0F, 0, 3, 16);
        _rightRein.setRotationPoint(0.0F, 4.0F, -10.0F);

        _mane = CreatePart(58, 0);
        _mane.addBox(-1.0F, -11.5F, 5.0F, 2, 16, 4);
        _mane.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_mane, 0.5235988F, 0.0F, 0.0F);

        _faceRopes = CreatePart(80, 12);
        _faceRopes.addBox(-2.5F, -10.1F, -7.0F, 5, 5, 12, 0.2F);
        _faceRopes.setRotationPoint(0.0F, 4.0F, -10.0F);
        SetRotation(_faceRopes, 0.5235988F, 0.0F, 0.0F);
    }

    public override void render(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, age, yaw, pitch, scale);

        if (_showSaddle)
        {
            _faceRopes.render(scale);
            _saddleBottom.render(scale);
            _saddleFront.render(scale);
            _saddleBack.render(scale);
            _leftSaddleRope.render(scale);
            _leftSaddleMetal.render(scale);
            _rightSaddleRope.render(scale);
            _rightSaddleMetal.render(scale);
            _leftFaceMetal.render(scale);
            _rightFaceMetal.render(scale);

            if (_showReins)
            {
                _leftRein.render(scale);
                _rightRein.render(scale);
            }
        }

        _backLeftLeg.render(scale);
        _backLeftShin.render(scale);
        _backLeftHoof.render(scale);
        _backRightLeg.render(scale);
        _backRightShin.render(scale);
        _backRightHoof.render(scale);
        _frontLeftLeg.render(scale);
        _frontLeftShin.render(scale);
        _frontLeftHoof.render(scale);
        _frontRightLeg.render(scale);
        _frontRightShin.render(scale);
        _frontRightHoof.render(scale);
        _body.render(scale);
        _tailBase.render(scale);
        _tailMiddle.render(scale);
        _tailTip.render(scale);
        _neck.render(scale);
        _mane.render(scale);
        _leftEar.render(scale);
        _rightEar.render(scale);
        _head.render(scale);
        _upperMouth.render(scale);
        _lowerMouth.render(scale);
    }

    public override void setLivingAnimations(EntityLiving entity, float limbAngle, float limbDistance, float tickDelta)
    {
        if (entity is EntityHorse horse)
        {
            _showSaddle = horse.Saddled.Value;
            _showReins = _showSaddle && horse.passenger != null;
        }
        else
        {
            _showSaddle = false;
            _showReins = false;
        }
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        float clampedHeadYaw = Math.Clamp(yaw, -20.0F, 20.0F) * Deg2Rad;
        float headPitch = 0.5235988F + pitch * Deg2Rad;
        if (limbDistance > 0.2F)
        {
            headPitch += MathHelper.Cos(limbAngle * 0.4F) * 0.15F * limbDistance;
        }

        float legSwing = MathHelper.Cos(limbAngle * 0.6662F + (float)System.Math.PI);
        float frontSwing = legSwing * 0.8F * limbDistance;

        _body.rotateAngleX = 0.0F;

        ApplyHeadPose(_head, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_upperMouth, 3.95F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_lowerMouth, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_leftEar, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_rightEar, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_neck, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_mane, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_faceRopes, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_leftFaceMetal, 4.0F, -10.0F, headPitch, clampedHeadYaw);
        ApplyHeadPose(_rightFaceMetal, 4.0F, -10.0F, headPitch, clampedHeadYaw);

        _leftRein.rotationPointY = _rightRein.rotationPointY = 4.0F;
        _leftRein.rotationPointZ = _rightRein.rotationPointZ = -10.0F;
        _leftRein.rotateAngleX = _rightRein.rotateAngleX = pitch * Deg2Rad;
        _leftRein.rotateAngleY = _rightRein.rotateAngleY = clampedHeadYaw;

        _backLeftLeg.rotateAngleX = -legSwing * 0.5F * limbDistance;
        _backRightLeg.rotateAngleX = legSwing * 0.5F * limbDistance;

        float backLeftStep = -legSwing * 0.5F * limbDistance;
        float backRightStep = legSwing * 0.5F * limbDistance;
        _backLeftShin.rotateAngleX = backLeftStep - System.MathF.Max(0.0F, -backLeftStep);
        _backRightShin.rotateAngleX = backRightStep - System.MathF.Max(0.0F, -backRightStep);
        _backLeftHoof.rotateAngleX = _backLeftShin.rotateAngleX;
        _backRightHoof.rotateAngleX = _backRightShin.rotateAngleX;

        _backLeftShin.rotationPointY = _backLeftLeg.rotationPointY + MathHelper.Sin(HalfPi + backLeftStep) * 7.0F;
        _backLeftShin.rotationPointZ = _backLeftLeg.rotationPointZ + MathHelper.Cos(ThreeHalvesPi + backLeftStep) * 7.0F;
        _backRightShin.rotationPointY = _backRightLeg.rotationPointY + MathHelper.Sin(HalfPi + backRightStep) * 7.0F;
        _backRightShin.rotationPointZ = _backRightLeg.rotationPointZ + MathHelper.Cos(ThreeHalvesPi + backRightStep) * 7.0F;
        _backLeftHoof.rotationPointY = _backLeftShin.rotationPointY;
        _backLeftHoof.rotationPointZ = _backLeftShin.rotationPointZ;
        _backRightHoof.rotationPointY = _backRightShin.rotationPointY;
        _backRightHoof.rotationPointZ = _backRightShin.rotationPointZ;

        _frontLeftLeg.rotationPointY = _frontRightLeg.rotationPointY = 9.0F;
        _frontLeftLeg.rotationPointZ = _frontRightLeg.rotationPointZ = -8.0F;
        _frontLeftLeg.rotateAngleX = frontSwing;
        _frontRightLeg.rotateAngleX = -frontSwing;
        _frontLeftShin.rotateAngleX = frontSwing + System.MathF.Max(0.0F, legSwing * 0.5F * limbDistance);
        _frontRightShin.rotateAngleX = -frontSwing + System.MathF.Max(0.0F, -legSwing * 0.5F * limbDistance);
        _frontLeftHoof.rotateAngleX = _frontLeftShin.rotateAngleX;
        _frontRightHoof.rotateAngleX = _frontRightShin.rotateAngleX;

        _frontLeftShin.rotationPointY = _frontLeftLeg.rotationPointY + MathHelper.Sin(HalfPi + _frontLeftLeg.rotateAngleX) * 7.0F;
        _frontLeftShin.rotationPointZ = _frontLeftLeg.rotationPointZ + MathHelper.Cos(ThreeHalvesPi + _frontLeftLeg.rotateAngleX) * 7.0F;
        _frontRightShin.rotationPointY = _frontRightLeg.rotationPointY + MathHelper.Sin(HalfPi + _frontRightLeg.rotateAngleX) * 7.0F;
        _frontRightShin.rotationPointZ = _frontRightLeg.rotationPointZ + MathHelper.Cos(ThreeHalvesPi + _frontRightLeg.rotateAngleX) * 7.0F;
        _frontLeftHoof.rotationPointY = _frontLeftShin.rotationPointY;
        _frontLeftHoof.rotationPointZ = _frontLeftShin.rotationPointZ;
        _frontRightHoof.rotationPointY = _frontRightShin.rotationPointY;
        _frontRightHoof.rotationPointZ = _frontRightShin.rotationPointZ;

        float ropeSwingX = _showReins ? -1.0471976F : frontSwing / 3.0F;
        float ropeSwingZ = _showReins ? 0.0F : frontSwing / 5.0F;
        _leftSaddleRope.rotateAngleX = _leftSaddleMetal.rotateAngleX = ropeSwingX;
        _rightSaddleRope.rotateAngleX = _rightSaddleMetal.rotateAngleX = ropeSwingX;
        _leftSaddleRope.rotateAngleZ = _leftSaddleMetal.rotateAngleZ = ropeSwingZ;
        _rightSaddleRope.rotateAngleZ = _rightSaddleMetal.rotateAngleZ = -ropeSwingZ;

        float tailPitch = -1.3089F + limbDistance * 1.5F;
        if (tailPitch > 0.0F)
        {
            tailPitch = 0.0F;
        }

        float tailYaw = MathHelper.Cos(age * 0.7F) * 0.05F;
        _tailBase.rotationPointY = 3.0F;
        _tailBase.rotationPointZ = 14.0F;
        _tailMiddle.rotationPointY = _tailTip.rotationPointY = _tailBase.rotationPointY;
        _tailMiddle.rotationPointZ = _tailTip.rotationPointZ = _tailBase.rotationPointZ;
        _tailBase.rotateAngleX = tailPitch;
        _tailMiddle.rotateAngleX = tailPitch;
        _tailTip.rotateAngleX = -0.2618F + tailPitch;
        _tailBase.rotateAngleY = _tailMiddle.rotateAngleY = _tailTip.rotateAngleY = tailYaw;
    }

    private static ModelPart CreatePart(int textureX, int textureY)
    {
        return new ModelPart(textureX, textureY).setTextureSize(128, 128);
    }

    private static void SetRotation(ModelPart part, float x, float y, float z)
    {
        part.rotateAngleX = x;
        part.rotateAngleY = y;
        part.rotateAngleZ = z;
    }

    private static void ApplyHeadPose(ModelPart part, float pointY, float pointZ, float angleX, float angleY)
    {
        part.rotationPointY = pointY;
        part.rotationPointZ = pointZ;
        part.rotateAngleX = angleX;
        part.rotateAngleY = angleY;
    }
}
