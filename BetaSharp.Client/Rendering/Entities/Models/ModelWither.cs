using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelWither : ModelBase
{
    private readonly ModelPart[] _bodyParts = new ModelPart[3];
    private readonly ModelPart[] _heads = new ModelPart[3];

    public ModelWither(float scale = 0.0F)
    {
        _bodyParts[0] = new ModelPart(0, 16).setTextureSize(64, 64);
        _bodyParts[0].addBox(-10.0F, 3.9F, -0.5F, 20, 3, 3, scale);

        _bodyParts[1] = new ModelPart(0, 22).setTextureSize(64, 64);
        _bodyParts[1].addBox(0.0F, 0.0F, 0.0F, 3, 10, 3, scale);
        _bodyParts[1].addBox(-4.0F, 1.5F, 0.5F, 11, 2, 2, scale);
        _bodyParts[1].addBox(-4.0F, 4.0F, 0.5F, 11, 2, 2, scale);
        _bodyParts[1].addBox(-4.0F, 6.5F, 0.5F, 11, 2, 2, scale);
        _bodyParts[1].setRotationPoint(-2.0F, 6.9F, -0.5F);

        _bodyParts[2] = new ModelPart(12, 22).setTextureSize(64, 64);
        _bodyParts[2].addBox(0.0F, 0.0F, 0.0F, 3, 6, 3, scale);

        _heads[0] = new ModelPart(0, 0).setTextureSize(64, 64);
        _heads[0].addBox(-4.0F, -4.0F, -4.0F, 8, 8, 8, scale);

        _heads[1] = new ModelPart(32, 0).setTextureSize(64, 64);
        _heads[1].addBox(-4.0F, -4.0F, -4.0F, 6, 6, 6, scale);
        _heads[1].rotationPointX = -8.0F;
        _heads[1].rotationPointY = 4.0F;

        _heads[2] = new ModelPart(32, 0).setTextureSize(64, 64);
        _heads[2].addBox(-4.0F, -4.0F, -4.0F, 6, 6, 6, scale);
        _heads[2].rotationPointX = 10.0F;
        _heads[2].rotationPointY = 4.0F;
    }

    public override void render(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, age, yaw, pitch, scale);

        for (int i = 0; i < _heads.Length; ++i)
        {
            _heads[i].render(scale);
        }

        for (int i = 0; i < _bodyParts.Length; ++i)
        {
            _bodyParts[i].render(scale);
        }
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        float pulse = MathHelper.Cos(age * 0.1F);
        _bodyParts[1].rotateAngleX = (0.065F + 0.05F * pulse) * (float)System.Math.PI;
        _bodyParts[2].setRotationPoint(
            -2.0F,
            6.9F + MathHelper.Cos(_bodyParts[1].rotateAngleX) * 10.0F,
            -0.5F + MathHelper.Sin(_bodyParts[1].rotateAngleX) * 10.0F);
        _bodyParts[2].rotateAngleX = (0.265F + 0.1F * pulse) * (float)System.Math.PI;

        _heads[0].rotateAngleY = yaw / (180.0F / (float)System.Math.PI);
        _heads[0].rotateAngleX = pitch / (180.0F / (float)System.Math.PI);
        _heads[1].rotateAngleY = MathHelper.Cos(age * 0.14F) * 0.35F;
        _heads[1].rotateAngleX = MathHelper.Sin(age * 0.11F) * 0.15F;
        _heads[2].rotateAngleY = MathHelper.Cos(age * 0.14F + (float)System.Math.PI) * 0.35F;
        _heads[2].rotateAngleX = MathHelper.Sin(age * 0.11F + (float)System.Math.PI) * 0.15F;
    }

    public override void setLivingAnimations(EntityLiving var1, float var2, float var3, float var4)
    {
        if (var1 is not EntityWither wither)
        {
            return;
        }

        float bodyYaw = var1.lastBodyYaw + (var1.bodyYaw - var1.lastBodyYaw) * var4;
        _heads[1].rotateAngleY = (wither.GetTrackedHeadYaw(0) - bodyYaw) / (180.0F / (float)System.Math.PI);
        _heads[1].rotateAngleX = wither.GetTrackedHeadPitch(0) / (180.0F / (float)System.Math.PI);
        _heads[2].rotateAngleY = (wither.GetTrackedHeadYaw(1) - bodyYaw) / (180.0F / (float)System.Math.PI);
        _heads[2].rotateAngleX = wither.GetTrackedHeadPitch(1) / (180.0F / (float)System.Math.PI);
    }
}
