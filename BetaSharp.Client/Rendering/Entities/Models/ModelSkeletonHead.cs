namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelSkeletonHead : ModelBase
{
    private readonly ModelPart _head;

    public ModelSkeletonHead(int textureOffsetX, int textureOffsetY, int textureWidth, int textureHeight)
    {
        _head = new ModelPart(textureOffsetX, textureOffsetY).setTextureSize(textureWidth, textureHeight);
        _head.addBox(-4.0F, -8.0F, -4.0F, 8, 8, 8, 0.0F);
        _head.setRotationPoint(0.0F, 0.0F, 0.0F);
    }

    public override void render(float limbAngle, float limbDistance, float age, float netHeadYaw, float headPitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, age, netHeadYaw, headPitch, scale);
        _head.render(scale);
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float netHeadYaw, float headPitch, float scale)
    {
        _head.rotateAngleY = netHeadYaw / (180.0F / (float)System.Math.PI);
        _head.rotateAngleX = headPitch / (180.0F / (float)System.Math.PI);
    }
}
