using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Inventorys;
using BetaSharp.Screens;

namespace BetaSharp.Client.Guis;

public sealed class GuiBrewingStand : GuiContainer
{
    private readonly BlockEntityBrewingStand _brewingStand;

    public GuiBrewingStand(InventoryPlayer playerInventory, BlockEntityBrewingStand brewingStand) : base(new BrewingStandScreenHandler(playerInventory, brewingStand))
    {
        _brewingStand = brewingStand;
    }

    protected override void DrawGuiContainerForegroundLayer()
    {
        FontRenderer.DrawString("Brewing Stand", 44, 6, Color.Gray40);
        FontRenderer.DrawString("Inventory", 8, _ySize - 96 + 2, Color.Gray40);
    }

    protected override void DrawGuiContainerBackgroundLayer(float partialTicks)
    {
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/gui/alchemy.png"));
        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;
        DrawTexturedModalRect(guiLeft, guiTop, 0, 0, _xSize, _ySize);

        int brewTime = _brewingStand.brewTime;
        if (brewTime > 0)
        {
            int progress = (int)(28.0F * (1.0F - brewTime / 400.0F));
            if (progress > 0)
            {
                DrawTexturedModalRect(guiLeft + 97, guiTop + 16, 176, 0, 9, progress);
            }

            int bubble = ((brewTime / 2) % 7) switch
            {
                0 => 29,
                1 => 24,
                2 => 20,
                3 => 16,
                4 => 11,
                5 => 6,
                _ => 0
            };

            if (bubble > 0)
            {
                DrawTexturedModalRect(guiLeft + 65, guiTop + 14 + 29 - bubble, 185, 29 - bubble, 12, bubble);
            }
        }
    }
}
