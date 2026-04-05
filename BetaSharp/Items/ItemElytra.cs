using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemElytra : Item
{
    public ItemElytra(int id) : base(id)
    {
        setMaxCount(1);
        setMaxDamage(432);
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        if (entityPlayer.inventory.armor[2] != null)
        {
            return itemStack;
        }

        ItemStack equipped = itemStack.copy();
        equipped.count = 1;
        entityPlayer.inventory.armor[2] = equipped;
        entityPlayer.inventory.dirty = true;

        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            itemStack.count = 0;
        }

        return itemStack;
    }

    public static bool IsEquipped(EntityPlayer player)
    {
        ItemStack? chestItem = player.inventory.armor[2];
        return chestItem != null && chestItem.itemId == Item.Elytra.id && IsUsable(chestItem);
    }

    public static bool IsUsable(ItemStack stack)
    {
        return stack.getDamage() < stack.getMaxDamage() - 1;
    }
}
