using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class SilverfishEntityRenderer : LivingEntityRenderer
{
    public SilverfishEntityRenderer() : base(new ModelSilverfish(), 0.3F)
    {
    }

    protected override float getDeathMaxRotation(EntityLiving var1)
    {
        return 180.0F;
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        doRenderLiving((EntitySilverfish)target, x, y, z, yaw, tickDelta);
    }
}
