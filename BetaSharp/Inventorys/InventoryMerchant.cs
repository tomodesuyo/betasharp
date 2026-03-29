using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.Screens;
using BetaSharp.Trading;

namespace BetaSharp.Inventorys;

public sealed class InventoryMerchant : IInventory
{
    private readonly ItemStack?[] _stacks = new ItemStack[3];
    private readonly ScreenHandler _eventHandler;
    private readonly IMerchant _merchant;
    private MerchantRecipeList _offers = new();
    private int _currentRecipeIndex = -1;
    private MerchantRecipe? _activeRecipe;

    public InventoryMerchant(ScreenHandler eventHandler, IMerchant merchant)
    {
        _eventHandler = eventHandler;
        _merchant = merchant;
    }

    public MerchantRecipe? ActiveRecipe => _activeRecipe;

    public void SetOffers(MerchantRecipeList offers, int recipeIndex)
    {
        _offers = offers;
        _currentRecipeIndex = recipeIndex;
        UpdateOffers();
    }

    public void SetCurrentRecipeIndex(int recipeIndex)
    {
        _currentRecipeIndex = recipeIndex;
        UpdateOffers();
    }

    public void UpdateOffers()
    {
        _activeRecipe = _offers.FindMatch(_stacks[0], _stacks[1], _currentRecipeIndex);
        _stacks[2] = _activeRecipe != null && !_activeRecipe.IsDisabled ? _activeRecipe.SellItem.copy() : null;
    }

    public int size()
    {
        return _stacks.Length;
    }

    public ItemStack getStack(int slotIndex)
    {
        return _stacks[slotIndex];
    }

    public ItemStack? removeStack(int slotIndex, int amount)
    {
        ItemStack? stack = _stacks[slotIndex];
        if (stack == null)
        {
            return null;
        }

        ItemStack removedStack;
        if (stack.count <= amount)
        {
            removedStack = stack;
            _stacks[slotIndex] = null;
        }
        else
        {
            removedStack = stack.split(amount);
            if (stack.count == 0)
            {
                _stacks[slotIndex] = null;
            }
        }

        markDirty();
        return removedStack;
    }

    public void setStack(int slotIndex, ItemStack? itemStack)
    {
        _stacks[slotIndex] = itemStack;
        if (itemStack != null && itemStack.count > getMaxCountPerStack())
        {
            itemStack.count = getMaxCountPerStack();
        }

        markDirty();
    }

    public string getName()
    {
        return "Merchant";
    }

    public int getMaxCountPerStack()
    {
        return 64;
    }

    public void markDirty()
    {
        UpdateOffers();
        _eventHandler.onSlotUpdate(this);
    }

    public bool canPlayerUse(EntityPlayer entityPlayer)
    {
        EntityPlayer? customer = _merchant.GetCustomer();
        return customer == null || customer == entityPlayer;
    }
}
