using System;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public sealed class ModelSilverfish : ModelBase
{
    private static readonly int[][] s_bodySizes =
    [
        [3, 2, 2],
        [4, 3, 2],
        [6, 4, 3],
        [3, 3, 3],
        [2, 2, 3],
        [2, 1, 2],
        [1, 1, 2],
    ];

    private static readonly int[][] s_textureOffsets =
    [
        [0, 0],
        [0, 4],
        [0, 9],
        [0, 16],
        [0, 22],
        [11, 0],
        [13, 4],
    ];

    private readonly ModelPart[] _bodyParts = new ModelPart[7];
    private readonly ModelPart[] _wings = new ModelPart[3];
    private readonly float[] _bodyZ = new float[7];

    public ModelSilverfish()
    {
        float z = -3.5F;
        for (int i = 0; i < _bodyParts.Length; ++i)
        {
            _bodyParts[i] = new ModelPart(s_textureOffsets[i][0], s_textureOffsets[i][1]).setTextureSize(64, 32);
            _bodyParts[i].addBox(
                s_bodySizes[i][0] * -0.5F,
                0.0F,
                s_bodySizes[i][2] * -0.5F,
                s_bodySizes[i][0],
                s_bodySizes[i][1],
                s_bodySizes[i][2]);
            _bodyParts[i].setRotationPoint(0.0F, 24 - s_bodySizes[i][1], z);
            _bodyZ[i] = z;

            if (i < _bodyParts.Length - 1)
            {
                z += (s_bodySizes[i][2] + s_bodySizes[i + 1][2]) * 0.5F;
            }
        }

        _wings[0] = new ModelPart(20, 0).setTextureSize(64, 32);
        _wings[0].addBox(-5.0F, 0.0F, s_bodySizes[2][2] * -0.5F, 10, 8, s_bodySizes[2][2]);
        _wings[0].setRotationPoint(0.0F, 16.0F, _bodyZ[2]);

        _wings[1] = new ModelPart(20, 11).setTextureSize(64, 32);
        _wings[1].addBox(-3.0F, 0.0F, s_bodySizes[4][2] * -0.5F, 6, 4, s_bodySizes[4][2]);
        _wings[1].setRotationPoint(0.0F, 20.0F, _bodyZ[4]);

        _wings[2] = new ModelPart(20, 18).setTextureSize(64, 32);
        _wings[2].addBox(-3.0F, 0.0F, s_bodySizes[4][2] * -0.5F, 6, 5, s_bodySizes[1][2]);
        _wings[2].setRotationPoint(0.0F, 19.0F, _bodyZ[1]);
    }

    public override void render(float var1, float var2, float var3, float var4, float var5, float var6)
    {
        setRotationAngles(var1, var2, var3, var4, var5, var6);

        for (int i = 0; i < _bodyParts.Length; ++i)
        {
            _bodyParts[i].render(var6);
        }

        for (int i = 0; i < _wings.Length; ++i)
        {
            _wings[i].render(var6);
        }
    }

    public override void setRotationAngles(float var1, float var2, float var3, float var4, float var5, float var6)
    {
        for (int i = 0; i < _bodyParts.Length; ++i)
        {
            float phase = var3 * 0.9F + i * 0.15F * (float)Math.PI;
            _bodyParts[i].rotateAngleY = MathHelper.Cos(phase) * (float)Math.PI * 0.05F * (1 + Math.Abs(i - 2));
            _bodyParts[i].rotationPointX = MathHelper.Sin(phase) * (float)Math.PI * 0.2F * Math.Abs(i - 2);
        }

        _wings[0].rotateAngleY = _bodyParts[2].rotateAngleY;
        _wings[1].rotateAngleY = _bodyParts[4].rotateAngleY;
        _wings[1].rotationPointX = _bodyParts[4].rotationPointX;
        _wings[2].rotateAngleY = _bodyParts[1].rotateAngleY;
        _wings[2].rotationPointX = _bodyParts[1].rotationPointX;
    }
}
