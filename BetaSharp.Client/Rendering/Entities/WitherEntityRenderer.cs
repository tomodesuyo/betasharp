using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class WitherEntityRenderer : LivingEntityRenderer
{
    public WitherEntityRenderer() : base(new ModelWither(), 1.0F)
    {
    }

    protected override void preRenderCallback(EntityLiving entity, float tickDelta)
    {
        GLManager.GL.Scale(2.0F, 2.0F, 2.0F);
    }
}
