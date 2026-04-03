using BetaSharp.Items;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public class EntityCow : EntityAnimal
{
    public override EntityType Type => EntityRegistry.Cow;
    
    public EntityCow(IWorldContext world) : base(world)
    {
        this.texture = "/mob/cow.png";
        this.setBoundingBoxSpacing(0.9F, 1.3F);
    }

    protected override string getLivingSound()
    {
        return "mob.cow";
    }

    protected override string getHurtSound()
    {
        return "mob.cowhurt";
    }

    protected override string getDeathSound()
    {
        return "mob.cowhurt";
    }

    protected override float getSoundVolume()
    {
        return 0.4F;
    }

    protected override int getDropItemId()
    {
        return Item.Leather.id;
    }

    protected override void dropFewItems()
    {
        int leatherCount = random.NextInt(3);
        for (int i = 0; i < leatherCount; ++i)
        {
            dropItem(Item.Leather.id, 1);
        }

        int beefCount = random.NextInt(3) + 1;
        int beefItemId = fireTicks > 0 ? Item.CookedBeef.id : Item.RawBeef.id;
        for (int i = 0; i < beefCount; ++i)
        {
            dropItem(beefItemId, 1);
        }
    }

    public override bool interact(EntityPlayer player)
    {
        ItemStack heldBucket = player.inventory.getSelectedItem();
        if (heldBucket != null && heldBucket.itemId == Item.Bucket.id)
        {
            player.inventory.setStack(player.inventory.selectedSlot, new ItemStack(Item.MilkBucket));
            return true;
        }
        else
        {
            return false;
        }
    }
}
