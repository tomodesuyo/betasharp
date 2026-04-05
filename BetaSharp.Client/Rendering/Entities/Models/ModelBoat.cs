using BetaSharp.Client.Rendering.Core;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities.Models;

public class ModelBoat : ModelBase
{
    private readonly ModelPart[] _boatSides = new ModelPart[5];
    private readonly ModelPart[] _paddles = new ModelPart[2];
    private float _leftRowingTime;
    private float _rightRowingTime;

    public ModelBoat()
    {
        _boatSides[0] = new ModelPart(0, 0).setTextureSize(128, 64);
        _boatSides[1] = new ModelPart(0, 19).setTextureSize(128, 64);
        _boatSides[2] = new ModelPart(0, 27).setTextureSize(128, 64);
        _boatSides[3] = new ModelPart(0, 35).setTextureSize(128, 64);
        _boatSides[4] = new ModelPart(0, 43).setTextureSize(128, 64);
        _boatSides[0].addBox(-14.0F, -9.0F, -3.0F, 28, 16, 3, 0.0F);
        _boatSides[0].setRotationPoint(0.0F, 3.0F, 1.0F);
        _boatSides[1].addBox(-13.0F, -7.0F, -1.0F, 18, 6, 2, 0.0F);
        _boatSides[1].setRotationPoint(-15.0F, 4.0F, 4.0F);
        _boatSides[2].addBox(-8.0F, -7.0F, -1.0F, 16, 6, 2, 0.0F);
        _boatSides[2].setRotationPoint(15.0F, 4.0F, 0.0F);
        _boatSides[3].addBox(-14.0F, -7.0F, -1.0F, 28, 6, 2, 0.0F);
        _boatSides[3].setRotationPoint(0.0F, 4.0F, -9.0F);
        _boatSides[4].addBox(-14.0F, -7.0F, -1.0F, 28, 6, 2, 0.0F);
        _boatSides[4].setRotationPoint(0.0F, 4.0F, 9.0F);
        _boatSides[0].rotateAngleX = (float)Math.PI / 2.0F;
        _boatSides[1].rotateAngleY = (float)Math.PI * 3.0F / 2.0F;
        _boatSides[2].rotateAngleY = (float)Math.PI / 2.0F;
        _boatSides[3].rotateAngleY = (float)Math.PI;

        _paddles[0] = MakePaddle(true);
        _paddles[0].setRotationPoint(3.0F, -5.0F, 9.0F);
        _paddles[1] = MakePaddle(false);
        _paddles[1].setRotationPoint(3.0F, -5.0F, -9.0F);
        _paddles[1].rotateAngleY = (float)Math.PI;
        _paddles[0].rotateAngleZ = 0.19634955F;
        _paddles[1].rotateAngleZ = 0.19634955F;
    }

    public void SetRowingTimes(float leftRowingTime, float rightRowingTime)
    {
        _leftRowingTime = leftRowingTime;
        _rightRowingTime = rightRowingTime;
    }

    public override void render(float var1, float var2, float var3, float var4, float var5, float scale)
    {
        GLManager.GL.Rotate(90.0F, 0.0F, 1.0F, 0.0F);

        for (int i = 0; i < _boatSides.Length; ++i)
        {
            _boatSides[i].render(scale);
        }

        RenderPaddle(0, _leftRowingTime, scale);
        RenderPaddle(1, _rightRowingTime, scale);
    }

    private static float Lerp(float start, float end, float delta)
    {
        return start + (end - start) * Math.Clamp(delta, 0.0F, 1.0F);
    }

    private static ModelPart MakePaddle(bool left)
    {
        ModelPart paddle = new ModelPart(62, left ? 0 : 20).setTextureSize(128, 64);
        paddle.addBox(-1.0F, 0.0F, -5.0F, 2, 2, 18);
        paddle.addBox(left ? -1.001F : 0.001F, -3.0F, 8.0F, 1, 6, 7);
        return paddle;
    }

    private void RenderPaddle(int paddleIndex, float rowingTime, float scale)
    {
        ModelPart paddle = _paddles[paddleIndex];
        float swing = (MathHelper.Sin(-rowingTime) + 1.0F) * 0.5F;
        float yawSwing = (MathHelper.Sin(-rowingTime + 1.0F) + 1.0F) * 0.5F;
        paddle.rotateAngleX = Lerp(-1.0471976F, -0.2617994F, swing);
        paddle.rotateAngleY = Lerp(-(float)Math.PI / 4.0F, (float)Math.PI / 4.0F, yawSwing);
        if (paddleIndex == 1)
        {
            paddle.rotateAngleY = (float)Math.PI - paddle.rotateAngleY;
        }

        paddle.render(scale);
    }
}
