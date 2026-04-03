using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class HorseEntityRenderer : LivingEntityRenderer
{
    private readonly ModelHorse _armorModel = new();

    public HorseEntityRenderer() : base(new ModelHorse(), 0.75F)
    {
        _armorModel.ShowHarness = false;
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

    protected override bool shouldRenderPass(EntityLiving entity, int pass, float tickDelta)
    {
        if (pass != 0 || entity is not EntityHorse horse)
        {
            return false;
        }

        string? armorTexture = horse.GetArmorTexture();
        if (armorTexture == null)
        {
            return false;
        }

        loadTexture(armorTexture);
        _armorModel.setLivingAnimations(entity, 0.0F, 0.0F, tickDelta);
        setRenderPassModel(_armorModel);
        return true;
    }
}
