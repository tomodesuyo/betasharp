using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class MooshroomEntityRenderer : CowEntityRenderer
{
    public MooshroomEntityRenderer(ModelBase mainModel, float shadowRadius) : base(mainModel, shadowRadius)
    {
    }

    protected override void renderMore(EntityLiving var1, float var2)
    {
        base.renderMore(var1, var2);
        EntityMooshroom mooshroom = (EntityMooshroom)var1;
        loadTexture("/terrain.png");
        GLManager.GL.Enable(GLEnum.CullFace);

        GLManager.GL.PushMatrix();
        GLManager.GL.Scale(1.0F, -1.0F, 1.0F);
        GLManager.GL.Translate(0.2F, 0.4F, 0.5F);
        GLManager.GL.Rotate(42.0F, 0.0F, 1.0F, 0.0F);
        BlockRenderer.RenderBlockOnInventory(Block.RedMushroom, 0, 1.0F, Tessellator.instance);
        GLManager.GL.Translate(0.1F, 0.0F, -0.6F);
        GLManager.GL.Rotate(42.0F, 0.0F, 1.0F, 0.0F);
        BlockRenderer.RenderBlockOnInventory(Block.RedMushroom, 0, 1.0F, Tessellator.instance);
        GLManager.GL.PopMatrix();

        GLManager.GL.PushMatrix();
        ((ModelQuadruped)mainModel).head.transform(1.0F / 16.0F);
        GLManager.GL.Scale(1.0F, -1.0F, 1.0F);
        GLManager.GL.Translate(0.0F, 12.0F / 16.0F, -0.2F);
        GLManager.GL.Rotate(12.0F, 0.0F, 1.0F, 0.0F);
        BlockRenderer.RenderBlockOnInventory(Block.RedMushroom, 0, 1.0F, Tessellator.instance);
        GLManager.GL.PopMatrix();

        GLManager.GL.Disable(GLEnum.CullFace);
    }
}
