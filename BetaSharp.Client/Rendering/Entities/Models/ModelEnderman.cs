namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelEnderman : ModelBiped
{
    public bool isCarrying;
    public bool isAttacking;

    public ModelEnderman() : base(0.0F, -14.0F)
    {
        const float yOffset = -14.0F;
        bipedHeadwear = new ModelPart(0, 16);
        bipedHeadwear.addBox(-4.0F, -8.0F, -4.0F, 8, 8, 8, -0.5F);
        bipedHeadwear.setRotationPoint(0.0F, yOffset, 0.0F);

        bipedBody = new ModelPart(32, 16);
        bipedBody.addBox(-4.0F, 0.0F, -2.0F, 8, 12, 4, 0.0F);
        bipedBody.setRotationPoint(0.0F, yOffset, 0.0F);

        bipedRightArm = new ModelPart(56, 0);
        bipedRightArm.addBox(-1.0F, -2.0F, -1.0F, 2, 30, 2, 0.0F);
        bipedRightArm.setRotationPoint(-3.0F, 2.0F + yOffset, 0.0F);

        bipedLeftArm = new ModelPart(56, 0) { mirror = true };
        bipedLeftArm.addBox(-1.0F, -2.0F, -1.0F, 2, 30, 2, 0.0F);
        bipedLeftArm.setRotationPoint(5.0F, 2.0F + yOffset, 0.0F);

        bipedRightLeg = new ModelPart(56, 0);
        bipedRightLeg.addBox(-1.0F, 0.0F, -1.0F, 2, 30, 2, 0.0F);
        bipedRightLeg.setRotationPoint(-2.0F, 12.0F + yOffset, 0.0F);

        bipedLeftLeg = new ModelPart(56, 0) { mirror = true };
        bipedLeftLeg.addBox(-1.0F, 0.0F, -1.0F, 2, 30, 2, 0.0F);
        bipedLeftLeg.setRotationPoint(2.0F, 12.0F + yOffset, 0.0F);
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        base.setRotationAngles(limbAngle, limbDistance, tickDelta, yaw, pitch, scale);

        const float yOffset = -14.0F;
        bipedBody.rotateAngleX = 0.0F;
        bipedBody.rotationPointY = yOffset;
        bipedRightArm.rotateAngleX *= 0.5F;
        bipedLeftArm.rotateAngleX *= 0.5F;
        bipedRightLeg.rotateAngleX *= 0.5F;
        bipedLeftLeg.rotateAngleX *= 0.5F;

        ClampLimbRotation(bipedRightArm);
        ClampLimbRotation(bipedLeftArm);
        ClampLimbRotation(bipedRightLeg);
        ClampLimbRotation(bipedLeftLeg);

        if (isCarrying)
        {
            bipedRightArm.rotateAngleX = -0.5F;
            bipedLeftArm.rotateAngleX = -0.5F;
            bipedRightArm.rotateAngleZ = 0.05F;
            bipedLeftArm.rotateAngleZ = -0.05F;
        }

        bipedRightArm.rotationPointZ = 0.0F;
        bipedLeftArm.rotationPointZ = 0.0F;
        bipedRightLeg.rotationPointZ = 0.0F;
        bipedLeftLeg.rotationPointZ = 0.0F;
        bipedRightLeg.rotationPointY = 9.0F + yOffset;
        bipedLeftLeg.rotationPointY = 9.0F + yOffset;
        bipedHead.rotationPointY = yOffset + 1.0F;
        bipedHeadwear.rotationPointX = bipedHead.rotationPointX;
        bipedHeadwear.rotationPointY = bipedHead.rotationPointY;
        bipedHeadwear.rotationPointZ = bipedHead.rotationPointZ;
        bipedHeadwear.rotateAngleX = bipedHead.rotateAngleX;
        bipedHeadwear.rotateAngleY = bipedHead.rotateAngleY;
        bipedHeadwear.rotateAngleZ = bipedHead.rotateAngleZ;

        if (isAttacking)
        {
            bipedHead.rotationPointY -= 5.0F;
        }
    }

    private static void ClampLimbRotation(ModelPart limb)
    {
        const float max = 0.4F;
        if (limb.rotateAngleX > max) limb.rotateAngleX = max;
        if (limb.rotateAngleX < -max) limb.rotateAngleX = -max;
    }
}
