using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;

namespace BetaSharp.Screens.Slots;

internal class CreativeSelectionSlot(IInventory inventory, int slotIndex, int x, int y) : Slot(inventory, slotIndex, x, y)
{
    public override bool canInsert(ItemStack stack) => false;

    public override bool canTake(EntityPlayer player) => false;
}
