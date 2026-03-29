using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;

namespace BetaSharp.Screens.Slots;

internal sealed class BrewingStandPotionSlot : Slot
{
    private readonly EntityPlayer _player;

    public BrewingStandPotionSlot(EntityPlayer player, IInventory inventory, int slotIndex, int x, int y) : base(inventory, slotIndex, x, y)
    {
        _player = player;
    }

    public override bool canInsert(ItemStack stack)
    {
        return stack.itemId == Item.Potion.id || stack.itemId == Item.GlassBottle.id;
    }

    public override int getMaxItemCount()
    {
        return 1;
    }

    public override void onTakeItem(ItemStack stack)
    {
        base.onTakeItem(stack);
    }
}
