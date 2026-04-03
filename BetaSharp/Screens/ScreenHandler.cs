using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Screens.Slots;

namespace BetaSharp.Screens;

public abstract class ScreenHandler
{
    protected List<ScreenHandlerListener> Listeners { get; private set; } = [];
    private short _revision;
    private HashSet<EntityPlayer> _players = new HashSet<EntityPlayer>();
    private int _dragMode = -1;
    private int _dragEvent;
    private readonly HashSet<Slot> _dragSlots = [];



    public List<ItemStack?> TrackedStacks { get; private set; } = [];
    public List<Slot> Slots { get; private set; } = [];
    public int SyncId { get; set; } = 0;


    protected void AddSlot(Slot slot)
    {
        slot.id = Slots.Count;
        Slots.Add(slot);
        TrackedStacks.Add(null);
    }

    public virtual void AddListener(ScreenHandlerListener listener)
    {
        if (Listeners.Contains(listener))
        {
            throw new ArgumentException("Listener already listening", nameof(listener));
        }
        else
        {
            Listeners.Add(listener);
            listener.onContentsUpdate(this, GetStacks());
            SendContentUpdates();
        }
    }

    public List<ItemStack> GetStacks()
    {
        List<ItemStack> stacks = new List<ItemStack>();

        for (int slotIndex = 0; slotIndex < Slots.Count; slotIndex++)
        {
            stacks.Add(Slots[slotIndex].getStack());
        }
        return stacks;
    }

    public virtual void SendContentUpdates()
    {
        for (int slotIndex = 0; slotIndex < Slots.Count; ++slotIndex)
        {
            ItemStack slotStack = Slots[slotIndex].getStack();
            ItemStack? trackedStack = TrackedStacks[slotIndex];
            if (!ItemStack.areEqual(trackedStack, slotStack))
            {
                trackedStack = slotStack is null ? null : slotStack.copy();
                TrackedStacks[slotIndex] = trackedStack;

                for (int listenerIndex = 0; listenerIndex < Listeners.Count; ++listenerIndex)
                {
                    Listeners[listenerIndex].onSlotUpdate(this, slotIndex, trackedStack);
                }
            }
        }
    }

    public Slot? GetSlot(IInventory inventory, int index)
    {
        for (int slotIndex = 0; slotIndex < Slots.Count; slotIndex++)
        {
            Slot slot = Slots[slotIndex];
            if (slot.Equals(inventory, index))
            {
                return slot;
            }
        }
        return null;
    }

    public Slot GetSlot(int index)
    {
        return Slots[index];
    }

    public virtual ItemStack? quickMove(int index)
    {
        if (index < 0 || index > Slots.Count)
            return null;

        return Slots[index].getStack();
    }

    public ItemStack? onSlotClick(int index, int button, bool shift, EntityPlayer player)
    {
        return onSlotClick(index, button, shift ? ScreenHandlerClickMode.QuickMove : ScreenHandlerClickMode.Pickup, player);
    }

