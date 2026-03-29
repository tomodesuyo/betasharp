using BetaSharp.Client.Rendering.Core;

namespace BetaSharp.Client.Guis;

internal sealed class GuiButtonMerchant : GuiButton
{
    private readonly bool _isNextButton;

    public GuiButtonMerchant(int id, int x, int y, bool isNextButton) : base(id, x, y, 12, 19, string.Empty)
    {
        _isNextButton = isNextButton;
    }

    public override void DrawButton(BetaSharp game, int mouseX, int mouseY)
    {
        if (!Visible)
        {
            return;
        }

        game.textureManager.BindTexture(game.textureManager.GetTextureId("/gui/trading.png"));
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);

        bool isHovered = mouseX >= XPosition && mouseY >= YPosition && mouseX < XPosition + Width && mouseY < YPosition + Height;
        int textureU = 176;
        if (!Enabled)
        {
            textureU += Width * 2;
        }
        else if (isHovered)
        {
            textureU += Width;
        }

        int textureV = _isNextButton ? 0 : Height;
        DrawTexturedModalRect(XPosition, YPosition, textureU, textureV, Width, Height);
    }
}
