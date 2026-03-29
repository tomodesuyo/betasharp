using BetaSharp.Client.Input;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Items;
using BetaSharp.Inventorys;
using BetaSharp.Items;
using BetaSharp.Screens;
using BetaSharp.Trading;

namespace BetaSharp.Client.Guis;

public sealed class GuiMerchant : GuiContainer
{
    private const int PreviousButtonId = 1;
    private const int NextButtonId = 2;
    private readonly ItemRenderer _itemRenderer = new();

    private readonly string _title;
    private readonly MerchantScreenHandler _merchantScreenHandler;
    private GuiButton? _previousButton;
    private GuiButton? _nextButton;

    public GuiMerchant(InventoryPlayer playerInventory, string title) : base(new MerchantScreenHandler(playerInventory))
    {
        _title = title;
        _merchantScreenHandler = (MerchantScreenHandler)InventorySlots;
    }

    public override void InitGui()
    {
        base.InitGui();
        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        _controlList.Add(_nextButton = new GuiButtonMerchant(NextButtonId, guiLeft + 147, guiTop + 23, true));
        _controlList.Add(_previousButton = new GuiButtonMerchant(PreviousButtonId, guiLeft + 17, guiTop + 23, false));
        UpdateButtons();
    }

    protected override void ActionPerformed(GuiButton button)
    {
        base.ActionPerformed(button);

        if (button.Id == PreviousButtonId)
        {
            ChangeSelection(-1);
        }
        else if (button.Id == NextButtonId)
        {
            ChangeSelection(1);
        }
    }

    public override void Render(int mouseX, int mouseY, float partialTicks)
    {
        UpdateButtons();
        base.Render(mouseX, mouseY, partialTicks);
        DrawTradePreview(mouseX, mouseY);
    }

    protected override void DrawGuiContainerForegroundLayer()
    {
        DrawCenteredString(FontRenderer, _title, _xSize / 2, 6, Color.Gray40);
        FontRenderer.DrawString("Inventory", 8, _ySize - 96 + 2, Color.Gray40);

        if (_merchantScreenHandler.Offers.Count > 0)
        {
            string counter = $"{_merchantScreenHandler.SelectedRecipeIndex + 1}/{_merchantScreenHandler.Offers.Count}";
            FontRenderer.DrawString(counter, 114, 6, Color.Gray40);

            MerchantRecipe? recipe = GetSelectedRecipe();
            if (recipe != null && recipe.IsDisabled)
            {
                FontRenderer.DrawString("Out of stock", 83, 22, Color.Yellow);
            }
        }
    }

    protected override void DrawGuiContainerBackgroundLayer(float partialTicks)
    {
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/gui/trading.png"));
        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        DrawTexturedModalRect(guiLeft, guiTop, 0, 0, _xSize, _ySize);

        MerchantRecipe? recipe = GetSelectedRecipe();
        if (recipe != null && recipe.IsDisabled)
        {
            DrawTexturedModalRect(guiLeft + 83, guiTop + 21, 212, 0, 28, 21);
            DrawTexturedModalRect(guiLeft + 83, guiTop + 51, 212, 0, 28, 21);
        }
    }

    private void ChangeSelection(int delta)
    {
        if (_merchantScreenHandler.Offers.Count == 0)
        {
            return;
        }

        int currentIndex = Math.Max(_merchantScreenHandler.SelectedRecipeIndex, 0);
        int nextIndex = Math.Clamp(currentIndex + delta, 0, _merchantScreenHandler.Offers.Count - 1);
        if (nextIndex == _merchantScreenHandler.SelectedRecipeIndex)
        {
            return;
        }

        _merchantScreenHandler.SelectRecipe(nextIndex);
        if (Game.playerController is PlayerControllerMP playerController)
        {
            playerController.SelectMerchantTrade(InventorySlots.SyncId, nextIndex);
        }

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        if (_previousButton == null || _nextButton == null)
        {
            return;
        }

        int offersCount = _merchantScreenHandler.Offers.Count;
        int selectedIndex = Math.Max(_merchantScreenHandler.SelectedRecipeIndex, 0);
        _previousButton.Enabled = offersCount > 0 && selectedIndex > 0;
        _nextButton.Enabled = offersCount > 0 && selectedIndex < offersCount - 1;
    }

