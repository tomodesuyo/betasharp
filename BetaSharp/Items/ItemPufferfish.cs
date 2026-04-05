using BetaSharp.Entities;
using BetaSharp.Potions;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemPufferfish : ItemFood
{
    public ItemPufferfish(int id) : base(id, 1, false)
    {
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        ItemStack result = base.use(itemStack, world, entityPlayer);
        if (!world.IsRemote)
        {
            entityPlayer.addPotionEffect(new PotionEffect(BetaSharp.Potions.Potion.Poison.Id, 1200, 3));
            entityPlayer.addPotionEffect(new PotionEffect(BetaSharp.Potions.Potion.Hunger.Id, 300, 2));
            entityPlayer.addPotionEffect(new PotionEffect(BetaSharp.Potions.Potion.Confusion.Id, 300, 1));
        }

        return result;
    }
}
