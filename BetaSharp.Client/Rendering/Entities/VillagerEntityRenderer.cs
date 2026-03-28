using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class VillagerEntityRenderer : LivingEntityRenderer
{
    public VillagerEntityRenderer() : base(new ModelVillager(), 0.5F)
    {
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        doRenderLiving((EntityLiving)target, x, y, z, yaw, tickDelta);
    }
}
