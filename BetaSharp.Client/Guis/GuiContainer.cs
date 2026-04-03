using BetaSharp.Client.Input;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Items;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Screens;
using BetaSharp.Screens.Slots;

namespace BetaSharp.Client.Guis;

public abstract class GuiContainer : GuiScreen
{
    private static readonly ItemRenderer s_itemRenderer = new();
    private readonly HashSet<Slot> _dragSplittingSlots = [];
    protected int _xSize = 176;
    protected int _ySize = 166;
    public ScreenHandler InventorySlots;
    protected Slot? _hoveredSlot;
    private Slot? _mouseTweaksSlot;
    private bool _dragSplitting;
    private int _dragSplittingButton;
    private int _dragSplittingLimit;
    private long _lastClickTime;
    private Slot? _lastClickSlot;
    private int _lastClickButton;
    private bool _doubleClick;

    public override bool PausesGame => false;
    protected int GuiLeft => (Width - _xSize) / 2;
    protected int GuiTop => (Height - _ySize) / 2;

    public GuiContainer(ScreenHandler inventorySlots)
    {
        InventorySlots = inventorySlots;
    }

    public override void InitGui()
    {
        base.InitGui();
        Game.player.currentScreenHandler = InventorySlots;
    }

    public override void Render(int mouseX, int mouseY, float partialTicks)
    {
        DrawDefaultBackground();

        int guiLeft = GuiLeft;
        int guiTop = GuiTop;

        DrawGuiContainerBackgroundLayer(partialTicks);

        GLManager.GL.PushMatrix();
        GLManager.GL.Rotate(120.0F, 1.0F, 0.0F, 0.0F);
        Lighting.turnOn();
        GLManager.GL.PopMatrix();

        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(guiLeft, guiTop, 0.0F);
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        GLManager.GL.Enable(GLEnum.RescaleNormal);

        _hoveredSlot = null;


        for (int i = 0; i < InventorySlots.Slots.Count; ++i)
        {
            Slot slot = InventorySlots.Slots[i];
            DrawSlotInventory(slot);
            if (GetIsMouseOverSlot(slot, mouseX, mouseY))
            {
                _hoveredSlot = slot;

                GLManager.GL.Disable(GLEnum.Lighting);
                GLManager.GL.Disable(GLEnum.DepthTest);
                int sx = slot.xDisplayPosition;
                int sy = slot.yDisplayPosition;
                DrawGradientRect(sx, sy, sx + 16, sy + 16, Color.BackgroundWhiteAlpha, Color.BackgroundWhiteAlpha);
                GLManager.GL.Enable(GLEnum.Lighting);
                GLManager.GL.Enable(GLEnum.DepthTest);
            }
        }

        InventoryPlayer playerInv = Game.player.inventory;

        GLManager.GL.Disable(GLEnum.RescaleNormal);
        Lighting.turnOff();
        GLManager.GL.Disable(GLEnum.Lighting);
        GLManager.GL.Disable(GLEnum.DepthTest);
        DrawGuiContainerForegroundLayer();

        if (playerInv?.getCursorStack() == null && _hoveredSlot != null && _hoveredSlot.hasStack())
        {
            string itemName = TranslationStorage.Instance.TranslateNamedKey(_hoveredSlot.getStack().getItemName()).Trim();
            if (itemName.Length > 0)
            {
                int tipX = mouseX - guiLeft + 12;
                int tipY = mouseY - guiTop - 12;

                List<string> tooltipLines = [itemName];
                if (Game.options.AdvancedItemTooltips)
                {
                    tooltipLines.Add(_hoveredSlot.getStack().getDamage() != 0 ? $"{_hoveredSlot.getStack().itemId}:{_hoveredSlot.getStack().getDamage()}" : $"{_hoveredSlot.getStack().itemId}");
                }

                int maxTextWidth = tooltipLines.Select(line => FontRenderer.GetStringWidth(line)).Prepend(0).Max();

                int boxHeight = 8;
                if (tooltipLines.Count > 1)
                {
                    boxHeight += 2 + (tooltipLines.Count - 1) * 10;
                }

                DrawGradientRect(tipX - 3, tipY - 3, tipX + maxTextWidth + 3, tipY + boxHeight + 3, Color.BlackAlphaC0, Color.BlackAlphaC0);

                for (int i = 0; i < tooltipLines.Count; i++)
                {
                    FontRenderer.DrawStringWithShadow(tooltipLines[i], tipX, tipY, i == 0 ? Color.White : new Color((byte)170, (byte)170, (byte)170));
                    if (i == 0) tipY += 2;
                    tipY += 10;
                }
            }
        }

        if (playerInv.getCursorStack() != null)
        {
            GLManager.GL.Enable(GLEnum.RescaleNormal);
            GLManager.GL.PushMatrix();
            GLManager.GL.Rotate(120.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(-90.0F, 0.0F, 1.0F, 0.0F);
            Lighting.turnOn();
            GLManager.GL.PopMatrix();
            GLManager.GL.Enable(GLEnum.Lighting);
            GLManager.GL.Enable(GLEnum.DepthTest);

            GLManager.GL.Translate(0.0F, 0.0F, 32.0F);
            s_itemRenderer.renderItemIntoGUI(FontRenderer, Game.textureManager, playerInv.getCursorStack(), mouseX - guiLeft - 8, mouseY - guiTop - 8);
            s_itemRenderer.renderItemOverlayIntoGUI(FontRenderer, Game.textureManager, playerInv.getCursorStack(), mouseX - guiLeft - 8, mouseY - guiTop - 8);

            Lighting.turnOff();
            GLManager.GL.Disable(GLEnum.Lighting);
            GLManager.GL.Disable(GLEnum.DepthTest);
            GLManager.GL.Disable(GLEnum.RescaleNormal);
        }

        GLManager.GL.PopMatrix();
        base.Render(mouseX, mouseY, partialTicks);

        GLManager.GL.Enable(GLEnum.Lighting);
        GLManager.GL.Enable(GLEnum.DepthTest);
    }

    protected virtual void DrawGuiContainerForegroundLayer()
    {
    }

    protected abstract void DrawGuiContainerBackgroundLayer(float partialTicks);

    private void DrawSlotInventory(Slot slot)
    {
        int x = slot.xDisplayPosition;
        int y = slot.yDisplayPosition;
        ItemStack item = slot.getStack();
        if (item == null)
        {
            int iconIdx = slot.getBackgroundTextureId();
            if (iconIdx >= 0)
            {
                GLManager.GL.Disable(GLEnum.Lighting);
                Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/gui/items.png"));
                DrawTexturedModalRect(x, y, iconIdx % 16 * 16, iconIdx / 16 * 16, 16, 16);
                GLManager.GL.Enable(GLEnum.Lighting);
                return;
            }
        }

        s_itemRenderer.renderItemIntoGUI(FontRenderer, Game.textureManager, item, x, y);
        s_itemRenderer.renderItemOverlayIntoGUI(FontRenderer, Game.textureManager, item, x, y);
    }

    protected Slot? GetSlotAtPosition(int mouseX, int mouseY)
    {
        for (int i = 0; i < InventorySlots.Slots.Count; ++i)
        {
            Slot slot = InventorySlots.Slots[i];
            if (GetIsMouseOverSlot(slot, mouseX, mouseY))
            {
                return slot;
            }
        }

        return null;
    }

    protected bool GetIsMouseOverSlot(Slot slot, int mouseX, int mouseY)
    {
        mouseX -= GuiLeft;
        mouseY -= GuiTop;

        return mouseX >= slot.xDisplayPosition - 1 &&
               mouseX < slot.xDisplayPosition + 16 + 1 &&
               mouseY >= slot.yDisplayPosition - 1 &&
               mouseY < slot.yDisplayPosition + 16 + 1;
    }

    protected override void MouseClicked(int x, int y, int button)
    {
        base.MouseClicked(x, y, button);
        if (button is not (0 or 1 or 2))
        {
            return;
        }

        Slot? slot = GetSlotAtPosition(x, y);
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _doubleClick = _lastClickSlot == slot && now - _lastClickTime < 250L && _lastClickButton == button;

        InventoryPlayer playerInventory = Game.player.inventory;
        ItemStack? cursorStack = playerInventory.getCursorStack();

        if (button is 0 or 1)
        {
            if (_doubleClick
                && button == 0
                && slot != null
                && cursorStack != null
                && slot.hasStack()
                && AreStacksCompatible(cursorStack, slot.getStack()))
            {
                _lastClickSlot = slot;
                _lastClickTime = now;
                _lastClickButton = button;
                return;
            }

            if (cursorStack != null
                && slot != null
                && !IsMouseTweaksIgnored(slot)
                && !IsMouseTweaksResultSlot(slot))
            {
                _dragSplitting = true;
                _dragSplittingButton = button;
                _dragSplittingLimit = button == 0 ? 0 : 1;
                _dragSplittingSlots.Clear();
                TryAddDragSplittingSlot(slot);
            }
            else if (slot != null)
            {
                ClickContainerSlot(slot, button, IsShiftDown());
            }
            else if (IsPointOutsideBounds(x, y))
            {
                ClickContainerSlot(-999, button, ScreenHandlerClickMode.Pickup);
            }
        }
        else if (slot != null)
        {
            ClickContainerSlot(slot, button, IsShiftDown());
        }
        else if (IsPointOutsideBounds(x, y))
        {
            ClickContainerSlot(-999, button, ScreenHandlerClickMode.Clone);
        }

        _lastClickSlot = slot;
        _lastClickTime = now;
        _lastClickButton = button;
    }

    protected override void HandleQuickMove(int x, int y)
    {
        Slot? slot = GetSlotAtPosition(x, y);
        if (slot != null)
        {
            ClickContainerSlot(slot, 0, true);
        }
    }

    public override void HandleMouseInput()
    {
        base.HandleMouseInput();

        GetScaledEventMousePosition(out int mouseX, out int mouseY);
        int wheel = Mouse.getEventDWheel();
        if (wheel != 0)
        {
            HandleContainerMouseWheel(wheel, mouseX, mouseY);
        }

        if (Mouse.getEventButton() == -1)
        {
            if (_dragSplitting)
            {
                HandleDragSplitting(mouseX, mouseY);
            }
            else
            {
                HandleMouseTweaksDrag(mouseX, mouseY);
            }
        }

        _mouseTweaksSlot = GetSlotAtPosition(mouseX, mouseY);
    }

    protected override void MouseMovedOrUp(int x, int y, int button)
    {
        Slot? slot = GetSlotAtPosition(x, y);
        ItemStack? cursorStack = Game.player.inventory.getCursorStack();

        if (_doubleClick
            && button == 0
            && slot != null
            && cursorStack != null
            && slot.hasStack()
            && AreStacksCompatible(cursorStack, slot.getStack()))
        {
            ClickContainerSlot(slot, button, ScreenHandlerClickMode.PickupAll);
            _doubleClick = false;
            _lastClickTime = 0L;
            return;
        }

        if (!_dragSplitting || button != _dragSplittingButton)
        {
            return;
        }

        if (_dragSplittingSlots.Count > 1)
        {
            PerformDragSplitting();
        }
        else if (slot != null)
        {
            ClickContainerSlot(slot, button, ScreenHandlerClickMode.Pickup);
        }
        else if (IsPointOutsideBounds(x, y))
        {
            ClickContainerSlot(-999, button, ScreenHandlerClickMode.Pickup);
        }

        _dragSplitting = false;
        _dragSplittingSlots.Clear();
    }

    protected override void KeyTyped(char eventChar, int eventKey)
    {
        if (eventKey == Keyboard.KEY_ESCAPE || eventKey == Game.options.KeyBindInventory.keyCode)
        {
            Game.player.closeHandledScreen();
            return;
        }

        if (_hoveredSlot == null)
        {
            return;
        }

        if (Game.player.inventory.getCursorStack() == null)
        {
            for (int hotbarSlot = 0; hotbarSlot < 9; ++hotbarSlot)
            {
                if (eventKey == Keyboard.KEY_1 + hotbarSlot)
                {
                    Game.playerController.func_27174_a(InventorySlots.SyncId, _hoveredSlot.id, hotbarSlot, ScreenHandlerClickMode.Swap, Game.player);
                    return;
                }
            }
        }

        if (_hoveredSlot.hasStack() && eventKey == Game.options.KeyBindDrop.keyCode)
        {
            int button = Keyboard.isKeyDown(Keyboard.KEY_LCONTROL) || Keyboard.isKeyDown(Keyboard.KEY_RCONTROL) ? 1 : 0;
            Game.playerController.func_27174_a(InventorySlots.SyncId, _hoveredSlot.id, button, ScreenHandlerClickMode.Throw, Game.player);
        }
    }

    public override void OnGuiClosed()
    {
        _mouseTweaksSlot = null;
        _dragSplitting = false;
        _dragSplittingSlots.Clear();
        _doubleClick = false;
        if (Game.player != null)
        {
            Game.playerController.func_20086_a(InventorySlots.SyncId, Game.player);
        }
    }


    public override void UpdateScreen()
    {
        base.UpdateScreen();
        if (!Game.player.isAlive() || Game.player.dead)
        {
            Game.player.closeHandledScreen();
        }
    }

    public override bool HandleDPadNavigation(int dpadX, int dpadY, ref float cursorX, ref float cursorY)
    {
        int guiLeft = GuiLeft;
        int guiTop = GuiTop;

        ScaledResolution sr = new(Game.options, Game.displayWidth, Game.displayHeight);

        int scaledMouseX = (int)(cursorX * sr.ScaledWidth / Game.displayWidth);
        int scaledMouseY = (int)(cursorY * sr.ScaledHeight / Game.displayHeight);

        Slot? currentSlot = GetSlotAtPosition(scaledMouseX, scaledMouseY);

        float refX, refY;
        if (currentSlot != null)
        {
            refX = currentSlot.xDisplayPosition + 8;
            refY = currentSlot.yDisplayPosition + 8;
        }
        else
        {
            refX = scaledMouseX - guiLeft;
            refY = scaledMouseY - guiTop;
        }

        Slot? bestSlot = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < InventorySlots.Slots.Count; i++)
        {
            Slot slot = InventorySlots.Slots[i];
            if (slot == currentSlot) continue;

            float slotCenterX = slot.xDisplayPosition + 8;
            float slotCenterY = slot.yDisplayPosition + 8;

            float dx = slotCenterX - refX;
            float dy = slotCenterY - refY;

            if (dpadX > 0 && dx <= 0) continue;
            if (dpadX < 0 && dx >= 0) continue;
            if (dpadY > 0 && dy <= 0) continue;
            if (dpadY < 0 && dy >= 0) continue;

            float primaryDist = dpadX != 0 ? Math.Abs(dx) : Math.Abs(dy);
            float crossDist = dpadX != 0 ? Math.Abs(dy) : Math.Abs(dx);
            float score = primaryDist + crossDist * 3f;

            if (score < bestScore)
            {
                bestScore = score;
                bestSlot = slot;
            }
        }

        if (bestSlot != null)
        {
            float targetScaledX = guiLeft + bestSlot.xDisplayPosition + 8;
            float targetScaledY = guiTop + bestSlot.yDisplayPosition + 8;
            cursorX = targetScaledX * Game.displayWidth / sr.ScaledWidth;
            cursorY = targetScaledY * Game.displayHeight / sr.ScaledHeight;
            return true;
        }

        return base.HandleDPadNavigation(dpadX, dpadY, ref cursorX, ref cursorY);
    }

