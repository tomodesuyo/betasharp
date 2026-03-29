using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Trading;

namespace BetaSharp.Screens.Slots;

public sealed class MerchantResultSlot : Slot
{
    private readonly EntityPlayer _player;
    private readonly IMerchant _merchant;
    private readonly InventoryMerchant _merchantInventory;
    private readonly MerchantScreenHandler _handler;

    public MerchantResultSlot(EntityPlayer player, IMerchant merchant, InventoryMerchant inventory, int slotIndex, int x, int y, MerchantScreenHandler handler)
        : base(inventory, slotIndex, x, y)
    {
        _player = player;
        _merchant = merchant;
        _merchantInventory = inventory;
        _handler = handler;
    }

    public override bool canInsert(ItemStack stack)
    {
        return false;
    }

    public override void onTakeItem(ItemStack stack)
    {
        MerchantRecipe? recipe = _merchantInventory.ActiveRecipe;
        if (recipe != null && recipe.Matches(_merchantInventory.getStack(0), _merchantInventory.getStack(1)))
        {
            ConsumeInput(0, recipe.BuyItem1.count);

            if (recipe.BuyItem2 != null)
            {
                ConsumeInput(1, recipe.BuyItem2.count);
            }

            _merchant.UseRecipe(recipe);
            _handler.OnRecipeUsed();
        }

        stack.onCraft(_player.world, _player);
        base.onTakeItem(stack);
    }

    private void ConsumeInput(int slotIndex, int amount)
    {
        ItemStack? input = _merchantInventory.getStack(slotIndex);
        if (input == null)
        {
            return;
        }

        input.count -= amount;
        _merchantInventory.setStack(slotIndex, input.count > 0 ? input : null);
    }
}
