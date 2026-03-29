using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class HorseEntityRenderer : LivingEntityRenderer
{
    public HorseEntityRenderer() : base(new ModelHorse(), 0.75F)
    {
    }

    protected override void preRenderCallback(EntityLiving entity, float tickDelta)
    {
        if (entity is not EntityHorse horse)
        {
            return;
        }

        float scale = horse.HorseType.Value switch
        {
            1 => 0.87F,
            2 => 0.92F,
            _ => 1.0F,
        };

        GLManager.GL.Scale(scale, scale, scale);
    }
}