    protected virtual void ClickContainerSlot(Slot slot, int button, bool shiftPressed)
    {
        int clickMode = button == 2
            ? ScreenHandlerClickMode.Clone
            : shiftPressed
                ? ScreenHandlerClickMode.QuickMove
                : ScreenHandlerClickMode.Pickup;
        ClickContainerSlot(slot.id, button, clickMode);
    }

    protected virtual void ClickContainerSlot(Slot slot, int button, int clickMode)
    {
        ClickContainerSlot(slot.id, button, clickMode);
    }

    protected virtual void ClickContainerSlot(int slotId, int button, int clickMode)
    {
        Game.playerController.func_27174_a(InventorySlots.SyncId, slotId, button, clickMode, Game.player);
    }

    protected virtual void HandleContainerMouseWheel(int wheel, int mouseX, int mouseY)
    {
        HandleMouseTweaksWheel(wheel, mouseX, mouseY);
    }

    protected virtual bool IsMouseTweaksIgnored(Slot slot)
    {
        return slot.GetType().Name == "CreativeSelectionSlot";
    }

    protected virtual bool IsMouseTweaksResultSlot(Slot slot)
    {
        return slot.GetType().Name is "CraftingResultSlot" or "FurnaceOutputSlot" or "MerchantResultSlot";
    }

