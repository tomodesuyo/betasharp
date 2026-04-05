using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities.Models;

public class ModelArmorStandArmor : ModelBiped
{
    public ModelArmorStandArmor() : this(0.0F)
    {
    }

    public ModelArmorStandArmor(float modelSize) : base(modelSize)
    {
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
    }

    public override void setLivingAnimations(EntityLiving entity, float limbAngle, float limbDistance, float tickDelta)
    {
        if (entity is not EntityArmorStand armorStand)
        {
            return;
        }

        ApplyPose(bipedHead, armorStand.GetHeadRotation());
        bipedHead.setRotationPoint(0.0F, 1.0F, 0.0F);
        ApplyPose(bipedBody, armorStand.GetBodyRotation());
        ApplyPose(bipedLeftArm, armorStand.GetLeftArmRotation());
        ApplyPose(bipedRightArm, armorStand.GetRightArmRotation());
        ApplyPose(bipedLeftLeg, armorStand.GetLeftLegRotation());
        bipedLeftLeg.setRotationPoint(1.9F, 11.0F, 0.0F);
        ApplyPose(bipedRightLeg, armorStand.GetRightLegRotation());
        bipedRightLeg.setRotationPoint(-1.9F, 11.0F, 0.0F);
        bipedHeadwear.rotateAngleX = bipedHead.rotateAngleX;
        bipedHeadwear.rotateAngleY = bipedHead.rotateAngleY;
        bipedHeadwear.rotateAngleZ = bipedHead.rotateAngleZ;
    }

    private static void ApplyPose(ModelPart part, EntityArmorStand.ArmorStandPose pose)
    {
        const float radiansPerDegree = (float)System.Math.PI / 180.0F;
        part.rotateAngleX = pose.X * radiansPerDegree;
        part.rotateAngleY = pose.Y * radiansPerDegree;
        part.rotateAngleZ = pose.Z * radiansPerDegree;
    }
}
