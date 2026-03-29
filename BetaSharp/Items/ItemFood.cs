using BetaSharp.Entities;
using BetaSharp.Potions;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal class ItemFood : Item
{
    private readonly int healAmount;
    private readonly bool isWolfsFavoriteMeat;
    private int potionId;
    private int potionDuration;
    private int potionAmplifier;
    private float potionEffectProbability;

    public ItemFood(int id, int healAmount, bool isWolfsFavoriteMeat) : base(id)
    {
        this.healAmount = healAmount;
        this.isWolfsFavoriteMeat = isWolfsFavoriteMeat;
        maxCount = 1;
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        itemStack.ConsumeItem(entityPlayer);
        entityPlayer.heal(healAmount);
        if (!world.IsRemote && potionId > 0 && world.Random.NextFloat() < potionEffectProbability)
        {
            entityPlayer.addPotionEffect(new PotionEffect(potionId, potionDuration * 20, potionAmplifier));
        }

        return itemStack;
    }

    public ItemFood SetPotionEffect(int id, int durationSeconds, int amplifier, float probability)
    {
        potionId = id;
        potionDuration = durationSeconds;
        potionAmplifier = amplifier;
        potionEffectProbability = probability;
        return this;
    }

    public int getHealAmount()
    {
        return healAmount;
    }

    public bool getIsWolfsFavoriteMeat()
    {
        return isWolfsFavoriteMeat;
    }
}
