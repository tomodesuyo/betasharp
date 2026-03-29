using BetaSharp.Blocks.Materials;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockCauldron : Block
{
    public BlockCauldron(int id) : base(id, 154, Material.Metal)
    {
    }

    public override int getTexture(int side, int meta) => side == 1 ? 138 : side == 0 ? 155 : 154;

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 5.0F / 16.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        float thickness = 2.0F / 16.0F;
        setBoundingBox(0.0F, 0.0F, 0.0F, thickness, 1.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, thickness);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(1.0F - thickness, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(0.0F, 0.0F, 1.0F - thickness, 1.0F, 1.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setupRenderBoundingBox();
    }

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Cauldron;

    public override int getDroppedItemId(int blockMeta) => Items.Item.Cauldron.id;

    public override bool onUse(OnUseEvent @event)
    {
        ItemStack? heldStack = @event.Player.getHand();
        if (heldStack == null)
        {
            return false;
        }

        int level = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (level < 0)
        {
            level = 0;
        }
        else if (level > 3)
        {
            level = 3;
        }

        if (heldStack.itemId == Items.Item.WaterBucket.id)
        {
            if (level >= 3)
            {
                return true;
            }

            if (!@event.Player.capabilities.IsCreativeMode)
            {
                @event.Player.inventory.setStack(@event.Player.inventory.selectedSlot, new ItemStack(Items.Item.Bucket));
            }

            @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, 3);
            return true;
        }

        if (heldStack.itemId != Items.Item.GlassBottle.id || level <= 0)
        {
            return false;
        }

        ItemStack filledBottle = new(Items.Item.Potion, 1, 0);
        if (@event.Player.capabilities.IsCreativeMode)
        {
            if (!@event.Player.inventory.addItemStackToInventory(filledBottle.copy()))
            {
                @event.Player.dropItem(filledBottle.copy());
            }
        }
        else if (heldStack.count == 1)
        {
            @event.Player.inventory.setStack(@event.Player.inventory.selectedSlot, filledBottle);
        }
        else
        {
            --heldStack.count;
            if (!@event.Player.inventory.addItemStackToInventory(filledBottle.copy()))
            {
                @event.Player.dropItem(filledBottle.copy());
            }
        }

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, level - 1);
        return true;
    }

    public override void onEntityCollision(OnEntityCollisionEvent @event)
    {
        int level = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (@event.World.IsRemote || level <= 0 || @event.Entity.fireTicks <= 0)
        {
            return;
        }

        double waterY = @event.Y + (6.0D + 3.0D * level) / 16.0D;
        if (@event.Entity.boundingBox.MinY > waterY)
        {
            return;
        }

        @event.Entity.fireTicks = 0;
        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, level - 1);
    }
}
