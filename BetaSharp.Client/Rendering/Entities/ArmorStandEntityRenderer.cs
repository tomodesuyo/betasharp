using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Items;

namespace BetaSharp.Client.Rendering.Entities;

public sealed class ArmorStandEntityRenderer : LivingEntityRenderer
{
    private static readonly string[] s_armorFilenamePrefix = ["cloth", "chain", "iron", "diamond", "gold"];

    private readonly ModelArmorStand _armorStandModel;
    private readonly ModelArmorStandArmor _armorChestplateModel = new(1.0F);
    private readonly ModelArmorStandArmor _armorModel = new(0.5F);
    private readonly ModelElytra _elytraModel = new();

    public ArmorStandEntityRenderer() : base(new ModelArmorStand(), 0.0F)
    {
        _armorStandModel = (ModelArmorStand)mainModel;
    }

    public override void render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        doRenderLiving((EntityLiving)target, x, y, z, yaw, tickDelta);
    }

    protected override bool shouldRenderPass(EntityLiving entity, int pass, float tickDelta)
    {
        ItemStack? armorStack = ((EntityArmorStand)entity).GetEquipmentStack(4 - pass);
        if (armorStack == null || armorStack.getItem() is not ItemArmor armor)
        {
            return false;
        }

        loadTexture("/armor/" + s_armorFilenamePrefix[armor.renderIndex] + "_" + (pass == 2 ? 2 : 1) + ".png");
        ModelBiped model = pass == 2 ? _armorModel : _armorChestplateModel;
        model.bipedHead.visible = pass == 0;
        model.bipedHeadwear.visible = false;
        model.bipedBody.visible = pass == 1 || pass == 2;
        model.bipedRightArm.visible = pass == 1;
        model.bipedLeftArm.visible = pass == 1;
        model.bipedRightLeg.visible = pass == 2 || pass == 3;
        model.bipedLeftLeg.visible = pass == 2 || pass == 3;
        model.setLivingAnimations(entity, 0.0F, 0.0F, tickDelta);
        setRenderPassModel(model);
        return true;
    }

    protected override bool shouldRenderPassFoil(EntityLiving entity, int pass, float tickDelta)
    {
        ItemStack? armorStack = ((EntityArmorStand)entity).GetEquipmentStack(4 - pass);
        return armorStack != null && armorStack.getItem().hasEffect(armorStack);
    }

    protected override void renderMore(EntityLiving entity, float tickDelta)
    {
        EntityArmorStand armorStand = (EntityArmorStand)entity;
        ItemStack? headStack = armorStand.GetEquipmentStack(4);
        if (headStack != null)
        {
            GLManager.GL.PushMatrix();
            _armorStandModel.bipedHead.transform(1.0F / 16.0F);
            if (headStack.itemId == Item.Skull.id)
            {
                GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
                GLManager.GL.Scale(1.1875F, -1.1875F, -1.1875F);
                SkullRenderHelper.RenderSkull(loadTexture, System.Math.Clamp(headStack.getDamage(), BlockEntitySkull.Skeleton, BlockEntitySkull.Dragon), 180.0F);
            }
            else if (headStack.itemId < 256 && BlockRenderer.IsSideLit(Block.Blocks[headStack.itemId].getRenderType()))
            {
                float scale = 10.0F / 16.0F;
                GLManager.GL.Translate(0.0F, -0.25F, 0.0F);
                GLManager.GL.Rotate(180.0F, 0.0F, 1.0F, 0.0F);
                GLManager.GL.Scale(scale, -scale, scale);
                Dispatcher.heldItemRenderer.renderItem(armorStand, headStack);
            }

            GLManager.GL.PopMatrix();
        }

        ItemStack? chestStack = armorStand.GetEquipmentStack(3);
        if (chestStack != null && chestStack.itemId == Item.Elytra.id)
        {
            GLManager.GL.PushMatrix();
            loadTexture("/mob/elytra.png");
            GLManager.GL.Translate(0.0F, 0.0F, 0.125F);
            _elytraModel.setLivingAnimations(armorStand, 0.0F, 0.0F, tickDelta);
            _elytraModel.render(0.0F, 0.0F, armorStand.age + tickDelta, 0.0F, 0.0F, 1.0F / 16.0F);
            if (chestStack.getItem().hasEffect(chestStack))
            {
                setRenderPassModel(_elytraModel);
                RenderPassFoil(armorStand, 0.0F, 0.0F, armorStand.age + tickDelta, 0.0F, 0.0F, 1.0F / 16.0F, tickDelta);
                setRenderPassModel(null);
            }

            GLManager.GL.PopMatrix();
        }

        ItemStack? handStack = armorStand.GetEquipmentStack(0);
        if (handStack == null || !armorStand.GetShowArms())
        {
            return;
        }

        GLManager.GL.PushMatrix();
        _armorStandModel.bipedRightArm.transform(1.0F / 16.0F);
        GLManager.GL.Translate(-(1.0F / 16.0F), 7.0F / 16.0F, 1.0F / 16.0F);
        if (handStack.itemId < 256 && BlockRenderer.IsSideLit(Block.Blocks[handStack.itemId].getRenderType()))
        {
            float scale = 0.5F;
            GLManager.GL.Translate(0.0F, 3.0F / 16.0F, -(5.0F / 16.0F));
            scale *= 12.0F / 16.0F;
            GLManager.GL.Rotate(20.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(45.0F, 0.0F, 1.0F, 0.0F);
            GLManager.GL.Scale(scale, -scale, scale);
        }
        else if (handStack.getItem().isHandheld())
        {
            float scale = 10.0F / 16.0F;
            if (handStack.getItem().isHandheldRod())
            {
                GLManager.GL.Rotate(180.0F, 0.0F, 0.0F, 1.0F);
                GLManager.GL.Translate(0.0F, -(2.0F / 16.0F), 0.0F);
            }

            GLManager.GL.Translate(0.0F, 3.0F / 16.0F, 0.0F);
            GLManager.GL.Scale(scale, -scale, scale);
            GLManager.GL.Rotate(-100.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(45.0F, 0.0F, 1.0F, 0.0F);
        }
        else
        {
            float scale = 6.0F / 16.0F;
            GLManager.GL.Translate(0.25F, 3.0F / 16.0F, -(3.0F / 16.0F));
            GLManager.GL.Scale(scale, scale, scale);
            GLManager.GL.Rotate(60.0F, 0.0F, 0.0F, 1.0F);
            GLManager.GL.Rotate(-90.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(20.0F, 0.0F, 0.0F, 1.0F);
        }

        Dispatcher.heldItemRenderer.renderItem(armorStand, handStack);
        GLManager.GL.PopMatrix();
    }
}
