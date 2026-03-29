using System;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelBlaze : ModelBase
{
    private readonly ModelPart[] _rods = new ModelPart[12];
    private readonly ModelPart _head;

    public ModelBlaze()
    {
        for (int i = 0; i < _rods.Length; ++i)
        {
            _rods[i] = new ModelPart(0, 16).setTextureSize(64, 32);
            _rods[i].addBox(0.0F, 0.0F, 0.0F, 2, 8, 2);
        }

        _head = new ModelPart(0, 0).setTextureSize(64, 32);
        _head.addBox(-4.0F, -4.0F, -4.0F, 8, 8, 8);
    }

    public override void render(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        setRotationAngles(limbAngle, limbDistance, age, yaw, pitch, scale);
        _head.render(scale);

        for (int i = 0; i < _rods.Length; ++i)
        {
            _rods[i].render(scale);
        }
    }

    public override void setRotationAngles(float limbAngle, float limbDistance, float age, float yaw, float pitch, float scale)
    {
        float phase = age * (float)Math.PI * -0.1F;

        for (int i = 0; i < 4; ++i)
        {
            _rods[i].rotationPointY = -2.0F + MathHelper.Cos((i * 2.0F + age) * 0.25F);
            _rods[i].rotationPointX = MathHelper.Cos(phase) * 9.0F;
            _rods[i].rotationPointZ = MathHelper.Sin(phase) * 9.0F;
            phase += (float)Math.PI * 0.5F;
        }

        phase = (float)Math.PI * 0.25F + age * (float)Math.PI * 0.03F;
        for (int i = 4; i < 8; ++i)
        {
            _rods[i].rotationPointY = 2.0F + MathHelper.Cos((i * 2.0F + age) * 0.25F);
            _rods[i].rotationPointX = MathHelper.Cos(phase) * 7.0F;
            _rods[i].rotationPointZ = MathHelper.Sin(phase) * 7.0F;
            phase += (float)Math.PI * 0.5F;
        }

        phase = (float)Math.PI * 0.15F + age * (float)Math.PI * -0.05F;
        for (int i = 8; i < 12; ++i)
        {
            _rods[i].rotationPointY = 11.0F + MathHelper.Cos((i * 1.5F + age) * 0.5F);
            _rods[i].rotationPointX = MathHelper.Cos(phase) * 5.0F;
            _rods[i].rotationPointZ = MathHelper.Sin(phase) * 5.0F;
            phase += (float)Math.PI * 0.5F;
        }

        _head.rotateAngleY = yaw / (180.0F / (float)Math.PI);
        _head.rotateAngleX = pitch / (180.0F / (float)Math.PI);
    }
}
