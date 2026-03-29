using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.NBT;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

public class ItemStack
{
    public int count;
    public int bobbingAnimationTime;
    public int itemId;
    private int damage;

    public ItemStack(Block block) : this((Block)block, 1)
    {
    }

    public ItemStack(int id, int count)
    {
        itemId = id;
        this.count = count;
    }

    public ItemStack(Block block, int count) : this(block.id, count, 0)
    {
    }

    public ItemStack(Block block, int count, int damage) : this(block.id, count, damage)
    {
    }

    public ItemStack(Item item) : this(item.id, 1, 0)
    {
    }

    public ItemStack(Item item, int count) : this(item.id, count, 0)
    {
    }

    public ItemStack(Item item, int count, int damage) : this(item.id, count, damage)
    {
    }

    public ItemStack(int itemId, int count, int damage)
    {
        this.count = 0;
        this.itemId = itemId;
        this.count = count;
        this.damage = damage;
    }

    public ItemStack(NBTTagCompound nbt)
    {
        count = 0;
        readFromNBT(nbt);
    }

    public ItemStack split(int splitAmount)
    {
        count -= splitAmount;
        return new ItemStack(itemId, splitAmount, damage);
    }

    public bool HasRegisteredItem()
    {
        return itemId >= 0 && itemId < Item.ITEMS.Length && Item.ITEMS[itemId] != null;
    }

    private Item? ResolveItem()
    {
        return HasRegisteredItem() ? Item.ITEMS[itemId] : null;
    }

    public Item getItem()
    {
        return Item.ITEMS[itemId];
    }

    public int getTextureId()
    {
        Item? item = ResolveItem();
        return item != null ? item.getTextureId(this) : 0;
    }

    public bool useOnBlock(EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int meta)
    {
        Item? item = ResolveItem();
        if (item == null)
        {
            return false;
        }

        bool used = item.useOnBlock(this, entityPlayer, world, x, y, z, meta);
        if (used)
        {
            entityPlayer.increaseStat(Stats.Stats.Used[itemId], 1);
        }

        return used;
    }

    public float getMiningSpeedMultiplier(Block block)
    {
        Item? item = ResolveItem();
        return item != null ? item.getMiningSpeedMultiplier(this, block) : 1.0F;
    }

    public ItemStack use(IWorldContext world, EntityPlayer entityPlayer)
    {
        Item? item = ResolveItem();
        return item != null ? item.use(this, world, entityPlayer) : this;
    }

    public NBTTagCompound writeToNBT(NBTTagCompound nbt)
    {
        nbt.SetShort("id", (short)itemId);
        nbt.SetByte("Count", (sbyte)count);
        nbt.SetShort("Damage", (short)damage);
        return nbt;
    }

    public void readFromNBT(NBTTagCompound nbt)
    {
        itemId = nbt.GetShort("id");
        count = nbt.GetByte("Count");
        damage = nbt.GetShort("Damage");
    }

    public int getMaxCount()
    {
        Item? item = ResolveItem();
        return item != null ? item.getMaxCount() : 64;
    }

    public bool isStackable()
    {
        return getMaxCount() > 1 && (!isDamageable() || !isDamaged());
    }

    public bool isDamageable()
    {
        Item? item = ResolveItem();
        return item != null && item.getMaxDamage() > 0;
    }

    public bool getHasSubtypes()
    {
        Item? item = ResolveItem();
        return item != null && item.getHasSubtypes();
    }

    public bool isDamaged()
    {
        return isDamageable() && damage > 0;
    }

    public int getDamage2()
    {
        return damage;
    }

    public int getDamage()
    {
        return damage;
    }

    public void setDamage(int damage)
    {
        this.damage = damage;
    }

    public int getMaxDamage()
    {
        Item? item = ResolveItem();
        return item != null ? item.getMaxDamage() : 0;
    }

    public void ConsumeItem(EntityPlayer player)
    {
        if (!player.GameMode.FiniteResources) return;
        count--;
    }

