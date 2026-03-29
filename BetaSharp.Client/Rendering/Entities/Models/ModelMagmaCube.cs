using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelMagmaCube : ModelBase
{
    private readonly ModelPart[] _segments = new ModelPart[8];
    private readonly ModelPart _core;

    public ModelMagmaCube()
    {
        for (int i = 0; i < _segments.Length; ++i)
        {
            int texU = 0;
            int texV = i;

            if (i == 2)
            {
                texU = 24;
                texV = 10;
            }
            else if (i == 3)
            {
                texU = 24;
                texV = 19;
            }

            _segments[i] = new ModelPart(texU, texV);
            _segments[i].addBox(-4.0F, 16.0F + i, -4.0F, 8, 1, 8);
        }

        _core = new ModelPart(0, 16);
        _core.addBox(-2.0F, 18.0F, -2.0F, 4, 4, 4);
    }

    public int GetModelVersion() => 5;

    public override void setLivingAnimations(EntityLiving entity, float limbAngle, float limbDistance, float tickDelta)
    {
        EntityMagmaCube magmaCube = (EntityMagmaCube)entity;
        float squish = magmaCube.prevSquishAmount + (magmaCube.squishAmount - magmaCube.prevSquishAmount) * tickDelta;
        if (squish < 0.0F)
        {
            squish = 0.0F;
        }

        for (int i = 0; i < _segments.Length; ++i)
        {
            _segments[i].rotationPointY = -(4 - i) * squish * 1.7F;
        }
    }

    public override void render(float limbAngle, float limbDistance, float age, float netHeadYaw, float headPitch, float scale)
    {
        _core.render(scale);
        for (int i = 0; i < _segments.Length; ++i)
        {
            _segments[i].render(scale);
        }
    }
}
