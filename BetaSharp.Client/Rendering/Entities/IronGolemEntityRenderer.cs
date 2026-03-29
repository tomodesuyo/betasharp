using System;
using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class IronGolemEntityRenderer : LivingEntityRenderer
{
    private readonly ModelIronGolem _model;

    public IronGolemEntityRenderer() : base(new ModelIronGolem(), 0.5F)
    {
        _model = (ModelIronGolem)mainModel;
    }

    protected override void rotateCorpse(EntityLiving entity, float age, float bodyYaw, float tickDelta)
    {
        base.rotateCorpse(entity, age, bodyYaw, tickDelta);
        if (entity.walkAnimationSpeed < 0.01F)
        {
            return;
        }

        float period = 13.0F;
        float phase = entity.animationPhase - entity.walkAnimationSpeed * (1.0F - tickDelta) + 6.0F;
        float sway = (Math.Abs(phase % period - period * 0.5F) - period * 0.25F) / (period * 0.25F);
        GLManager.GL.Rotate(6.5F * sway, 0.0F, 0.0F, 1.0F);
    }

    protected override void renderMore(EntityLiving entity, float tickDelta)
    {
        base.renderMore(entity, tickDelta);
        EntityIronGolem golem = (EntityIronGolem)entity;
        if (golem.OfferRoseTicks == 0)
        {
            return;
        }

        loadTexture("/terrain.png");
        GLManager.GL.PushMatrix();
        _model.RightArm.transform(1.0F / 16.0F);
        GLManager.GL.Rotate(5.0F + 180.0F * _model.RightArm.rotateAngleX / (float)Math.PI, 1.0F, 0.0F, 0.0F);
        GLManager.GL.Translate(-(11.0F / 16.0F), 1.25F, -(15.0F / 16.0F));
        GLManager.GL.Rotate(90.0F, 1.0F, 0.0F, 0.0F);
        GLManager.GL.Scale(0.8F, -0.8F, 0.8F);
        BlockRenderer.RenderBlockOnInventory(Block.Rose, 0, 1.0F, Tessellator.instance);
        GLManager.GL.PopMatrix();
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        doRenderLiving((EntityLiving)target, x, y, z, yaw, tickDelta);
    }
}