    protected bool HandleMouseTweaksWheel(int wheel, int mouseX, int mouseY)
    {
        Slot? selectedSlot = GetSlotAtPosition(mouseX, mouseY);
        if (wheel == 0 || selectedSlot == null || IsMouseTweaksIgnored(selectedSlot) || IsMouseTweaksResultSlot(selectedSlot))
        {
            return false;
        }

        ItemStack? selectedStack = selectedSlot.getStack();
        if (selectedStack == null)
        {
            return false;
        }

        int itemsToMove = Math.Abs(wheel / 120);
        if (itemsToMove == 0)
        {
            return false;
        }

        bool pushItems = wheel < 0;
        while (itemsToMove > 0)
        {
            Slot? targetSlot = FindWheelApplicableSlot(selectedSlot, pushItems);
            if (targetSlot == null)
            {
                break;
            }

            int moved = pushItems
                ? MoveItemsBetweenSlots(selectedSlot, targetSlot, itemsToMove)
                : MoveItemsBetweenSlots(targetSlot, selectedSlot, itemsToMove);

            if (moved <= 0)
            {
                break;
            }

            itemsToMove -= moved;
        }

        return true;
    }

    private void HandleMouseTweaksDrag(int mouseX, int mouseY)
    {
        Slot? selectedSlot = GetSlotAtPosition(mouseX, mouseY);
        if (selectedSlot == null || selectedSlot == _mouseTweaksSlot || IsMouseTweaksIgnored(selectedSlot))
        {
            return;
        }

        InventoryPlayer playerInventory = Game.player.inventory;
        ItemStack? cursorStack = playerInventory.getCursorStack();
        bool shiftDown = IsShiftDown();

        if (Mouse.isButtonDown(1))
        {
            if (cursorStack != null
                && !IsMouseTweaksResultSlot(selectedSlot)
                && selectedSlot.canInsert(cursorStack)
                && AreStacksCompatible(cursorStack, selectedSlot.getStack()))
            {
                ClickContainerSlot(selectedSlot, 1, false);
            }

            return;
        }

        if (!Mouse.isButtonDown(0))
        {
            return;
        }

        if (cursorStack == null)
        {
            if (shiftDown)
            {
                ClickContainerSlot(selectedSlot, 0, true);
            }

            return;
        }

        if (IsMouseTweaksResultSlot(selectedSlot) || !selectedSlot.canInsert(cursorStack))
        {
            return;
        }

        ItemStack? targetStack = selectedSlot.getStack();
        if (targetStack == null || !AreStacksCompatible(cursorStack, targetStack))
        {
            return;
        }

        if (shiftDown)
        {
            ClickContainerSlot(selectedSlot, 0, true);
            return;
        }

        ClickContainerSlot(selectedSlot, 0, false);
    }

