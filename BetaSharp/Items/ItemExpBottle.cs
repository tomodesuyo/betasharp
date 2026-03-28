using BetaSharp.Entities;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemExpBottle : Item
{
    public ItemExpBottle(int id) : base(id)
    {
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            itemStack.ConsumeItem(entityPlayer);
        }

        world.Broadcaster.PlaySoundAtEntity(entityPlayer, "random.bow", 0.5F, 0.4F / (itemRand.NextFloat() * 0.4F + 0.8F));
        if (!world.IsRemote)
        {
            world.SpawnEntity(new EntityExpBottle(world, entityPlayer));
        }

        return itemStack;
    }
}
