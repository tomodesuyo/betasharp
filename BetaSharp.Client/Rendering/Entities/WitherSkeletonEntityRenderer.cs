using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class WitherSkeletonEntityRenderer : UndeadEntityRenderer
{
    public WitherSkeletonEntityRenderer() : base(new ModelSkeleton(), 0.5F)
    {
    }

    protected override void preRenderCallback(EntityLiving var1, float var2)
    {
        GLManager.GL.Scale(1.2F, 1.2F, 1.2F);
    }
}