    private int MoveItemsBetweenSlots(Slot sourceSlot, Slot targetSlot, int itemsToMove)
    {
        ItemStack? sourceStack = sourceSlot.getStack();
        if (sourceStack == null || itemsToMove <= 0)
        {
            return 0;
        }

        ItemStack? targetStack = targetSlot.getStack();
        if (targetStack != null && (!AreStacksCompatible(sourceStack, targetStack) || !targetSlot.canInsert(sourceStack)))
        {
            return 0;
        }

        int targetLimit = Math.Min(targetSlot.getMaxItemCount(), sourceStack.getMaxCount());
        int targetCount = targetStack?.count ?? 0;
        int space = targetLimit - targetCount;
        if (space <= 0)
        {
            return 0;
        }

        int moved = Math.Min(itemsToMove, Math.Min(space, sourceStack.count));
        if (moved <= 0)
        {
            return 0;
        }

        ClickContainerSlot(sourceSlot, 0, false);
        ItemStack? cursorStack = Game.player.inventory.getCursorStack();
        if (cursorStack == null)
        {
            return 0;
        }

        if (moved >= cursorStack.count)
        {
            ClickContainerSlot(targetSlot, 0, false);
            return moved;
        }

        int placed = 0;
        while (placed < moved && Game.player.inventory.getCursorStack() != null)
        {
            ClickContainerSlot(targetSlot, 1, false);
            ++placed;
        }

        if (Game.player.inventory.getCursorStack() != null)
        {
            ClickContainerSlot(sourceSlot, 0, false);
        }

        return placed;
    }

