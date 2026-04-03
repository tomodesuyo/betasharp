namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelHumanoidHead : ModelBase
{
    private readonly ModelSkeletonHead _innerHead = new(0, 0, 64, 64);
    private readonly ModelPart _headwear;

    public ModelHumanoidHead()
    {
        _headwear = new ModelPart(32, 0).setTextureSize(64, 64);
        _headwear.addBox(-4.0F, -8.0F, -4.0F, 8, 8, 8, 0.25F);
        _headwear.setRotationPoint(0.0F, 0.0F, 0.0F);
    }

    public override void render(float limbAngle, float limbDistance, float age, float netHeadYaw, float headPitch, float scale)
    {
        _innerHead.render(limbAngle, limbDistance, age, netHeadYaw, headPitch, scale);
        _headwear.rotateAngleY = netHeadYaw / (180.0F / (float)System.Math.PI);
        _headwear.rotateAngleX = headPitch / (180.0F / (float)System.Math.PI);
        _headwear.render(scale);
    }
}