    private MerchantRecipe? GetSelectedRecipe()
    {
        int selectedIndex = _merchantScreenHandler.SelectedRecipeIndex;
        return selectedIndex >= 0 && selectedIndex < _merchantScreenHandler.Offers.Count
            ? _merchantScreenHandler.Offers[selectedIndex]
            : null;
    }

    private void DrawTradePreview(int mouseX, int mouseY)
    {
        MerchantRecipe? recipe = GetSelectedRecipe();
        if (recipe == null)
        {
            return;
        }

        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;

        GLManager.GL.PushMatrix();
        Lighting.turnOn();
        GLManager.GL.Enable(GLEnum.RescaleNormal);
        GLManager.GL.Translate(0.0F, 0.0F, 32.0F);

        RenderPreviewStack(recipe.BuyItem1, guiLeft + 36, guiTop + 24);
        if (recipe.BuyItem2 != null)
        {
            RenderPreviewStack(recipe.BuyItem2, guiLeft + 62, guiTop + 24);
        }

        RenderPreviewStack(recipe.SellItem, guiLeft + 120, guiTop + 24);

        GLManager.GL.PopMatrix();
        Lighting.turnOff();

        if (Game.player.inventory.getCursorStack() != null)
        {
            return;
        }

        if (IsPointInRegion(36, 24, 16, 16, mouseX, mouseY))
        {
            DrawPreviewTooltip(recipe.BuyItem1, mouseX, mouseY);
        }
        else if (recipe.BuyItem2 != null && IsPointInRegion(62, 24, 16, 16, mouseX, mouseY))
        {
            DrawPreviewTooltip(recipe.BuyItem2, mouseX, mouseY);
        }
        else if (IsPointInRegion(120, 24, 16, 16, mouseX, mouseY))
        {
            DrawPreviewTooltip(recipe.SellItem, mouseX, mouseY);
        }
        else if (recipe.IsDisabled && (IsPointInRegion(83, 21, 28, 21, mouseX, mouseY) || IsPointInRegion(83, 51, 28, 21, mouseX, mouseY)))
        {
            DrawSimpleTooltip("merchant.deprecated", mouseX, mouseY, true);
        }
    }

    private void RenderPreviewStack(ItemStack stack, int x, int y)
    {
        _itemRenderer.renderItemIntoGUI(FontRenderer, Game.textureManager, stack, x, y);
        _itemRenderer.renderItemOverlayIntoGUI(FontRenderer, Game.textureManager, stack, x, y);
    }

    private void DrawPreviewTooltip(ItemStack stack, int mouseX, int mouseY)
    {
        string key = stack.getItemName();
        string translatedName = TranslationStorage.Instance.TranslateNamedKey(key).Trim();
        DrawSimpleTooltip(translatedName.Length > 0 ? translatedName : key, mouseX, mouseY, false);
    }

    private void DrawSimpleTooltip(string text, int mouseX, int mouseY, bool translateKey)
    {
        string line = translateKey ? TranslationStorage.Instance.TranslateKey(text) : text;
        int tipX = mouseX + 12;
        int tipY = mouseY - 12;
        int textWidth = FontRenderer.GetStringWidth(line);
        DrawGradientRect(tipX - 3, tipY - 3, tipX + textWidth + 3, tipY + 11, Color.BlackAlphaC0, Color.BlackAlphaC0);
        FontRenderer.DrawStringWithShadow(line, tipX, tipY, Color.White);
    }

    private bool IsPointInRegion(int x, int y, int width, int height, int mouseX, int mouseY)
    {
        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        int relativeMouseX = mouseX - guiLeft;
        int relativeMouseY = mouseY - guiTop;
        return relativeMouseX >= x && relativeMouseX < x + width && relativeMouseY >= y && relativeMouseY < y + height;
    }
}