    public virtual ItemStack? onSlotClick(int index, int button, int mode, EntityPlayer player)
    {
        ItemStack? returnStack = null;
        InventoryPlayer playerInventory = player.inventory;

        if (mode == ScreenHandlerClickMode.Drag)
        {
            int previousEvent = _dragEvent;
            _dragEvent = GetDragEvent(button);

            if ((previousEvent != 1 || _dragEvent != 2) && previousEvent != _dragEvent)
            {
                ResetDrag();
            }
            else if (playerInventory.getCursorStack() == null)
            {
                ResetDrag();
            }
            else if (_dragEvent == 0)
            {
                _dragMode = ExtractDragMode(button);
                if (IsValidDragMode(_dragMode, player))
                {
                    _dragEvent = 1;
                    _dragSlots.Clear();
                }
                else
                {
                    ResetDrag();
                }
            }
            else if (_dragEvent == 1)
            {
                if (index >= 0 && index < Slots.Count)
                {
                    Slot slot = Slots[index];
                    ItemStack cursorStack = playerInventory.getCursorStack()!;
                    if (CanAddItemToSlot(slot, cursorStack, true)
                        && slot.canInsert(cursorStack)
                        && cursorStack.count > _dragSlots.Count
                        && canDragIntoSlot(slot))
                    {
                        _dragSlots.Add(slot);
                    }
                }
            }
            else if (_dragEvent == 2)
            {
                if (_dragSlots.Count > 0)
                {
                    ItemStack dragStack = playerInventory.getCursorStack()!.copy();
                    int remainingCount = playerInventory.getCursorStack()!.count;

                    foreach (Slot slot in _dragSlots)
                    {
                        if (!CanAddItemToSlot(slot, playerInventory.getCursorStack()!, true)
                            || !slot.canInsert(playerInventory.getCursorStack()!)
                            || playerInventory.getCursorStack()!.count < _dragSlots.Count
                            || !canDragIntoSlot(slot))
                        {
                            continue;
                        }

                        ItemStack placedStack = dragStack.copy();
                        int existingCount = slot.hasStack() ? slot.getStack().count : 0;
                        ComputeStackSize(_dragSlots, _dragMode, placedStack, existingCount);

                        if (placedStack.count > placedStack.getMaxCount())
                        {
                            placedStack.count = placedStack.getMaxCount();
                        }

                        if (placedStack.count > slot.getMaxItemCount())
                        {
                            placedStack.count = slot.getMaxItemCount();
                        }

                        remainingCount -= placedStack.count - existingCount;
                        slot.setStack(placedStack);
                    }

                    dragStack.count = remainingCount;
                    playerInventory.setItemStack(dragStack.count > 0 ? dragStack : null);
                }

                ResetDrag();
            }
            else
            {
                ResetDrag();
            }

            return null;
        }

        if (_dragEvent != 0)
        {
            ResetDrag();
        }

        if (mode == ScreenHandlerClickMode.QuickMove)
        {
            if (index < 0 || index >= Slots.Count)
            {
                return null;
            }

            ItemStack? itemStack = quickMove(index);
            if (itemStack is not null)
            {
                int itemStackSize = itemStack.count;
                returnStack = itemStack.copy();
                Slot slot = Slots[index];
                if (slot.getStack() is not null)
                {
                    int slotItemStackSize = slot.getStack().count;
                    if (slotItemStackSize < itemStackSize)
                    {
                        onSlotClick(index, button, ScreenHandlerClickMode.QuickMove, player);
                    }
                }
            }

            return returnStack;
        }

        if (mode == ScreenHandlerClickMode.Pickup && (button == 0 || button == 1))
        {
            if (index == -999)
            {
                if (playerInventory.getCursorStack() is not null)
                {
                    if (button == 0)
                    {
                        player.dropItem(playerInventory.getCursorStack());
                        playerInventory.setItemStack(null);
                    }
                    else if (button == 1)
                    {
                        player.dropItem(playerInventory.getCursorStack().split(1));
                        if (playerInventory.getCursorStack().count == 0)
                        {
                            playerInventory.setItemStack(null);
                        }
                    }
                }

                return null;
            }

            if (index < 0 || index >= Slots.Count)
            {
                return null;
            }

            Slot slot = Slots[index];
            slot.markDirty();
            ItemStack? slotStack = slot.getStack();
            ItemStack? cursorStack = playerInventory.getCursorStack();
            if (slotStack is not null)
            {
                returnStack = slotStack.copy();
            }

            if (slotStack is null)
            {
                if (cursorStack is not null && slot.canInsert(cursorStack))
                {
                    int slotItemStackSize = button == 0 ? cursorStack.count : 1;
                    if (slotItemStackSize > slot.getMaxItemCount())
                    {
                        slotItemStackSize = slot.getMaxItemCount();
                    }

                    slot.setStack(cursorStack.split(slotItemStackSize));
                    if (cursorStack.count == 0)
                    {
                        playerInventory.setItemStack(null);
                    }
                }
            }
            else if (cursorStack is null)
            {
                if (!slot.canTake(player))
                {
                    return returnStack;
                }

                int slotItemStackSize = button == 0 ? slotStack.count : (slotStack.count + 1) / 2;
                ItemStack takenStack = slot.takeStack(slotItemStackSize);
                playerInventory.setItemStack(takenStack);
                if (slotStack.count == 0)
                {
                    slot.setStack(null);
                }

                slot.onTakeItem(playerInventory.getCursorStack());
            }
            else if (slot.canInsert(cursorStack))
            {
                if (slotStack.itemId != cursorStack.itemId || slotStack.getHasSubtypes() && slotStack.getDamage() != cursorStack.getDamage())
                {
                    if (cursorStack.count <= slot.getMaxItemCount())
                    {
                        slot.setStack(cursorStack);
                        playerInventory.setItemStack(slotStack);
                    }
                }
                else
                {
                    int slotItemStackSize = button == 0 ? cursorStack.count : 1;
                    if (slotItemStackSize > slot.getMaxItemCount() - slotStack.count)
                    {
                        slotItemStackSize = slot.getMaxItemCount() - slotStack.count;
                    }

                    if (slotItemStackSize > cursorStack.getMaxCount() - slotStack.count)
                    {
                        slotItemStackSize = cursorStack.getMaxCount() - slotStack.count;
                    }

                    cursorStack.split(slotItemStackSize);
                    if (cursorStack.count == 0)
                    {
                        playerInventory.setItemStack(null);
                    }

                    slotStack.count += slotItemStackSize;
                }
            }
            else if (slotStack.itemId == cursorStack.itemId && cursorStack.getMaxCount() > 1 && (!slotStack.getHasSubtypes() || slotStack.getDamage() == cursorStack.getDamage()))
            {
                int slotItemStackSize = slotStack.count;
                if (slotItemStackSize > 0 && slotItemStackSize + cursorStack.count <= cursorStack.getMaxCount())
                {
                    cursorStack.count += slotItemStackSize;
                    slotStack.split(slotItemStackSize);
                    if (slotStack.count == 0)
                    {
                        slot.setStack(null);
                    }

                    slot.onTakeItem(playerInventory.getCursorStack());
                }
            }

            return returnStack;
        }

        if (mode == ScreenHandlerClickMode.Swap && button >= 0 && button < 9 && index >= 0 && index < Slots.Count)
        {
            Slot slot = Slots[index];
            ItemStack? hotbarStack = playerInventory.getStack(button);
            if (!slot.canTake(player))
            {
                return null;
            }

            if (slot.hasStack())
            {
                ItemStack slotStack = slot.getStack();
                returnStack = slotStack.copy();
                if (hotbarStack == null || slot.canInsert(hotbarStack))
                {
                    playerInventory.setStack(button, slotStack);
                    slot.setStack(hotbarStack);
                    slot.onTakeItem(slotStack);
                }
            }
            else if (hotbarStack != null && slot.canInsert(hotbarStack))
            {
                slot.setStack(hotbarStack);
                playerInventory.setStack(button, null);
            }

            return returnStack;
        }

        if (mode == ScreenHandlerClickMode.Clone && player.capabilities.isCreativeMode && playerInventory.getCursorStack() == null && index >= 0 && index < Slots.Count)
        {
            Slot slot = Slots[index];
            if (slot.hasStack())
            {
                ItemStack clonedStack = slot.getStack().copy();
                clonedStack.count = clonedStack.getMaxCount();
                playerInventory.setItemStack(clonedStack);
                return slot.getStack().copy();
            }

            return null;
        }

        if (mode == ScreenHandlerClickMode.Throw && playerInventory.getCursorStack() == null && index >= 0 && index < Slots.Count)
        {
            Slot slot = Slots[index];
            if (slot.hasStack() && slot.canTake(player))
            {
                ItemStack droppedStack = slot.takeStack(button == 0 ? 1 : slot.getStack().count);
                if (droppedStack != null)
                {
                    returnStack = slot.getStack() != null ? slot.getStack().copy() : droppedStack.copy();
                    slot.onTakeItem(droppedStack);
                    player.dropItem(droppedStack, true);
                    if (slot.getStack() != null && slot.getStack().count == 0)
                    {
                        slot.setStack(null);
                    }
                }
            }
        }

        if (mode == ScreenHandlerClickMode.PickupAll && index >= 0 && index < Slots.Count)
        {
            Slot clickedSlot = Slots[index];
            ItemStack? cursorStack = playerInventory.getCursorStack();
            if (cursorStack == null || !clickedSlot.canTake(player))
            {
                return null;
            }

            if (!clickedSlot.hasStack() || !AreStacksCompatible(cursorStack, clickedSlot.getStack()))
            {
                return null;
            }

            returnStack = cursorStack.copy();
            IInventory sourceInventory = clickedSlot.Inventory;
            int maxCount = cursorStack.getMaxCount();

            for (int pass = 0; pass < 2 && cursorStack.count < maxCount; ++pass)
            {
                for (int slotIndex = 0; slotIndex < Slots.Count && cursorStack.count < maxCount; ++slotIndex)
                {
                    Slot slot = Slots[slotIndex];
                    if (slot.Inventory != sourceInventory || !slot.canTake(player) || !slot.hasStack())
                    {
                        continue;
                    }

                    ItemStack slotStack = slot.getStack();
                    if (!AreStacksCompatible(cursorStack, slotStack))
                    {
                        continue;
                    }

                    bool takeWholeStack = pass == 0;
                    if (!takeWholeStack && slotStack.count == slotStack.getMaxCount())
                    {
                        continue;
                    }

                    int availableSpace = maxCount - cursorStack.count;
                    if (availableSpace <= 0)
                    {
                        break;
                    }

                    int amountToTake = Math.Min(availableSpace, slotStack.count);
                    if (amountToTake <= 0)
                    {
                        continue;
                    }

                    ItemStack takenStack = slot.takeStack(amountToTake);
                    if (takenStack == null)
                    {
                        continue;
                    }

                    cursorStack.count += takenStack.count;
                    if (slot.getStack() != null && slot.getStack().count == 0)
                    {
                        slot.setStack(null);
                    }

                    slot.onTakeItem(takenStack);
                }
            }
        }

        return returnStack;
    }

