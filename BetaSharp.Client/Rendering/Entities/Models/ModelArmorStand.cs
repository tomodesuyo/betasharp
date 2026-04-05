using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelArmorStand : ModelArmorStandArmor
{
    private readonly ModelPart _standRightSide;
    private readonly ModelPart _standLeftSide;
    private readonly ModelPart _standWaist;
    private readonly ModelPart _standBase;

    public ModelArmorStand()
    {
        bipedHead = new ModelPart(0, 0).setTextureSize(64, 64);
        bipedHead.addBox(-1.0F, -7.0F, -1.0F, 2, 7, 2, 0.0F);
        bipedHead.setRotationPoint(0.0F, 0.0F, 0.0F);

        bipedHeadwear = new ModelPart(0, 0).setTextureSize(64, 64);
        bipedHeadwear.hidden = true;

        bipedBody = new ModelPart(0, 26).setTextureSize(64, 64);
        bipedBody.addBox(-6.0F, 0.0F, -1.5F, 12, 3, 3, 0.0F);
        bipedBody.setRotationPoint(0.0F, 0.0F, 0.0F);

        bipedRightArm = new ModelPart(24, 0).setTextureSize(64, 64);
        bipedRightArm.addBox(-2.0F, -2.0F, -1.0F, 2, 12, 2, 0.0F);
        bipedRightArm.setRotationPoint(-5.0F, 2.0F, 0.0F);

        bipedLeftArm = new ModelPart(32, 16).setTextureSize(64, 64);
        bipedLeftArm.mirror = true;
        bipedLeftArm.addBox(0.0F, -2.0F, -1.0F, 2, 12, 2, 0.0F);
        bipedLeftArm.setRotationPoint(5.0F, 2.0F, 0.0F);

        bipedRightLeg = new ModelPart(8, 0).setTextureSize(64, 64);
        bipedRightLeg.addBox(-1.0F, 0.0F, -1.0F, 2, 11, 2, 0.0F);
        bipedRightLeg.setRotationPoint(-1.9F, 12.0F, 0.0F);

        bipedLeftLeg = new ModelPart(40, 16).setTextureSize(64, 64);
        bipedLeftLeg.mirror = true;
        bipedLeftLeg.addBox(-1.0F, 0.0F, -1.0F, 2, 11, 2, 0.0F);
        bipedLeftLeg.setRotationPoint(1.9F, 12.0F, 0.0F);

        _standRightSide = new ModelPart(16, 0).setTextureSize(64, 64);
        _standRightSide.addBox(-3.0F, 3.0F, -1.0F, 2, 7, 2, 0.0F);
        _standRightSide.setRotationPoint(0.0F, 0.0F, 0.0F);

        _standLeftSide = new ModelPart(48, 16).setTextureSize(64, 64);
        _standLeftSide.addBox(1.0F, 3.0F, -1.0F, 2, 7, 2, 0.0F);
        _standLeftSide.setRotationPoint(0.0F, 0.0F, 0.0F);

        _standWaist = new ModelPart(0, 48).setTextureSize(64, 64);
        _standWaist.addBox(-4.0F, 10.0F, -1.0F, 8, 2, 2, 0.0F);
        _standWaist.setRotationPoint(0.0F, 0.0F, 0.0F);

        _standBase = new ModelPart(0, 32).setTextureSize(64, 64);
        _standBase.addBox(-6.0F, 11.0F, -6.0F, 12, 1, 12, 0.0F);
        _standBase.setRotationPoint(0.0F, 12.0F, 0.0F);
    }

    public override void render(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, tickDelta, yaw, pitch, scale);
        bipedHead.render(scale);
        bipedBody.render(scale);
        _standRightSide.render(scale);
        _standLeftSide.render(scale);
        _standWaist.render(scale);
        bipedRightArm.render(scale);
        bipedLeftArm.render(scale);
        bipedRightLeg.render(scale);
        bipedLeftLeg.render(scale);
        _standBase.render(scale);
    }

    public override void setLivingAnimations(EntityLiving entity, float limbAngle, float limbDistance, float tickDelta)
    {
        if (entity is not EntityArmorStand armorStand)
        {
            return;
        }

        base.setLivingAnimations(entity, limbAngle, limbDistance, tickDelta);
        bipedRightArm.visible = armorStand.GetShowArms();
        bipedLeftArm.visible = armorStand.GetShowArms();
        _standBase.visible = !armorStand.HasNoBasePlate();
        bipedRightLeg.setRotationPoint(-1.9F, 12.0F, 0.0F);
        bipedLeftLeg.setRotationPoint(1.9F, 12.0F, 0.0F);

        _standRightSide.rotateAngleX = bipedBody.rotateAngleX;
        _standRightSide.rotateAngleY = bipedBody.rotateAngleY;
        _standRightSide.rotateAngleZ = bipedBody.rotateAngleZ;
        _standLeftSide.rotateAngleX = bipedBody.rotateAngleX;
        _standLeftSide.rotateAngleY = bipedBody.rotateAngleY;
        _standLeftSide.rotateAngleZ = bipedBody.rotateAngleZ;
        _standWaist.rotateAngleX = bipedBody.rotateAngleX;
        _standWaist.rotateAngleY = bipedBody.rotateAngleY;
        _standWaist.rotateAngleZ = bipedBody.rotateAngleZ;
        _standBase.rotateAngleX = 0.0F;
        _standBase.rotateAngleY = -entity.yaw / (180.0F / (float)System.Math.PI);
        _standBase.rotateAngleZ = 0.0F;
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        bipedHead.rotateAngleY = yaw / (180.0F / (float)System.Math.PI);
        bipedHead.rotateAngleX = pitch / (180.0F / (float)System.Math.PI);
        bipedHeadwear.rotateAngleY = bipedHead.rotateAngleY;
        bipedHeadwear.rotateAngleX = bipedHead.rotateAngleX;
        bipedHeadwear.visible = false;
        _standRightSide.rotateAngleX = bipedBody.rotateAngleX;
        _standRightSide.rotateAngleY = bipedBody.rotateAngleY;
        _standRightSide.rotateAngleZ = bipedBody.rotateAngleZ;
        _standLeftSide.rotateAngleX = bipedBody.rotateAngleX;
        _standLeftSide.rotateAngleY = bipedBody.rotateAngleY;
        _standLeftSide.rotateAngleZ = bipedBody.rotateAngleZ;
        _standWaist.rotateAngleX = bipedBody.rotateAngleX;
        _standWaist.rotateAngleY = bipedBody.rotateAngleY;
        _standWaist.rotateAngleZ = bipedBody.rotateAngleZ;
    }
}
