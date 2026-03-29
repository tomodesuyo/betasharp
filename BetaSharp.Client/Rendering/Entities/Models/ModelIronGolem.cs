using System;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelIronGolem : ModelBase
{
    public readonly ModelPart Head;
    public readonly ModelPart Body;
    public readonly ModelPart RightArm;
    public readonly ModelPart LeftArm;
    public readonly ModelPart RightLeg;
    public readonly ModelPart LeftLeg;

    public ModelIronGolem() : this(0.0F, -7.0F)
    {
    }

    public ModelIronGolem(float expansion, float yOffset)
    {
        const int textureWidth = 128;
        const int textureHeight = 128;

        Head = new ModelPart(0, 0).setTextureSize(textureWidth, textureHeight);
        Head.setRotationPoint(0.0F, yOffset, -2.0F);
        Head.setTextureOffset(0, 0).addBox(-4.0F, -12.0F, -5.5F, 8, 10, 8, expansion);
        Head.setTextureOffset(24, 0).addBox(-1.0F, -5.0F, -7.5F, 2, 4, 2, expansion);

        Body = new ModelPart(0, 40).setTextureSize(textureWidth, textureHeight);
        Body.setRotationPoint(0.0F, yOffset, 0.0F);
        Body.setTextureOffset(0, 40).addBox(-9.0F, -2.0F, -6.0F, 18, 12, 11, expansion);
        Body.setTextureOffset(0, 70).addBox(-4.5F, 10.0F, -3.0F, 9, 5, 6, expansion + 0.5F);

        RightArm = new ModelPart(60, 21).setTextureSize(textureWidth, textureHeight);
        RightArm.setRotationPoint(0.0F, -7.0F, 0.0F);
        RightArm.setTextureOffset(60, 21).addBox(-13.0F, -2.5F, -3.0F, 4, 30, 6, expansion);

        LeftArm = new ModelPart(60, 58).setTextureSize(textureWidth, textureHeight);
        LeftArm.setRotationPoint(0.0F, -7.0F, 0.0F);
        LeftArm.setTextureOffset(60, 58).addBox(9.0F, -2.5F, -3.0F, 4, 30, 6, expansion);

        RightLeg = new ModelPart(0, 22).setTextureSize(textureWidth, textureHeight);
        RightLeg.setRotationPoint(-4.0F, 18.0F + yOffset, 0.0F);
        RightLeg.setTextureOffset(37, 0).addBox(-3.5F, -3.0F, -3.0F, 6, 16, 5, expansion);

        LeftLeg = new ModelPart(0, 22).setTextureSize(textureWidth, textureHeight);
        LeftLeg.mirror = true;
        LeftLeg.setRotationPoint(5.0F, 18.0F + yOffset, 0.0F);
        LeftLeg.setTextureOffset(60, 0).addBox(-3.5F, -3.0F, -3.0F, 6, 16, 5, expansion);
    }

    public override void render(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, tickDelta, yaw, pitch, scale);
        Head.render(scale);
        Body.render(scale);
        RightLeg.render(scale);
        LeftLeg.render(scale);
        RightArm.render(scale);
        LeftArm.render(scale);
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        Head.rotateAngleY = yaw / (180.0F / (float)Math.PI);
        Head.rotateAngleX = pitch / (180.0F / (float)Math.PI);
        RightLeg.rotateAngleX = -1.5F * TriangleWave(limbAngle, 13.0F) * limbDistance;
        LeftLeg.rotateAngleX = 1.5F * TriangleWave(limbAngle, 13.0F) * limbDistance;
        RightLeg.rotateAngleY = 0.0F;
        LeftLeg.rotateAngleY = 0.0F;
    }

    public override void setLivingAnimations(EntityLiving entity, float limbAngle, float limbDistance, float tickDelta)
    {
        EntityIronGolem golem = (EntityIronGolem)entity;
        if (golem.AttackTicks > 0)
        {
            RightArm.rotateAngleX = -2.0F + 1.5F * TriangleWave(golem.AttackTicks - tickDelta, 10.0F);
            LeftArm.rotateAngleX = -2.0F + 1.5F * TriangleWave(golem.AttackTicks - tickDelta, 10.0F);
        }
        else if (golem.OfferRoseTicks > 0)
        {
            RightArm.rotateAngleX = -0.8F + 0.025F * TriangleWave(golem.OfferRoseTicks, 70.0F);
            LeftArm.rotateAngleX = 0.0F;
        }
        else
        {
            RightArm.rotateAngleX = (-0.2F + 1.5F * TriangleWave(limbAngle, 13.0F)) * limbDistance;
            LeftArm.rotateAngleX = (-0.2F - 1.5F * TriangleWave(limbAngle, 13.0F)) * limbDistance;
        }
    }

    private static float TriangleWave(float value, float period)
    {
        return (Math.Abs(value % period - period * 0.5F) - period * 0.25F) / (period * 0.25F);
    }
}
