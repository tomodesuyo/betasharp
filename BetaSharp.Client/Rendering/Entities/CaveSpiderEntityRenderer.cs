using BetaSharp.Client.Rendering.Core;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class CaveSpiderEntityRenderer : SpiderEntityRenderer
{
    protected override void preRenderCallback(EntityLiving var1, float var2)
    {
        GLManager.GL.Scale(0.7F, 0.7F, 0.7F);
    }
}
