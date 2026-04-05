using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Items;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class SnowGolemEntityRenderer : LivingEntityRenderer
{
    private readonly ModelSnowGolem _model;

    public SnowGolemEntityRenderer() : base(new ModelSnowGolem(), 0.5F)
    {
        _model = (ModelSnowGolem)mainModel;
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        doRenderLiving((EntityLiving)target, x, y, z, yaw, tickDelta);
    }

    protected override void renderMore(EntityLiving entity, float tickDelta)
    {
        GLManager.GL.PushMatrix();
        _model.HeadPart.transform(1.0F / 16.0F);
        if (BlockRenderer.IsSideLit(Block.Pumpkin.getRenderType()))
        {
            float scale = 10.0F / 16.0F;
            GLManager.GL.Translate(0.0F, -0.34375F, 0.0F);
            GLManager.GL.Rotate(180.0F, 0.0F, 1.0F, 0.0F);
            GLManager.GL.Scale(scale, -scale, -scale);
            Dispatcher.heldItemRenderer.renderItem(entity, new ItemStack(Block.Pumpkin));
        }

        GLManager.GL.PopMatrix();
    }
}
