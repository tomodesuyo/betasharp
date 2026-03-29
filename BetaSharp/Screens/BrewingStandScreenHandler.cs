using BetaSharp.Blocks.Entities;
using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Screens.Slots;

namespace BetaSharp.Screens;

public sealed class BrewingStandScreenHandler : ScreenHandler
{
    private readonly BlockEntityBrewingStand _brewingStand;
    private int _brewTime;

    public BrewingStandScreenHandler(InventoryPlayer playerInventory, BlockEntityBrewingStand brewingStand)
    {
        _brewingStand = brewingStand;

        AddSlot(new BrewingStandPotionSlot(playerInventory.player, brewingStand, 0, 56, 46));
        AddSlot(new BrewingStandPotionSlot(playerInventory.player, brewingStand, 1, 79, 53));
        AddSlot(new BrewingStandPotionSlot(playerInventory.player, brewingStand, 2, 102, 46));
        AddSlot(new BrewingStandIngredientSlot(brewingStand, 3, 79, 17));

        for (int row = 0; row < 3; ++row)
        {
            for (int col = 0; col < 9; ++col)
            {
                AddSlot(new Slot(playerInventory, col + row * 9 + 9, 8 + col * 18, 84 + row * 18));
            }
        }

        for (int i = 0; i < 9; ++i)
        {
            AddSlot(new Slot(playerInventory, i, 8 + i * 18, 142));
        }
    }

    public override void AddListener(ScreenHandlerListener listener)
    {
        base.AddListener(listener);
        listener.onPropertyUpdate(this, 0, _brewingStand.brewTime);
    }

    public override void SendContentUpdates()
    {
        base.SendContentUpdates();

        for (int i = 0; i < Listeners.Count; ++i)
        {
            if (_brewTime != _brewingStand.brewTime)
            {
                Listeners[i].onPropertyUpdate(this, 0, _brewingStand.brewTime);
            }
        }

        _brewTime = _brewingStand.brewTime;
    }

    public override void setProperty(int id, int value)
    {
        if (id == 0)
        {
            _brewingStand.brewTime = value;
        }
    }

    public override bool canUse(EntityPlayer player)
    {
        return _brewingStand.canPlayerUse(player);
    }

    public override ItemStack? quickMove(int slotNumber)
    {
        ItemStack? movedStack = null;
        Slot slot = Slots[slotNumber];
        if (slot != null && slot.hasStack())
        {
            ItemStack source = slot.getStack();
            movedStack = source.copy();

            if (slotNumber <= 3)
            {
                insertItem(source, 4, 40, true);
            }
            else if (source.getItem().isPotionIngredient())
            {
                insertItem(source, 3, 4, false);
            }
            else if (source.itemId == Item.Potion.id || source.itemId == Item.GlassBottle.id)
            {
                insertItem(source, 0, 3, false);
            }
            else if (slotNumber >= 4 && slotNumber < 31)
            {
                insertItem(source, 31, 40, false);
            }
            else if (slotNumber >= 31 && slotNumber < 40)
            {
                insertItem(source, 4, 31, false);
            }
            else
            {
                insertItem(source, 4, 40, false);
            }

            if (source.count == 0)
            {
                slot.setStack(null);
            }
            else
            {
                slot.markDirty();
            }

            if (source.count == movedStack.count)
            {
                return null;
            }

            slot.onTakeItem(source);
        }

        return movedStack;
    }
}
