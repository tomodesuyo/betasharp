using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class MagmaCubeEntityRenderer : LivingEntityRenderer
{
    private int _modelVersion;

    public MagmaCubeEntityRenderer() : base(new ModelMagmaCube(), 0.25F)
    {
        _modelVersion = ((ModelMagmaCube)mainModel).GetModelVersion();
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        int modelVersion = ((ModelMagmaCube)mainModel).GetModelVersion();
        if (modelVersion != _modelVersion)
        {
            _modelVersion = modelVersion;
            mainModel = new ModelMagmaCube();
        }

        doRenderLiving((EntityMagmaCube)target, x, y, z, yaw, tickDelta);
    }

    protected override void preRenderCallback(EntityLiving entity, float tickDelta)
    {
        EntityMagmaCube magmaCube = (EntityMagmaCube)entity;
        int size = magmaCube.getSlimeSize();
        float squish = (magmaCube.prevSquishAmount + (magmaCube.squishAmount - magmaCube.prevSquishAmount) * tickDelta) / (size * 0.5F + 1.0F);
        float squishScale = 1.0F / (squish + 1.0F);
        float sizeScale = size;
        GLManager.GL.Scale(squishScale * sizeScale, 1.0F / squishScale * sizeScale, squishScale * sizeScale);
    }
}
