using BetaSharp.Client.Input;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Items;
using BetaSharp.Creative;
using BetaSharp.Entities;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Screens;
using BetaSharp.Screens.Slots;

namespace BetaSharp.Client.Guis;

public class GuiContainerCreative : GuiContainer
{
    private readonly ItemRenderer _itemRenderer = new();
    private readonly CreativeScreenHandler _creativeScreenHandler;

    private GuiTextField? _searchField;
    private float _scrollPosition;
    private bool _isScrolling;
    private bool _wasMouseDown;
    private int _mouseXForPlayer;
    private int _mouseYForPlayer;

    public GuiContainerCreative(EntityPlayer player) : base(new CreativeScreenHandler(player.inventory))
    {
        _creativeScreenHandler = (CreativeScreenHandler)InventorySlots;
        AllowUserInput = true;
        _xSize = 195;
        _ySize = 136;
        player.increaseStat(global::BetaSharp.Achievements.OpenInventory, 1);
    }

    public override void InitGui()
    {
        if (!Game.playerController.isInCreativeMode())
        {
            Game.displayGuiScreen(new GuiInventory(Game.player));
            return;
        }

        base.InitGui();
        _controlList.Clear();

        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        _searchField = new GuiTextField(this, FontRenderer, guiLeft + 82, guiTop + 8, 89, 10, string.Empty);
        _searchField.SetMaxStringLength(15);
        _searchField.EnableBackgroundDrawing = false;
        SetCurrentTab(_creativeScreenHandler.CurrentTab);
    }

    public override void UpdateScreen()
    {
        base.UpdateScreen();
        _searchField?.updateCursorCounter();
        if (!Game.playerController.isInCreativeMode())
        {
            Game.displayGuiScreen(new GuiInventory(Game.player));
        }
    }

    public override void HandleMouseInput()
    {
        base.HandleMouseInput();
        int wheel = Mouse.getEventDWheel();
        if (wheel != 0 && _creativeScreenHandler.NeedsScrollBar())
        {
            int rows = _creativeScreenHandler.GetScrollableRowCount();
            if (rows > 0)
            {
                _scrollPosition -= Math.Sign(wheel) / (float)rows;
                _scrollPosition = Math.Clamp(_scrollPosition, 0.0F, 1.0F);
                _creativeScreenHandler.ScrollTo(_scrollPosition);
            }
        }
    }

    public override void Render(int mouseX, int mouseY, float partialTicks)
    {
        _mouseXForPlayer = mouseX;
        _mouseYForPlayer = mouseY;
        bool mouseDown = Mouse.isButtonDown(0);
        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        int scrollLeft = guiLeft + 175;
        int scrollTop = guiTop + 18;
        int scrollRight = scrollLeft + 14;
        int scrollBottom = scrollTop + 112;

        if (!_wasMouseDown && mouseDown && mouseX >= scrollLeft && mouseY >= scrollTop && mouseX < scrollRight && mouseY < scrollBottom)
        {
            _isScrolling = _creativeScreenHandler.NeedsScrollBar();
        }

        if (!mouseDown)
        {
            _isScrolling = false;
        }

        _wasMouseDown = mouseDown;
        if (_isScrolling)
        {
            _scrollPosition = (mouseY - scrollTop - 7.5F) / ((scrollBottom - scrollTop) - 15.0F);
            _scrollPosition = Math.Clamp(_scrollPosition, 0.0F, 1.0F);
            _creativeScreenHandler.ScrollTo(_scrollPosition);
        }

        base.Render(mouseX, mouseY, partialTicks);

        foreach (CreativeInventoryTab tab in CreativeInventoryTab.Tabs)
        {
            if (IsMouseOverTab(tab, mouseX, mouseY))
            {
                DrawCreativeTabHoveringText(tab.Label, mouseX, mouseY);
                break;
            }
        }
    }

    protected override void DrawGuiContainerForegroundLayer()
    {
        if (_creativeScreenHandler.CurrentTab.DrawTitle)
        {
            FontRenderer.DrawString(_creativeScreenHandler.CurrentTab.Label, 8, 6, Color.Gray40);
        }
    }

    protected override void DrawGuiContainerBackgroundLayer(float partialTicks)
    {
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);

