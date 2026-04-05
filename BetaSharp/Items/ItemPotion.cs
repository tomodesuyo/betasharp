using BetaSharp.Entities;
using BetaSharp.Potions;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemPotion : Item
{
    private readonly Dictionary<int, List<PotionEffect>?> _effectCache = new();

    public ItemPotion(int id) : base(id)
    {
        setMaxCount(1);
        setHasSubtypes(true);
        setMaxDamage(0);
    }

    public List<PotionEffect>? GetEffects(ItemStack stack)
    {
        return GetEffects(stack.getDamage());
    }

    public List<PotionEffect>? GetEffects(int potionDamage)
    {
        if (!_effectCache.TryGetValue(potionDamage, out List<PotionEffect>? effects))
        {
            effects = PotionHelper.GetPotionEffects(potionDamage, false);
            _effectCache[potionDamage] = effects;
        }

        return effects;
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        if (IsSplash(itemStack.getDamage()))
        {
            itemStack.ConsumeItem(entityPlayer);
            world.Broadcaster.PlaySoundAtEntity(entityPlayer, "random.bow", 0.5F, 0.4F / (itemRand.NextFloat() * 0.4F + 0.8F));
            if (!world.IsRemote)
            {
                world.SpawnEntity(new EntityPotion(world, entityPlayer, itemStack.getDamage()));
            }

            return itemStack;
        }

        if (!world.IsRemote)
        {
            List<PotionEffect>? effects = GetEffects(itemStack);
            if (effects != null)
            {
                for (int i = 0; i < effects.Count; ++i)
                {
                    entityPlayer.addPotionEffect(new PotionEffect(effects[i]));
                }
            }
        }

        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            --itemStack.count;
        }

        if (itemStack.count <= 0)
        {
            return entityPlayer.capabilities.IsCreativeMode ? itemStack : new ItemStack(Item.GlassBottle);
        }

        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            entityPlayer.inventory.addItemStackToInventory(new ItemStack(Item.GlassBottle));
        }

        return itemStack;
    }

    public override int getTextureId(int damage)
    {
        return IsSplash(damage) ? 154 : 140;
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        if (itemStack.getDamage() == 0)
        {
            return "item.emptyPotion";
        }

        List<PotionEffect>? effects = GetEffects(itemStack);
        if (effects != null && effects.Count > 0)
        {
            BetaSharp.Potions.Potion? potion = BetaSharp.Potions.Potion.PotionTypes[effects[0].PotionId];
            if (potion != null)
            {
                return potion.Name;
            }
        }

        return base.getItemNameIS(itemStack);
    }

    public override bool requiresMultipleRenderPasses()
    {
        return true;
    }

    public override int getTextureId(int damage, int pass)
    {
        return pass == 0 ? 141 : getTextureId(damage);
    }

    public override int getColorMultiplier(int damage, int pass)
    {
        return pass > 0 ? 0xFFFFFF : PotionHelper.GetColor(damage, false);
    }

    public override bool hasEffect(ItemStack stack)
    {
        if (stack.getDamage() == 0)
        {
            return false;
        }

        List<PotionEffect>? effects = GetEffects(stack);
        return effects != null && effects.Count > 0;
    }

    public static bool IsSplash(int potionDamage)
    {
        return (potionDamage & 16384) != 0;
    }
}
