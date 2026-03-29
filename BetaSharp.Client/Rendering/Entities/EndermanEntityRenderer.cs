using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class EndermanEntityRenderer : LivingEntityRenderer
{
    private readonly ModelEnderman _endermanModel;
    private readonly Random _random = new(1660);

    public EndermanEntityRenderer() : base(new ModelEnderman(), 0.5F)
    {
        _endermanModel = (ModelEnderman)mainModel;
        setRenderPassModel(_endermanModel);
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        EntityEnderman enderman = (EntityEnderman)target;
        _endermanModel.isCarrying = enderman.GetCarried() > 0;
        _endermanModel.isAttacking = enderman.IsAggressive;
        if (enderman.IsAggressive)
        {
            const double jitter = 0.02D;
            x += NextGaussian() * jitter;
            z += NextGaussian() * jitter;
        }

        doRenderLiving(enderman, x, y, z, yaw, tickDelta);
    }

    protected override bool shouldRenderPass(EntityLiving var1, int var2, float var3)
    {
        if (var2 != 0)
        {
            return false;
        }

        loadTexture("/mob/enderman_eyes.png");
        GLManager.GL.Enable(GLEnum.Blend);
        GLManager.GL.Disable(GLEnum.AlphaTest);
        GLManager.GL.BlendFunc(GLEnum.One, GLEnum.One);
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        return true;
    }

    protected override void renderMore(EntityLiving entity, float tickDelta)
    {
        base.renderMore(entity, tickDelta);
        EntityEnderman enderman = (EntityEnderman)entity;
        int carriedId = enderman.GetCarried();
        if (carriedId <= 0)
        {
            return;
        }

        Block carriedBlock = Block.Blocks[carriedId];
        GLManager.GL.Enable(GLEnum.RescaleNormal);
        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(0.0F, 11.0F / 16.0F, -(12.0F / 16.0F));
        GLManager.GL.Rotate(20.0F, 1.0F, 0.0F, 0.0F);
        GLManager.GL.Rotate(45.0F, 0.0F, 1.0F, 0.0F);
        GLManager.GL.Scale(0.5F, -0.5F, 0.5F);
        loadTexture("/terrain.png");
        BlockRenderer.RenderBlockOnInventory(carriedBlock, enderman.GetCarryingData(), 1.0F, Tessellator.instance);
        GLManager.GL.PopMatrix();
        GLManager.GL.Disable(GLEnum.RescaleNormal);
    }

    private double NextGaussian()
    {
        double u1 = 1.0D - _random.NextDouble();
        double u2 = 1.0D - _random.NextDouble();
        return System.Math.Sqrt(-2.0D * System.Math.Log(u1)) * System.Math.Cos(2.0D * System.Math.PI * u2);
    }
}