        foreach (CreativeInventoryTab tab in CreativeInventoryTab.Tabs)
        {
            if (tab != _creativeScreenHandler.CurrentTab)
            {
                RenderCreativeTab(tab);
            }
        }

        Game.textureManager.BindTexture(Game.textureManager.GetTextureId(_creativeScreenHandler.CurrentTab.BackgroundTexture));
        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        DrawTexturedModalRect(guiLeft, guiTop, 0, 0, _xSize, _ySize);

        _searchField?.DrawTextBox();

        if (_creativeScreenHandler.NeedsScrollBar())
        {
            DrawScrollBar(guiLeft, guiTop);
        }

        RenderCreativeTab(_creativeScreenHandler.CurrentTab);

        if (_creativeScreenHandler.CurrentTab == CreativeInventoryTab.Inventory)
        {
            GuiInventory.DrawPlayerOnGui(Game, guiLeft + 43, guiTop + 45, 20, guiLeft + 43 - _mouseXForPlayer, guiTop + 15 - _mouseYForPlayer);
        }
    }

    protected override void KeyTyped(char eventChar, int eventKey)
    {
        if (_creativeScreenHandler.CurrentTab != CreativeInventoryTab.Inventory && _hoveredSlot != null && _hoveredSlot.id < 45 && _hoveredSlot.hasStack())
        {
            ItemStack hoveredStack = _hoveredSlot.getStack();
            for (int hotbarSlot = 0; hotbarSlot < 9; ++hotbarSlot)
            {
                if (eventKey == Keyboard.KEY_1 + hotbarSlot)
                {
                    ItemStack?[] before = CaptureInventorySnapshot(Game.player.inventory);
                    ItemStack copiedStack = hoveredStack.copy();
                    copiedStack.count = copiedStack.getMaxCount();
                    Game.player.inventory.setStack(hotbarSlot, copiedStack);
                    SyncCreativeInventoryToServer(before, Game.player.inventory);
                    return;
                }
            }

            if (eventKey == Game.options.KeyBindDrop.keyCode)
            {
                ItemStack droppedStack = hoveredStack.copy();
                droppedStack.count = Keyboard.isKeyDown(Keyboard.KEY_LCONTROL) || Keyboard.isKeyDown(Keyboard.KEY_RCONTROL)
                    ? droppedStack.getMaxCount()
                    : 1;
                Game.player.dropItem(droppedStack);
                if (Game.playerController is PlayerControllerMP playerControllerMp)
                {
                    playerControllerMp.SendCreativeDropAction(droppedStack.copy());
                }

                return;
            }
        }

        if (_creativeScreenHandler.CurrentTab == CreativeInventoryTab.Search
            && eventKey != Keyboard.KEY_ESCAPE
            && eventKey != Game.options.KeyBindInventory.keyCode)
        {
            _searchField?.textboxKeyTyped(eventChar, eventKey);
            _creativeScreenHandler.SetSearchQuery(_searchField?.GetText() ?? string.Empty);
            _scrollPosition = 0.0F;
            return;
        }

        base.KeyTyped(eventChar, eventKey);
    }

    protected override void MouseClicked(int x, int y, int button)
    {
        if (button == 0)
        {
            foreach (CreativeInventoryTab tab in CreativeInventoryTab.Tabs)
            {
                if (IsMouseOverTab(tab, x, y))
                {
                    SetCurrentTab(tab);
                    return;
                }
            }
        }

        _searchField?.MouseClicked(x, y, button);

        InventoryPlayer playerInventory = Game.player.inventory;
        Slot? slot = GetSlotAtPosition(x, y);
        if (slot != null)
        {
            if (_creativeScreenHandler.CurrentTab == CreativeInventoryTab.Inventory)
            {
                HandleInventoryTabClick(slot, button, playerInventory);
            }
            else if (slot.id < 45)
            {
                HandleSelectionSlotClick(slot, button, Keyboard.isKeyDown(Keyboard.KEY_LSHIFT) || Keyboard.isKeyDown(Keyboard.KEY_RSHIFT), playerInventory);
            }
            else
            {
                InventorySlots.onSlotClick(slot.id, button, false, Game.player);
                ItemStack? slotStack = InventorySlots.GetSlot(slot.id).getStack();
                if (Game.playerController is PlayerControllerMP playerControllerMp)
                {
                    int creativeSlot = slot.id - InventorySlots.Slots.Count + 45;
                    playerControllerMp.SendCreativeSlotAction(slotStack, creativeSlot);
                }
            }

            return;
        }

        if (playerInventory.getCursorStack() == null)
        {
            return;
        }

        if (button == 0)
        {
            ItemStack stackToDrop = playerInventory.getCursorStack();
            Game.player.dropItem(stackToDrop);
            if (Game.playerController is PlayerControllerMP playerController)
            {
                playerController.SendCreativeDropAction(stackToDrop.copy());
            }

            playerInventory.setItemStack(null);
        }
        else if (button == 1)
        {
            ItemStack stackToDrop = playerInventory.getCursorStack().split(1);
            Game.player.dropItem(stackToDrop);
            if (Game.playerController is PlayerControllerMP playerController)
            {
                playerController.SendCreativeDropAction(stackToDrop.copy());
            }

            if (playerInventory.getCursorStack().count == 0)
            {
                playerInventory.setItemStack(null);
            }
        }
    }

    protected override void HandleQuickMove(int x, int y)
    {
    }

    private void SetCurrentTab(CreativeInventoryTab tab)
    {
        _creativeScreenHandler.SetTab(tab);
        _scrollPosition = 0.0F;
        if (_searchField == null)
        {
            return;
        }

        bool isSearch = tab == CreativeInventoryTab.Search;
        _searchField.SetText(string.Empty);
        _searchField.IsVisible = isSearch;
        _searchField.SetFocused(isSearch);
        if (!isSearch)
        {
            _creativeScreenHandler.SetSearchQuery(string.Empty);
        }
    }

    private void RenderCreativeTab(CreativeInventoryTab tab)
    {
        bool selected = tab == _creativeScreenHandler.CurrentTab;
        int textureU = tab.Column * 28;
        int textureV = selected ? 32 : 0;
        int x = GetTabRenderLeft(tab);
        int y = GetTabTop(tab);

        if (!tab.IsTopRow)
        {
            textureV += 64;
        }

        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/gui/allitems.png"));
        GLManager.GL.Disable(GLEnum.Lighting);
        DrawTexturedModalRect(x, y, textureU, textureV, 28, 32);

        int itemX = x + 6;
        int itemY = y + 8 + (tab.IsTopRow ? 1 : -1);
        GLManager.GL.Enable(GLEnum.Lighting);
        GLManager.GL.Enable(GLEnum.RescaleNormal);
        _itemRenderer.renderItemIntoGUI(FontRenderer, Game.textureManager, tab.Icon, itemX, itemY);
        _itemRenderer.renderItemOverlayIntoGUI(FontRenderer, Game.textureManager, tab.Icon, itemX, itemY);
        GLManager.GL.Disable(GLEnum.Lighting);
    }

    private void DrawScrollBar(int guiLeft, int guiTop)
    {
        int scrollY = guiTop + 18 + (int)(95.0F * _scrollPosition);
        int textureU = _creativeScreenHandler.NeedsScrollBar() ? 232 : 244;

        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/gui/allitems.png"));
        DrawTexturedModalRect(guiLeft + 175, scrollY, textureU, 0, 12, 15);
    }

    private bool IsMouseOverTab(CreativeInventoryTab tab, int mouseX, int mouseY)
    {
        int x = GetTabLeft(tab);
        int y = GetTabTop(tab);
        return mouseX >= x + 3 && mouseX <= x + 26 && mouseY >= y + 3 && mouseY <= y + 30;
    }

    private int GetTabLeft(CreativeInventoryTab tab)
    {
        int guiLeft = (Width - _xSize) / 2;
        int x = guiLeft + 28 * tab.Column;
        if (tab.Column == 5)
        {
            x = guiLeft + _xSize - 28 + 2;
        }
        else if (tab.Column > 0)
        {
            x += tab.Column;
        }

        return x;
    }

    private int GetTabRenderLeft(CreativeInventoryTab tab)
    {
        int guiLeft = (Width - _xSize) / 2;
        int x = guiLeft + 28 * tab.Column;
        if (tab.Column == 5)
        {
            x = guiLeft + _xSize - 28;
        }
        else if (tab.Column > 0)
        {
            x += tab.Column;
        }

        return x;
    }

    private int GetTabTop(CreativeInventoryTab tab)
    {
        int guiTop = (Height - _ySize) / 2;
        return tab.IsTopRow ? guiTop - 28 : guiTop + _ySize - 4;
    }

    private void DrawCreativeTabHoveringText(string text, int mouseX, int mouseY)
    {
        int tipX = mouseX + 12;
        int tipY = mouseY - 12;
        int textWidth = FontRenderer.GetStringWidth(text);
        DrawGradientRect(tipX - 3, tipY - 3, tipX + textWidth + 3, tipY + 11, Color.BlackAlphaC0, Color.BlackAlphaC0);
        FontRenderer.DrawStringWithShadow(text, tipX, tipY, Color.White);
    }

    private void HandleSelectionSlotClick(Slot slot, int button, bool fillStack, InventoryPlayer playerInventory)
    {
        ItemStack? cursorStack = playerInventory.getCursorStack();
        ItemStack? slotStack = slot.getStack();
        if (cursorStack != null && slotStack != null && cursorStack.isItemEqual(slotStack))
        {
            if (button == 0)
            {
                if (fillStack)
                {
                    cursorStack.count = cursorStack.getMaxCount();
                }
                else if (cursorStack.count < cursorStack.getMaxCount())
                {
                    ++cursorStack.count;
                }
            }
            else if (cursorStack.count <= 1)
            {
                playerInventory.setItemStack(null);
            }
            else
            {
                --cursorStack.count;
            }

            return;
        }

        if (cursorStack != null)
        {
            playerInventory.setItemStack(null);
            return;
        }

        if (slotStack == null)
        {
            return;
        }

        ItemStack copiedStack = slotStack.copy();
        if (fillStack)
        {
            copiedStack.count = copiedStack.getMaxCount();
        }

        playerInventory.setItemStack(copiedStack);
    }
    private void HandleInventoryTabClick(Slot slot, int button, InventoryPlayer playerInventory)
    {
        bool isShiftDown = Keyboard.isKeyDown(Keyboard.KEY_LSHIFT) || Keyboard.isKeyDown(Keyboard.KEY_RSHIFT);

        if (slot.id == 45)
        {
            if (isShiftDown)
            {
                for (int slotIndex = 0; slotIndex < playerInventory.size(); ++slotIndex)
                {
                    playerInventory.setStack(slotIndex, null);
                }

                playerInventory.setItemStack(null);
                SyncCreativeInventoryToServer(null, playerInventory);
            }
            else
            {
                playerInventory.setItemStack(null);
            }

            return;
        }

        ItemStack?[] before = CaptureInventorySnapshot(playerInventory);
        int clickMode = button == 2
            ? ScreenHandlerClickMode.Clone
            : isShiftDown
                ? ScreenHandlerClickMode.QuickMove
                : ScreenHandlerClickMode.Pickup;
        InventorySlots.onSlotClick(slot.id, button, clickMode, Game.player);
        SyncCreativeInventoryToServer(before, playerInventory);
    }

    private static ItemStack?[] CaptureInventorySnapshot(InventoryPlayer playerInventory)
    {
        ItemStack?[] snapshot = new ItemStack?[playerInventory.size()];
        for (int slotIndex = 0; slotIndex < snapshot.Length; ++slotIndex)
        {
            snapshot[slotIndex] = playerInventory.getStack(slotIndex)?.copy();
        }

        return snapshot;
    }

    private void SyncCreativeInventoryToServer(ItemStack?[]? before, InventoryPlayer playerInventory)
    {
        if (Game.playerController is not PlayerControllerMP playerControllerMp)
        {
            return;
        }

        for (int inventorySlot = 0; inventorySlot < playerInventory.size(); ++inventorySlot)
        {
            ItemStack? afterStack = playerInventory.getStack(inventorySlot);
            ItemStack? beforeStack = before != null ? before[inventorySlot] : null;
            if (before != null && ItemStack.areEqual(beforeStack, afterStack))
            {
                continue;
            }

            int creativeSlot = inventorySlot switch
            {
                >= 0 and < 9 => inventorySlot + 36,
                >= 9 and < 36 => inventorySlot,
                _ => 44 - inventorySlot
            };

            playerControllerMp.SendCreativeSlotAction(afterStack, creativeSlot);
        }
    }
}
