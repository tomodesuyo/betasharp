using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityMooshroom : EntityCow
{
    public override EntityType Type => EntityRegistry.Mooshroom;

    public EntityMooshroom(IWorldContext world) : base(world)
    {
        texture = "/mob/redcow.png";
        setBoundingBoxSpacing(0.9F, 1.3F);
    }

    public override string getTexture() => "/mob/redcow.png";

    public override bool interact(EntityPlayer player)
    {
        ItemStack heldItem = player.inventory.getSelectedItem();
        if (heldItem != null && heldItem.itemId == Item.Bowl.id)
        {
            if (heldItem.count == 1)
            {
                player.inventory.setStack(player.inventory.selectedSlot, new ItemStack(Item.MushroomStew));
                return true;
            }

            if (player.inventory.addItemStackToInventory(new ItemStack(Item.MushroomStew)))
            {
                heldItem.ConsumeItem(player);
                return true;
            }
        }

        if (heldItem != null && heldItem.itemId == Item.Shears.id)
        {
            if (!world.IsRemote)
            {
                markDead();
                world.Broadcaster.AddParticle("largeexplode", x, y + height / 2.0D, z, 0.0D, 0.0D, 0.0D);

                EntityCow cow = new(world);
                cow.setPositionAndAnglesKeepPrevAngles(x, y, z, yaw, pitch);
                cow.health = health;
                world.SpawnEntity(cow);

                for (int i = 0; i < 5; ++i)
                {
                    world.SpawnEntity(new EntityItem(world, x, y + height, z, new ItemStack(Block.RedMushroom)));
                }
            }

            heldItem.damageItem(1, player);
            return true;
        }

        return base.interact(player);
    }
}