    private Slot? FindWheelApplicableSlot(Slot selectedSlot, bool pushItems)
    {
        bool selectedInPlayerInventory = selectedSlot.Inventory == Game.player.inventory;
        ItemStack? selectedStack = selectedSlot.getStack();
        Slot? emptySlot = null;

        for (int slotIndex = InventorySlots.Slots.Count - 1; slotIndex >= 0; --slotIndex)
        {
            Slot slot = InventorySlots.Slots[slotIndex];
            if (slot == selectedSlot || IsMouseTweaksIgnored(slot))
            {
                continue;
            }

            bool slotInPlayerInventory = slot.Inventory == Game.player.inventory;
            if (slotInPlayerInventory == selectedInPlayerInventory)
            {
                continue;
            }

            ItemStack? stack = slot.getStack();
            if (stack == null)
            {
                if (pushItems && emptySlot == null && selectedStack != null && !IsMouseTweaksResultSlot(slot) && slot.canInsert(selectedStack))
                {
                    emptySlot = slot;
                }

                continue;
            }

            if (selectedStack != null && AreStacksCompatible(selectedStack, stack))
            {
                if (pushItems)
                {
                    int limit = Math.Min(slot.getMaxItemCount(), stack.getMaxCount());
                    if (!IsMouseTweaksResultSlot(slot) && stack.count < limit && slot.canInsert(selectedStack))
                    {
                        return slot;
                    }
                }
                else
                {
                    return slot;
                }
            }
        }

        return emptySlot;
    }