    public virtual void onClosed(EntityPlayer player)
    {
        InventoryPlayer playerInventory = player.inventory;
        if (playerInventory.getCursorStack() is not null)
        {
            player.dropItem(playerInventory.getCursorStack());
            playerInventory.setItemStack(null);
        }

    }

    public virtual void onSlotUpdate(IInventory inventory)
    {
        SendContentUpdates();
    }

    public void setStackInSlot(int index, ItemStack stack)
    {
        GetSlot(index).setStack(stack);
    }

    public void updateSlotStacks(ItemStack[] stacks)
    {
        for (int index = 0; index < stacks.Length; ++index)
        {
            GetSlot(index).setStack(stacks[index]);
        }

    }

    public virtual void setProperty(int id, int value)
    {
    }

    public short nextRevision(InventoryPlayer inventory)
    {
        ++_revision;
        return _revision;
    }

    public void onAcknowledgementAccepted(short actionType)
    {
    }

    public void onAcknowledgementDenied(short actionType)
    {
    }

    public bool canOpen(EntityPlayer player)
    {
        return !_players.Contains(player);
    }

    public void updatePlayerList(EntityPlayer player, bool remove)
    {
        if (remove)
        {
            _players.Remove(player);
        }
        else
        {
            _players.Add(player);
        }
    }