    public void damageItem(int damageAmount, Entity entity)
    {
        if (isDamageable())
        {
            if (entity is EntityPlayer player)
            {
                if (!player.GameMode.FiniteResources) return;

                damage += damageAmount;
                if (UpdateBroken())
                {
                    player.increaseStat(Stats.Stats.Broken[itemId], 1);
                }
            }
            else
            {
                damage += damageAmount;
                UpdateBroken();
            }
        }
    }

    private bool UpdateBroken()
    {
        if (damage > getMaxDamage())
        {
            --count;
            if (count < 0) count = 0;
            damage = 0;
            return true;
        }

        return false;
    }

    public void postHit(EntityLiving entityLiving, EntityPlayer entityPlayer)
    {
        Item? item = ResolveItem();
        if (item == null)
        {
            return;
        }

        bool hit = item.postHit(this, entityLiving, entityPlayer);
        if (hit)
        {
            entityPlayer.increaseStat(Stats.Stats.Used[itemId], 1);
        }

    }

    public void postMine(int blockId, int x, int y, int z, EntityPlayer entityPlayer)
    {
        Item? item = ResolveItem();
        if (item == null)
        {
            return;
        }

        bool mined = item.postMine(this, blockId, x, y, z, entityPlayer);
        if (mined)
        {
            entityPlayer.increaseStat(Stats.Stats.Used[itemId], 1);
        }

    }

    public int getAttackDamage(Entity entity)
    {
        Item? item = ResolveItem();
        return item != null ? item.getAttackDamage(entity) : 0;
    }

    public bool isSuitableFor(Block block)
    {
        Item? item = ResolveItem();
        return item != null && item.isSuitableFor(block);
    }

    public void onRemoved(EntityPlayer entityPlayer)
    {
    }

    public void useOnEntity(EntityLiving entityLiving, EntityPlayer entityPlayer)
    {
        Item? item = ResolveItem();
        if (item != null)
        {
            item.useOnEntity(this, entityLiving, entityPlayer);
        }
    }

    public ItemStack copy()
    {
        return new ItemStack(itemId, count, damage);
    }

    public static bool areEqual(ItemStack? a, ItemStack? b)
    {
        return a == null && b == null ? true : (a != null && b != null ? a.equals2(b) : false);
    }

    private bool equals2(ItemStack itemStack)
    {
        return count != itemStack.count ? false : (itemId != itemStack.itemId ? false : damage == itemStack.damage);
    }

    public bool isItemEqual(ItemStack itemStack)
    {
        return itemId == itemStack.itemId && damage == itemStack.damage;
    }

    public string getItemName()
    {
        Item? item = ResolveItem();
        return item != null ? item.getItemNameIS(this) : "item.invalid";
    }

    public static ItemStack clone(ItemStack itemStack)
    {
        return itemStack == null ? null : itemStack.copy();
    }

    public override string ToString()
    {
        Item? item = ResolveItem();
        return count + "x" + (item != null ? item.getItemName() : "invalid") + "@" + damage;
    }

    public void inventoryTick(IWorldContext world, Entity entity, int slotIndex, bool shouldUpdate)
    {
        if (bobbingAnimationTime > 0)
        {
            --bobbingAnimationTime;
        }

        Item? item = ResolveItem();
        if (item != null)
        {
            item.inventoryTick(this, world, entity, slotIndex, shouldUpdate);
        }
    }

    public void onCraft(IWorldContext world, EntityPlayer entityPlayer)
    {
        Item? item = ResolveItem();
        if (item == null)
        {
            return;
        }

        Stats.StatBase[]? craftedStats = Stats.Stats.Crafted;
        if (craftedStats != null && itemId >= 0 && itemId < craftedStats.Length && craftedStats[itemId] != null)
        {
            entityPlayer.increaseStat(craftedStats[itemId], count);
        }

        item.onCraft(this, world, entityPlayer);
    }

    public bool Equals(ItemStack itemStack)
    {
        return itemId == itemStack.itemId && count == itemStack.count && damage == itemStack.damage;
    }
}
