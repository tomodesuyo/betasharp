using System;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class DragonEntityRenderer : LivingEntityRenderer
{
    public DragonEntityRenderer() : base(new ModelDragon(), 0.5F)
    {
        setRenderPassModel(mainModel);
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        EntityDragon dragon = (EntityDragon)target;
        doRenderLiving(dragon, x, y, z, yaw, tickDelta);
        RenderHealingBeam(dragon, x, y, z, tickDelta);
    }

    protected override void rotateCorpse(EntityLiving entity, float age, float bodyYaw, float tickDelta)
    {
        EntityDragon dragon = (EntityDragon)entity;
        float yawOffset = (float)dragon.GetMovementOffsets(7, tickDelta)[0];
        float pitchOffset = (float)(dragon.GetMovementOffsets(5, tickDelta)[1] - dragon.GetMovementOffsets(10, tickDelta)[1]);
        GLManager.GL.Rotate(-yawOffset, 0.0F, 1.0F, 0.0F);
        GLManager.GL.Rotate(pitchOffset * 10.0F, 1.0F, 0.0F, 0.0F);
        GLManager.GL.Translate(0.0F, 0.0F, 1.0F);
        if (dragon.deathTime > 0)
        {
            float deathProgress = (dragon.deathTime + tickDelta - 1.0F) / 20.0F * 1.6F;
            deathProgress = MathHelper.Sqrt(deathProgress);
            if (deathProgress > 1.0F)
            {
                deathProgress = 1.0F;
            }

            GLManager.GL.Rotate(deathProgress * getDeathMaxRotation(entity), 0.0F, 0.0F, 1.0F);
        }
    }

    protected override bool shouldRenderPass(EntityLiving entity, int pass, float tickDelta)
    {
        if (pass == 1)
        {
            GLManager.GL.DepthFunc(GLEnum.Lequal);
            return false;
        }

        if (pass != 0)
        {
            return false;
        }

        loadTexture("/mob/enderdragon/ender_eyes.png");
        GLManager.GL.Enable(GLEnum.Blend);
        GLManager.GL.Disable(GLEnum.AlphaTest);
        GLManager.GL.BlendFunc(GLEnum.One, GLEnum.One);
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        return true;
    }

    protected override void RenderMainModel(EntityLiving entity, float limbAngle, float limbDistance, float age, float netHeadYaw, float headPitch, float scale, float tickDelta)
    {
        EntityDragon dragon = (EntityDragon)entity;
        if (dragon.DeathTicks > 0)
        {
            float deathProgress = (dragon.DeathTicks + tickDelta) / 200.0F;
            GLManager.GL.DepthFunc(GLEnum.Lequal);
            GLManager.GL.Enable(GLEnum.AlphaTest);
            GLManager.GL.AlphaFunc(GLEnum.Greater, deathProgress);
            loadTexture("/mob/enderdragon/shuffle.png");
            mainModel.render(limbAngle, limbDistance, age, netHeadYaw, headPitch, scale);
            GLManager.GL.AlphaFunc(GLEnum.Greater, 0.1F);
            GLManager.GL.DepthFunc(GLEnum.Equal);
        }

        loadTexture(dragon.getTexture());
        mainModel.render(limbAngle, limbDistance, age, netHeadYaw, headPitch, scale);
    }

    protected override void renderMore(EntityLiving entity, float tickDelta)
    {
        base.renderMore(entity, tickDelta);

        EntityDragon dragon = (EntityDragon)entity;
        if (dragon.DeathTicks <= 0)
        {
            return;
        }

        Tessellator tessellator = Tessellator.instance;
        float deathProgress = (dragon.DeathTicks + tickDelta) / 200.0F;
        float fadeProgress = deathProgress > 0.8F ? (deathProgress - 0.8F) / 0.2F : 0.0F;
        Random random = new(432);

        Lighting.turnOff();
        GLManager.GL.Disable(GLEnum.Texture2D);
        GLManager.GL.ShadeModel(GLEnum.Smooth);
        GLManager.GL.Enable(GLEnum.Blend);
        GLManager.GL.BlendFunc(GLEnum.SrcAlpha, GLEnum.One);
        GLManager.GL.Disable(GLEnum.AlphaTest);
        GLManager.GL.Enable(GLEnum.CullFace);
        GLManager.GL.DepthMask(false);
        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(0.0F, -1.0F, -2.0F);

        int rayCount = (int)(((deathProgress + deathProgress * deathProgress) / 2.0F) * 60.0F);
        for (int i = 0; i < rayCount; ++i)
        {
            GLManager.GL.Rotate(random.NextSingle() * 360.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(random.NextSingle() * 360.0F, 0.0F, 1.0F, 0.0F);
            GLManager.GL.Rotate(random.NextSingle() * 360.0F, 0.0F, 0.0F, 1.0F);
            GLManager.GL.Rotate(random.NextSingle() * 360.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(random.NextSingle() * 360.0F, 0.0F, 1.0F, 0.0F);
            GLManager.GL.Rotate(random.NextSingle() * 360.0F + deathProgress * 90.0F, 0.0F, 0.0F, 1.0F);

            tessellator.startDrawing(6);
            float rayHeight = random.NextSingle() * 20.0F + 5.0F + fadeProgress * 10.0F;
            float rayWidth = random.NextSingle() * 2.0F + 1.0F + fadeProgress * 2.0F;
            tessellator.setColorRGBA_I(0xFFFFFF, (int)(255.0F * (1.0F - fadeProgress)));
            tessellator.addVertex(0.0D, 0.0D, 0.0D);
            tessellator.setColorRGBA_I(0xFF00FF, 0);
            tessellator.addVertex(-0.866D * rayWidth, rayHeight, -0.5D * rayWidth);
            tessellator.addVertex(0.866D * rayWidth, rayHeight, -0.5D * rayWidth);
            tessellator.addVertex(0.0D, rayHeight, rayWidth);
            tessellator.addVertex(-0.866D * rayWidth, rayHeight, -0.5D * rayWidth);
            tessellator.draw();
        }

        GLManager.GL.PopMatrix();
        GLManager.GL.DepthMask(true);
        GLManager.GL.Disable(GLEnum.CullFace);
        GLManager.GL.Disable(GLEnum.Blend);
        GLManager.GL.ShadeModel(GLEnum.Flat);
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        GLManager.GL.Enable(GLEnum.Texture2D);
        GLManager.GL.Enable(GLEnum.AlphaTest);
        Lighting.turnOn();
    }

    private void RenderHealingBeam(EntityDragon dragon, double x, double y, double z, float tickDelta)
    {
        EntityEnderCrystal? crystal = dragon.HealingEnderCrystal;
        if (crystal == null)
        {
            return;
        }

        float rotation = crystal.InnerRotation + tickDelta;
        float bob = MathHelper.Sin(rotation * 0.2F) / 2.0F + 0.5F;
        bob = (bob * bob + bob) * 0.2F;

        double interpDragonX = dragon.lastTickX + (dragon.x - dragon.lastTickX) * tickDelta;
        double interpDragonY = dragon.lastTickY + (dragon.y - dragon.lastTickY) * tickDelta;
        double interpDragonZ = dragon.lastTickZ + (dragon.z - dragon.lastTickZ) * tickDelta;
        double interpCrystalX = crystal.lastTickX + (crystal.x - crystal.lastTickX) * tickDelta;
        double interpCrystalY = crystal.lastTickY + (crystal.y - crystal.lastTickY) * tickDelta;
        double interpCrystalZ = crystal.lastTickZ + (crystal.z - crystal.lastTickZ) * tickDelta;

        float beamX = (float)(interpCrystalX - interpDragonX);
        float beamY = (float)(bob + interpCrystalY - 1.0D - interpDragonY);
        float beamZ = (float)(interpCrystalZ - interpDragonZ);
        float horizontal = MathHelper.Sqrt(beamX * beamX + beamZ * beamZ);
        float length = MathHelper.Sqrt(beamX * beamX + beamY * beamY + beamZ * beamZ);

        GLManager.GL.PushMatrix();
        GLManager.GL.Translate((float)x, (float)y + 2.0F, (float)z);
        GLManager.GL.Rotate((float)(-Math.Atan2(beamZ, beamX)) * 180.0F / (float)Math.PI - 90.0F, 0.0F, 1.0F, 0.0F);
        GLManager.GL.Rotate((float)(-Math.Atan2(horizontal, beamY)) * 180.0F / (float)Math.PI - 90.0F, 1.0F, 0.0F, 0.0F);
        GLManager.GL.Disable(GLEnum.CullFace);
        loadTexture("/mob/enderdragon/beam.png");
        GLManager.GL.ShadeModel(GLEnum.Smooth);

        float vMin = -((dragon.age + tickDelta) * 0.01F);
        float vMax = length / 32.0F - ((dragon.age + tickDelta) * 0.01F);
        Tessellator tessellator = Tessellator.instance;
        tessellator.startDrawing(5);

        const int segments = 8;
        for (int i = 0; i <= segments; ++i)
        {
            float angle = (i % segments) * (float)Math.PI * 2.0F / segments;
            float px = MathHelper.Sin(angle) * (12.0F / 16.0F);
            float pz = MathHelper.Cos(angle) * (12.0F / 16.0F);
            float u = (i % segments) / (float)segments;
            tessellator.setColorOpaque_I(0);
            tessellator.addVertexWithUV(px * 0.2F, pz * 0.2F, 0.0D, u, vMax);
            tessellator.setColorOpaque_I(0xFFFFFF);
            tessellator.addVertexWithUV(px, pz, length, u, vMin);
        }

        tessellator.draw();
        GLManager.GL.Enable(GLEnum.CullFace);
        GLManager.GL.ShadeModel(GLEnum.Flat);
        GLManager.GL.PopMatrix();
    }
}