    public abstract bool canUse(EntityPlayer player);

    public virtual bool canMergeSlot(ItemStack? stack, Slot slot)
    {
        return true;
    }

    public virtual bool canDragIntoSlot(Slot slot)
    {
        return true;
    }

    protected void insertItem(ItemStack stack, int start, int end, bool fromLast)
    {
        int slotIndex = start;
        if (fromLast)
        {
            slotIndex = end - 1;
        }

        Slot slotToInsertStack;
        ItemStack itemStackToInsert;
        if (stack.isStackable())
        {
            while (stack.count > 0 && (!fromLast && slotIndex < end || fromLast && slotIndex >= start))
            {
                slotToInsertStack = Slots[slotIndex];
                itemStackToInsert = slotToInsertStack.getStack();
                if (itemStackToInsert is not null
                    && slotToInsertStack.canInsert(stack)
                    && itemStackToInsert.itemId == stack.itemId
                    && (!stack.getHasSubtypes() || stack.getDamage() == itemStackToInsert.getDamage()))
                {
                    int newitemStackSize = itemStackToInsert.count + stack.count;
                    if (newitemStackSize <= stack.getMaxCount())
                    {
                        stack.count = 0;
                        itemStackToInsert.count = newitemStackSize;
                        slotToInsertStack.markDirty();
                    }
                    else if (itemStackToInsert.count < stack.getMaxCount())
                    {
                        stack.count -= stack.getMaxCount() - itemStackToInsert.count;
                        itemStackToInsert.count = stack.getMaxCount();
                        slotToInsertStack.markDirty();
                    }
                }

                if (fromLast)
                {
                    --slotIndex;
                }
                else
                {
                    ++slotIndex;
                }
            }
        }

        if (stack.count > 0)
        {
            if (fromLast)
            {
                slotIndex = end - 1;
            }
            else
            {
                slotIndex = start;
            }

            while (!fromLast && slotIndex < end || fromLast && slotIndex >= start)
            {
                slotToInsertStack = Slots[slotIndex];
                itemStackToInsert = slotToInsertStack.getStack();
                if (itemStackToInsert is null && slotToInsertStack.canInsert(stack))
                {
                    slotToInsertStack.setStack(stack.copy());
                    slotToInsertStack.markDirty();
                    stack.count = 0;
                    break;
                }

                if (fromLast)
                {
                    --slotIndex;
                }
                else
                {
                    ++slotIndex;
                }
            }
        }
    }

