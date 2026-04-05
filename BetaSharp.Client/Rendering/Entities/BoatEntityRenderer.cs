using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities;

public class BoatEntityRenderer : EntityRenderer
{
    private readonly ModelBoat _modelBoat;

    public BoatEntityRenderer()
    {
        ShadowRadius = 0.5F;
        _modelBoat = new ModelBoat();
    }

    public void render(EntityBoat var1, double x, double y, double z, float yaw, float tickDelta)
    {
        GLManager.GL.PushMatrix();
        GLManager.GL.Translate((float)x, (float)y + 0.375F, (float)z);
        GLManager.GL.Rotate(180.0F - yaw, 0.0F, 1.0F, 0.0F);
        float var10 = var1.boatTimeSinceHit - tickDelta;
        float var11 = var1.boatCurrentDamage - tickDelta;
        if (var11 < 0.0F)
        {
            var11 = 0.0F;
        }

        if (var10 > 0.0F)
        {
            GLManager.GL.Rotate(MathHelper.Sin(var10) * var10 * var11 / 10.0F * var1.boatRockDirection, 1.0F, 0.0F, 0.0F);
        }

        loadTexture("/mob/boat/boat_oak.png");
        GLManager.GL.Scale(-1.0F, -1.0F, 1.0F);
        _modelBoat.SetRowingTimes(var1.GetRowingTime(0, tickDelta), var1.GetRowingTime(1, tickDelta));
        _modelBoat.render(0.0F, 0.0F, -0.1F, 0.0F, 0.0F, 1.0F / 16.0F);
        GLManager.GL.PopMatrix();
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        render((EntityBoat)target, x, y, z, yaw, tickDelta);
    }
}
