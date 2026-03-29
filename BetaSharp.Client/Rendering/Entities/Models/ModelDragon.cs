using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelDragon : ModelBase
{
    private readonly ModelPart _head;
    private readonly ModelPart _neck;
    private readonly ModelPart _jaw;
    private readonly ModelPart _body;
    private readonly ModelPart _rearLeg;
    private readonly ModelPart _frontLeg;
    private readonly ModelPart _rearLegTip;
    private readonly ModelPart _frontLegTip;
    private readonly ModelPart _rearFoot;
    private readonly ModelPart _frontFoot;
    private readonly ModelPart _wing;
    private readonly ModelPart _wingTip;
    private float _partialTick;
    private EntityDragon? _dragon;

    public ModelDragon()
    {
        const float textureDepthOffset = -16.0F;

        _head = new ModelPart(0, 0).setTextureSize(256, 256);
        _head.setTextureOffset(176, 44).addBox(-6.0F, -1.0F, -8.0F + textureDepthOffset, 12, 5, 16);
        _head.setTextureOffset(112, 30).addBox(-8.0F, -8.0F, 6.0F + textureDepthOffset, 16, 16, 16);
        _head.mirror = true;
        _head.setTextureOffset(0, 0).addBox(-5.0F, -12.0F, 12.0F + textureDepthOffset, 2, 4, 6);
        _head.setTextureOffset(112, 0).addBox(-5.0F, -3.0F, -6.0F + textureDepthOffset, 2, 2, 4);
        _head.mirror = false;
        _head.setTextureOffset(0, 0).addBox(3.0F, -12.0F, 12.0F + textureDepthOffset, 2, 4, 6);
        _head.setTextureOffset(112, 0).addBox(3.0F, -3.0F, -6.0F + textureDepthOffset, 2, 2, 4);

        _jaw = new ModelPart(176, 65).setTextureSize(256, 256);
        _jaw.setRotationPoint(0.0F, 4.0F, 8.0F + textureDepthOffset);
        _jaw.addBox(-6.0F, 0.0F, -16.0F, 12, 4, 16);
        _head.AddChild(_jaw);

        _neck = new ModelPart(192, 104).setTextureSize(256, 256);
        _neck.addBox(-5.0F, -5.0F, -5.0F, 10, 10, 10);
        _neck.setTextureOffset(48, 0).addBox(-1.0F, -9.0F, -3.0F, 2, 4, 6);

        _body = new ModelPart(0, 0).setTextureSize(256, 256);
        _body.setRotationPoint(0.0F, 4.0F, 8.0F);
        _body.addBox(-12.0F, 0.0F, -16.0F, 24, 24, 64);
        _body.setTextureOffset(220, 53).addBox(-1.0F, -6.0F, -10.0F, 2, 6, 12);
        _body.setTextureOffset(220, 53).addBox(-1.0F, -6.0F, 10.0F, 2, 6, 12);
        _body.setTextureOffset(220, 53).addBox(-1.0F, -6.0F, 30.0F, 2, 6, 12);

        _wing = new ModelPart(112, 88).setTextureSize(256, 256);
        _wing.setRotationPoint(-12.0F, 5.0F, 2.0F);
        _wing.addBox(-56.0F, -4.0F, -4.0F, 56, 8, 8);
        _wing.setTextureOffset(-56, 88).addBox(-56.0F, 0.0F, 2.0F, 56, 0, 56);

        _wingTip = new ModelPart(112, 136).setTextureSize(256, 256);
        _wingTip.setRotationPoint(-56.0F, 0.0F, 0.0F);
        _wingTip.addBox(-56.0F, -2.0F, -2.0F, 56, 4, 4);
        _wingTip.setTextureOffset(-56, 144).addBox(-56.0F, 0.0F, 2.0F, 56, 0, 56);
        _wing.AddChild(_wingTip);

        _frontLeg = new ModelPart(112, 104).setTextureSize(256, 256);
        _frontLeg.setRotationPoint(-12.0F, 20.0F, 2.0F);
        _frontLeg.addBox(-4.0F, -4.0F, -4.0F, 8, 24, 8);

        _frontLegTip = new ModelPart(226, 138).setTextureSize(256, 256);
        _frontLegTip.setRotationPoint(0.0F, 20.0F, -1.0F);
        _frontLegTip.addBox(-3.0F, -1.0F, -3.0F, 6, 24, 6);
        _frontLeg.AddChild(_frontLegTip);

        _frontFoot = new ModelPart(144, 104).setTextureSize(256, 256);
        _frontFoot.setRotationPoint(0.0F, 23.0F, 0.0F);
        _frontFoot.addBox(-4.0F, 0.0F, -12.0F, 8, 4, 16);
        _frontLegTip.AddChild(_frontFoot);

        _rearLeg = new ModelPart(0, 0).setTextureSize(256, 256);
        _rearLeg.setRotationPoint(-16.0F, 16.0F, 42.0F);
        _rearLeg.addBox(-8.0F, -4.0F, -8.0F, 16, 32, 16);

        _rearLegTip = new ModelPart(196, 0).setTextureSize(256, 256);
        _rearLegTip.setRotationPoint(0.0F, 32.0F, -4.0F);
        _rearLegTip.addBox(-6.0F, -2.0F, 0.0F, 12, 32, 12);
        _rearLeg.AddChild(_rearLegTip);

        _rearFoot = new ModelPart(112, 0).setTextureSize(256, 256);
        _rearFoot.setRotationPoint(0.0F, 31.0F, 4.0F);
        _rearFoot.addBox(-9.0F, 0.0F, -20.0F, 18, 6, 24);
        _rearLegTip.AddChild(_rearFoot);
    }

    public override void setLivingAnimations(EntityLiving living, float limbAngle, float limbDistance, float tickDelta)
    {
        _dragon = (EntityDragon)living;
        _partialTick = tickDelta;
    }

    public override void render(float limbAngle, float limbDistance, float tickDelta, float yaw, float pitch, float scale)
    {
        if (_dragon == null)
        {
            return;
        }

        GLManager.GL.PushMatrix();

        float flap = _dragon.LastFlapTime + (_dragon.FlapTime - _dragon.LastFlapTime) * _partialTick;
        _jaw.rotateAngleX = (float)(Math.Sin(flap * Math.PI * 2.0F) + 1.0D) * 0.2F;
        float bob = (float)(Math.Sin(flap * Math.PI * 2.0F - 1.0F) + 1.0D);
        bob = (bob * bob + bob * 2.0F) * 0.05F;
        GLManager.GL.Translate(0.0F, bob - 2.0F, -3.0F);
        GLManager.GL.Rotate(bob * 2.0F, 1.0F, 0.0F, 0.0F);

        float neckY = 20.0F;
        float neckZ = -12.0F;
        float neckX = 0.0F;
        float angleScale = 1.5F;
        double[] anchorOffsets = _dragon.GetMovementOffsets(6, _partialTick);
        float yawDelta = WrapDegrees(_dragon.GetMovementOffsets(5, _partialTick)[0] - _dragon.GetMovementOffsets(10, _partialTick)[0]);
        float headYaw = WrapDegrees(_dragon.GetMovementOffsets(5, _partialTick)[0] + yawDelta / 2.0F);
        float flapRadians = flap * (float)Math.PI * 2.0F;

        for (int i = 0; i < 5; ++i)
        {
            double[] neckOffsets = _dragon.GetMovementOffsets(5 - i, _partialTick);
            float swing = (float)Math.Cos(i * 0.45F + flapRadians) * 0.15F;
            _neck.rotateAngleY = WrapDegrees(neckOffsets[0] - anchorOffsets[0]) * (float)Math.PI / 180.0F * angleScale;
            _neck.rotateAngleX = swing + (float)(neckOffsets[1] - anchorOffsets[1]) * (float)Math.PI / 180.0F * angleScale * 5.0F;
            _neck.rotateAngleZ = -WrapDegrees(neckOffsets[0] - headYaw) * (float)Math.PI / 180.0F * angleScale;
            _neck.rotationPointY = neckY;
            _neck.rotationPointZ = neckZ;
            _neck.rotationPointX = neckX;
            neckY += (float)Math.Sin(_neck.rotateAngleX) * 10.0F;
            neckZ -= (float)(Math.Cos(_neck.rotateAngleY) * Math.Cos(_neck.rotateAngleX) * 10.0D);
            neckX -= (float)(Math.Sin(_neck.rotateAngleY) * Math.Cos(_neck.rotateAngleX) * 10.0D);
            _neck.render(scale);
        }

        _head.rotationPointY = neckY;
        _head.rotationPointZ = neckZ;
        _head.rotationPointX = neckX;
        double[] headOffsets = _dragon.GetMovementOffsets(0, _partialTick);
        _head.rotateAngleY = WrapDegrees(headOffsets[0] - anchorOffsets[0]) * (float)Math.PI / 180.0F;
        _head.rotateAngleZ = -WrapDegrees(headOffsets[0] - headYaw) * (float)Math.PI / 180.0F;
        _head.render(scale);

        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(0.0F, 1.0F, 0.0F);
        GLManager.GL.Rotate(-yawDelta * angleScale, 0.0F, 0.0F, 1.0F);
        GLManager.GL.Translate(0.0F, -1.0F, 0.0F);
        _body.rotateAngleZ = 0.0F;
        _body.render(scale);

        for (int side = 0; side < 2; ++side)
        {
            GLManager.GL.Enable(GLEnum.CullFace);
            GLManager.GL.CullFace(side == 0 ? GLEnum.Back : GLEnum.Front);
            float wingTick = flap * (float)Math.PI * 2.0F;
            _wing.rotateAngleX = 2.0F / 16.0F - (float)Math.Cos(wingTick) * 0.2F;
            _wing.rotateAngleY = 0.25F;
            _wing.rotateAngleZ = (float)(Math.Sin(wingTick) + 0.125D) * 0.8F;
            _wingTip.rotateAngleZ = -((float)(Math.Sin(wingTick + 2.0F) + 0.5D)) * (12.0F / 16.0F);
            _rearLeg.rotateAngleX = 1.0F + bob * 0.1F;
            _rearLegTip.rotateAngleX = 0.5F + bob * 0.1F;
            _rearFoot.rotateAngleX = 12.0F / 16.0F + bob * 0.1F;
            _frontLeg.rotateAngleX = 1.3F + bob * 0.1F;
            _frontLegTip.rotateAngleX = -0.5F - bob * 0.1F;
            _frontFoot.rotateAngleX = 12.0F / 16.0F + bob * 0.1F;
            _wing.render(scale);
            _frontLeg.render(scale);
            _rearLeg.render(scale);
            GLManager.GL.Scale(-1.0F, 1.0F, 1.0F);
        }

        GLManager.GL.PopMatrix();
        GLManager.GL.CullFace(GLEnum.Back);
        GLManager.GL.Disable(GLEnum.CullFace);

        float tailSwing = 0.0F;
        float tailTick = flap * (float)Math.PI * 2.0F;
        neckY = 10.0F;
        neckZ = 60.0F;
        neckX = 0.0F;
        anchorOffsets = _dragon.GetMovementOffsets(11, _partialTick);

        for (int i = 0; i < 12; ++i)
        {
            double[] tailOffsets = _dragon.GetMovementOffsets(12 + i, _partialTick);
            tailSwing += (float)Math.Sin(i * 0.45F + tailTick) * 0.05F;
            _neck.rotateAngleY = (WrapDegrees(tailOffsets[0] - anchorOffsets[0]) * angleScale + 180.0F) * (float)Math.PI / 180.0F;
            _neck.rotateAngleX = tailSwing + (float)(tailOffsets[1] - anchorOffsets[1]) * (float)Math.PI / 180.0F * angleScale * 5.0F;
            _neck.rotateAngleZ = WrapDegrees(tailOffsets[0] - headYaw) * (float)Math.PI / 180.0F * angleScale;
            _neck.rotationPointY = neckY;
            _neck.rotationPointZ = neckZ;
            _neck.rotationPointX = neckX;
            neckY += (float)Math.Sin(_neck.rotateAngleX) * 10.0F;
            neckZ -= (float)(Math.Cos(_neck.rotateAngleY) * Math.Cos(_neck.rotateAngleX) * 10.0D);
            neckX -= (float)(Math.Sin(_neck.rotateAngleY) * Math.Cos(_neck.rotateAngleX) * 10.0D);
            _neck.render(scale);
        }

        GLManager.GL.PopMatrix();
    }

    private static float WrapDegrees(double degrees)
    {
        while (degrees >= 180.0D)
        {
            degrees -= 360.0D;
        }

        while (degrees < -180.0D)
        {
            degrees += 360.0D;
        }

        return (float)degrees;
    }
}
