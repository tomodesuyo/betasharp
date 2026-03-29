using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Potions;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks.Entities;

public class BlockEntityBrewingStand : BlockEntity, IInventory
{
    private readonly ItemStack?[] _inventory = new ItemStack[4];
    private int _filledSlots;
    private int _ingredientId;

    public override BlockEntityType Type => BlockEntity.BrewingStand;
    public int brewTime;

    public int size() => _inventory.Length;

    public ItemStack? getStack(int slot)
    {
        return slot >= 0 && slot < _inventory.Length ? _inventory[slot] : null;
    }

    public ItemStack? removeStack(int slot, int amount)
    {
        if (slot < 0 || slot >= _inventory.Length || _inventory[slot] == null)
        {
            return null;
        }

        ItemStack stack = _inventory[slot]!;
        if (stack.count <= amount)
        {
            _inventory[slot] = null;
            markDirty();
            return stack;
        }

        ItemStack removed = stack.split(amount);
        if (stack.count == 0)
        {
            _inventory[slot] = null;
        }

        markDirty();
        return removed;
    }

    public void setStack(int slot, ItemStack? stack)
    {
        if (slot < 0 || slot >= _inventory.Length)
        {
            return;
        }

        _inventory[slot] = stack;
        if (stack != null && stack.count > getMaxCountPerStack())
        {
            stack.count = getMaxCountPerStack();
        }

        markDirty();
    }

    public string getName() => "Brewing Stand";

    public int getMaxCountPerStack() => 64;

    public bool canPlayerUse(EntityPlayer player)
    {
        return World.Entities.GetBlockEntity<BlockEntityBrewingStand>(X, Y, Z) == this
               && player.getSquaredDistance(X + 0.5D, Y + 0.5D, Z + 0.5D) <= 64.0D;
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        NBTTagList items = nbt.GetTagList("Items");

        Array.Clear(_inventory, 0, _inventory.Length);
        for (int i = 0; i < items.TagCount(); ++i)
        {
            NBTTagCompound itemTag = (NBTTagCompound)items.TagAt(i);
            int slot = itemTag.GetByte("Slot");
            if (slot >= 0 && slot < _inventory.Length)
            {
                _inventory[slot] = new ItemStack(itemTag);
            }
        }

        brewTime = nbt.GetShort("BrewTime");
        _filledSlots = GetFilledSlots();
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetShort("BrewTime", (short)brewTime);
        NBTTagList items = new();
        for (int i = 0; i < _inventory.Length; ++i)
        {
            if (_inventory[i] != null)
            {
                NBTTagCompound itemTag = new();
                itemTag.SetByte("Slot", (sbyte)i);
                _inventory[i]!.writeToNBT(itemTag);
                items.SetTag(itemTag);
            }
        }

        nbt.SetTag("Items", items);
    }

    public override void tick(EntityManager entities)
    {
        if (brewTime > 0)
        {
            --brewTime;
            if (brewTime == 0)
            {
                BrewPotions();
                markDirty();
            }
            else if (!CanBrew() || _inventory[3] == null || _ingredientId != _inventory[3]!.itemId)
            {
                brewTime = 0;
                markDirty();
            }
        }
        else if (CanBrew())
        {
            brewTime = 400;
            _ingredientId = _inventory[3]!.itemId;
        }

        UpdateFilledSlots();
    }

    public int GetBrewProgress(int pixels)
    {
        return brewTime * pixels / 400;
    }

    public int GetFilledSlots()
    {
        int filled = 0;
        for (int i = 0; i < 3; ++i)
        {
            if (_inventory[i] != null)
            {
                filled |= 1 << i;
            }
        }

        return filled;
    }

    public void markDirty()
    {
        UpdateFilledSlots();
        base.markDirty();
    }

    private void UpdateFilledSlots()
    {
        int filledSlots = GetFilledSlots();
        if (World != null && !World.IsRemote && filledSlots != _filledSlots)
        {
            _filledSlots = filledSlots;
            World.Writer.SetBlockMeta(X, Y, Z, filledSlots);
        }
        else
        {
            _filledSlots = filledSlots;
        }
    }

    private bool CanBrew()
    {
        ItemStack? ingredient = _inventory[3];
        if (ingredient == null || ingredient.count <= 0 || !ingredient.getItem().isPotionIngredient())
        {
            return false;
        }

        for (int i = 0; i < 3; ++i)
        {
            ItemStack? potionStack = _inventory[i];
            if (potionStack == null || potionStack.itemId != Item.Potion.id)
            {
                continue;
            }

            int oldDamage = potionStack.getDamage();
            int newDamage = GetPotionResult(oldDamage, ingredient);
            if (!ItemPotion.IsSplash(oldDamage) && ItemPotion.IsSplash(newDamage))
            {
                return true;
            }

            List<PotionEffect>? oldEffects = ((ItemPotion)Item.Potion).GetEffects(oldDamage);
            List<PotionEffect>? newEffects = ((ItemPotion)Item.Potion).GetEffects(newDamage);
            if ((!EffectsEqual(oldEffects, newEffects) || oldEffects == null && newEffects != null) && oldDamage != newDamage)
            {
                return true;
            }
        }

        return false;
    }

    private void BrewPotions()
    {
        if (!CanBrew())
        {
            return;
        }

        ItemStack ingredient = _inventory[3]!;

        for (int i = 0; i < 3; ++i)
        {
            ItemStack? potionStack = _inventory[i];
            if (potionStack == null || potionStack.itemId != Item.Potion.id)
            {
                continue;
            }

            int oldDamage = potionStack.getDamage();
            int newDamage = GetPotionResult(oldDamage, ingredient);
            List<PotionEffect>? oldEffects = ((ItemPotion)Item.Potion).GetEffects(oldDamage);
            List<PotionEffect>? newEffects = ((ItemPotion)Item.Potion).GetEffects(newDamage);

            if ((!ItemPotion.IsSplash(oldDamage) || !ItemPotion.IsSplash(newDamage)) && EffectsEqual(oldEffects, newEffects))
            {
                if (!ItemPotion.IsSplash(oldDamage) && ItemPotion.IsSplash(newDamage))
                {
                    potionStack.setDamage(newDamage);
                }
            }
            else if (oldDamage != newDamage)
            {
                potionStack.setDamage(newDamage);
            }
        }

        if (ingredient.getItem().hasContainerItem())
        {
            _inventory[3] = new ItemStack(ingredient.getItem().getContainerItem());
        }
        else
        {
            --ingredient.count;
            if (ingredient.count <= 0)
            {
                _inventory[3] = null;
            }
        }
    }

    private static bool EffectsEqual(List<PotionEffect>? a, List<PotionEffect>? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a == null || b == null || a.Count != b.Count)
        {
            return false;
        }

        for (int i = 0; i < a.Count; ++i)
        {
            if (!a[i].Equals(b[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static int GetPotionResult(int potionDamage, ItemStack ingredient)
    {
        string? effect = ingredient.getItem().getPotionEffect();
        return ingredient.getItem().isPotionIngredient() && effect != null ? PotionHelper.ApplyIngredient(potionDamage, effect) : potionDamage;
    }
}
