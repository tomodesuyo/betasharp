using BetaSharp.Inventorys;
using BetaSharp.Items;

namespace BetaSharp.Screens.Slots;

internal sealed class BrewingStandIngredientSlot : Slot
{
    public BrewingStandIngredientSlot(IInventory inventory, int slotIndex, int x, int y) : base(inventory, slotIndex, x, y)
    {
    }

    public override bool canInsert(ItemStack stack)
    {
        return stack.getItem().isPotionIngredient();
    }
}
