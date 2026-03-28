namespace BetaSharp.Client.Rendering.Entities.Models;

public class ModelPig : ModelQuadruped
{
    private readonly ModelPart snout = new(16, 16);

    public ModelPig() : base(6, 0.0f)
    {
        InitSnout(0.0f);
    }

    public ModelPig(float var1) : base(6, var1)
    {
        InitSnout(var1);
    }

    private void InitSnout(float scale)
    {
        snout.addBox(-2.0F, 0.0F, -9.0F, 4, 3, 1, scale);
        snout.setRotationPoint(0.0F, 12.0F, -6.0F);
    }

    public override void render(float var1, float var2, float var3, float var4, float var5, float var6)
    {
        base.render(var1, var2, var3, var4, var5, var6);
        snout.render(var6);
    }

    public override void setRotationAngles(float var1, float var2, float var3, float var4, float var5, float var6)
    {
        base.setRotationAngles(var1, var2, var3, var4, var5, var6);
        snout.rotateAngleX = head.rotateAngleX;
        snout.rotateAngleY = head.rotateAngleY;
        snout.rotateAngleZ = head.rotateAngleZ;
    }
}