    public static int ExtractDragMode(int button)
    {
        return button >> 2 & 3;
    }

    public static int GetDragEvent(int button)
    {
        return button & 3;
    }

    public static int PackDragData(int dragEvent, int dragMode)
    {
        return dragEvent & 3 | (dragMode & 3) << 2;
    }

    public static bool IsValidDragMode(int dragMode, EntityPlayer player)
    {
        return dragMode == 0
            || dragMode == 1
            || dragMode == 2 && player.capabilities.IsCreativeMode;
    }

    protected void ResetDrag()
    {
        _dragEvent = 0;
        _dragSlots.Clear();
    }

    public static bool CanAddItemToSlot(Slot slot, ItemStack stack, bool ignoreStackSize)
    {
        if (!slot.hasStack())
        {
            return true;
        }

        ItemStack slotStack = slot.getStack();
        return AreStacksCompatible(stack, slotStack)
               && slotStack.count + (ignoreStackSize ? 0 : stack.count) <= stack.getMaxCount();
    }

    public static void ComputeStackSize(ICollection<Slot> dragSlots, int dragMode, ItemStack stack, int existingCount)
    {
        switch (dragMode)
        {
            case 0:
                stack.count = stack.count / dragSlots.Count;
                break;
            case 1:
                stack.count = 1;
                break;
            case 2:
                stack.count = stack.getMaxCount();
                break;
        }

        stack.count += existingCount;
    }

    private static bool AreStacksCompatible(ItemStack a, ItemStack b)
    {
        return a.itemId == b.itemId
               && (!a.getHasSubtypes() || a.getDamage() == b.getDamage());
    }
}
