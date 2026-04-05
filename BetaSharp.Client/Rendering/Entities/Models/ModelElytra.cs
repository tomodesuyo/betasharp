using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelElytra : ModelBase
{
    private readonly ModelPart _leftWing;
    private readonly ModelPart _rightWing;

    public ModelElytra()
    {
        _leftWing = new ModelPart(22, 0).setTextureSize(64, 32);
        _leftWing.addBox(-10.0F, 0.0F, 0.0F, 10, 20, 2, 1.0F);
        _leftWing.setRotationPoint(5.0F, 0.0F, 0.0F);

        _rightWing = new ModelPart(22, 0).setTextureSize(64, 32);
        _rightWing.mirror = true;
        _rightWing.addBox(0.0F, 0.0F, 0.0F, 10, 20, 2, 1.0F);
        _rightWing.setRotationPoint(-5.0F, 0.0F, 0.0F);
    }

    public override void render(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float scale)
    {
        _leftWing.render(scale);
        _rightWing.render(scale);
    }

    public override void setLivingAnimations(EntityLiving entity, float limbSwing, float limbSwingAmount, float tickDelta)
    {
        float wingPitch = 0.2617994F;
        float wingRoll = -0.2617994F;
        float wingY = 0.0F;
        float wingYaw = 0.0F;

        if (entity is EntityPlayer player && player.IsGlidingWithElytra())
        {
            float glideFactor = 1.0F;
            if (entity.velocityY < 0.0D)
            {
                Vec3D velocity = new(entity.velocityX, entity.velocityY, entity.velocityZ);
                velocity = velocity.normalize();
                glideFactor = 1.0F - (float)System.Math.Pow(-velocity.y, 1.5D);
            }

            wingPitch = glideFactor * 0.34906584F + (1.0F - glideFactor) * wingPitch;
            wingRoll = glideFactor * -((float)System.Math.PI / 2.0F) + (1.0F - glideFactor) * wingRoll;
        }
        else if (entity.isSneaking())
        {
            wingPitch = ((float)System.Math.PI * 2.0F / 9.0F);
            wingRoll = -((float)System.Math.PI / 4.0F);
            wingY = 3.0F;
            wingYaw = 0.08726646F;
        }

        _leftWing.rotationPointX = 5.0F;
        _leftWing.rotationPointY = wingY;

        if (entity is EntityPlayer wingPlayer)
        {
            wingPlayer.rotateElytraX += (wingPitch - wingPlayer.rotateElytraX) * 0.1F;
            wingPlayer.rotateElytraY += (wingYaw - wingPlayer.rotateElytraY) * 0.1F;
            wingPlayer.rotateElytraZ += (wingRoll - wingPlayer.rotateElytraZ) * 0.1F;
            _leftWing.rotateAngleX = wingPlayer.rotateElytraX;
            _leftWing.rotateAngleY = wingPlayer.rotateElytraY;
            _leftWing.rotateAngleZ = wingPlayer.rotateElytraZ;
        }
        else
        {
            _leftWing.rotateAngleX = wingPitch;
            _leftWing.rotateAngleY = wingYaw;
            _leftWing.rotateAngleZ = wingRoll;
        }

        _rightWing.rotationPointX = -_leftWing.rotationPointX;
        _rightWing.rotationPointY = _leftWing.rotationPointY;
        _rightWing.rotateAngleX = _leftWing.rotateAngleX;
        _rightWing.rotateAngleY = -_leftWing.rotateAngleY;
        _rightWing.rotateAngleZ = -_leftWing.rotateAngleZ;
    }
}
