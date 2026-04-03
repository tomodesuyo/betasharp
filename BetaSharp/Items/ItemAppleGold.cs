using BetaSharp.Entities;
using BetaSharp.Potions;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;
using PotionType = BetaSharp.Potions.Potion;

namespace BetaSharp.Items;

internal sealed class ItemAppleGold : ItemFood
{
    public ItemAppleGold(int id, int healAmount, bool isWolfsFavoriteMeat) : base(id, healAmount, isWolfsFavoriteMeat)
    {
        setHasSubtypes(true);
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        itemStack.ConsumeItem(entityPlayer);
        entityPlayer.heal(getHealAmount());

        if (!world.IsRemote)
        {
            entityPlayer.addPotionEffect(new PotionEffect(PotionType.Absorption.Id, 2400, 0));

            if (itemStack.getDamage() > 0)
            {
                entityPlayer.addPotionEffect(new PotionEffect(PotionType.Regeneration.Id, 600, 4));
                entityPlayer.addPotionEffect(new PotionEffect(PotionType.Resistance.Id, 6000, 0));
                entityPlayer.addPotionEffect(new PotionEffect(PotionType.FireResistance.Id, 6000, 0));
            }
            else
            {
                entityPlayer.addPotionEffect(new PotionEffect(PotionType.Regeneration.Id, 100, 1));
            }
        }

        return itemStack;
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        return itemStack.getDamage() > 0 ? "item.appleGold.enchanted" : base.getItemNameIS(itemStack);
    }

    public override bool hasEffect(ItemStack stack)
    {
        return stack.getDamage() > 0;
    }
}