    private static bool AreStacksCompatible(ItemStack? a, ItemStack? b)
    {
        return a == null || b == null || a.isItemEqual(b);
    }

    private void HandleDragSplitting(int mouseX, int mouseY)
    {
        Slot? slot = GetSlotAtPosition(mouseX, mouseY);
        if (slot == null)
        {
            return;
        }

        TryAddDragSplittingSlot(slot);
    }

    private void TryAddDragSplittingSlot(Slot slot)
    {
        if (_dragSplittingSlots.Contains(slot) || IsMouseTweaksIgnored(slot) || IsMouseTweaksResultSlot(slot))
        {
            return;
        }

        ItemStack? cursorStack = Game.player.inventory.getCursorStack();
        if (cursorStack == null
            || !ScreenHandler.CanAddItemToSlot(slot, cursorStack, true)
            || !slot.canInsert(cursorStack)
            || cursorStack.count <= _dragSplittingSlots.Count
            || !InventorySlots.canDragIntoSlot(slot))
        {
            return;
        }

        _dragSplittingSlots.Add(slot);
    }

    private void PerformDragSplitting()
    {
        ClickContainerSlot(-999, ScreenHandler.PackDragData(0, _dragSplittingLimit), ScreenHandlerClickMode.Drag);
        foreach (Slot slot in _dragSplittingSlots)
        {
            ClickContainerSlot(slot, ScreenHandler.PackDragData(1, _dragSplittingLimit), ScreenHandlerClickMode.Drag);
        }

        ClickContainerSlot(-999, ScreenHandler.PackDragData(2, _dragSplittingLimit), ScreenHandlerClickMode.Drag);
    }

    private void GetScaledEventMousePosition(out int x, out int y)
    {
        x = Mouse.getEventX() * Width / Game.displayWidth;
        y = Height - Mouse.getEventY() * Height / Game.displayHeight - 1;
    }

    protected bool IsPointOutsideBounds(int x, int y)
    {
        return x < GuiLeft || y < GuiTop || x >= GuiLeft + _xSize || y >= GuiTop + _ySize;
    }

    protected static bool IsShiftDown()
    {
        return Keyboard.isKeyDown(Keyboard.KEY_LSHIFT) || Keyboard.isKeyDown(Keyboard.KEY_RSHIFT);
    }

    public override void GetTooltips(List<ActionTip> tips)
    {
        if (_hoveredSlot != null)
        {
            ItemStack cursorStack = Game.player.inventory.getCursorStack();
            if (_hoveredSlot.hasStack() || cursorStack != null)
            {
                tips.Add(new ActionTip(ControlIcon.A, "Move"));
            }

            if (_hoveredSlot.hasStack())
            {
                tips.Add(new ActionTip(ControlIcon.Y, "Quick Move"));
                if (_hoveredSlot.getStack().count > 1)
                {
                    tips.Add(new ActionTip(ControlIcon.X, "Take Half"));
                }
            }
        }
    }
}
