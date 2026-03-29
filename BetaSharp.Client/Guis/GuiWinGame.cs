using BetaSharp.Client.Input;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;

namespace BetaSharp.Client.Guis;

public sealed class GuiWinGame : GuiScreen
{
    private const float ScrollSpeed = 0.5F;
    private const int TextWidth = 274;

    private readonly List<string> _lines = [];
    private int _updateCounter;
    private int _scrollHeight;

    public override bool PausesGame => true;

    public override void InitGui()
    {
        _controlList.Clear();
        EnsureLinesLoaded();
    }

    public override void UpdateScreen()
    {
        ++_updateCounter;
        float maxTicks = (_scrollHeight + Height + Height + 24) / ScrollSpeed;
        if (_updateCounter > maxTicks)
        {
            CloseCredits();
        }
    }

    protected override void KeyTyped(char eventChar, int eventKey)
    {
        if (eventKey == Keyboard.KEY_ESCAPE)
        {
            CloseCredits();
        }
    }

    public override void Render(int mouseX, int mouseY, float partialTicks)
    {
        DrawScrollingBackground(partialTicks);

        Tessellator tessellator = Tessellator.instance;
        int left = Width / 2 - TextWidth / 2;
        int logoY = Height + 50;
        float scrollOffset = -(_updateCounter + partialTicks) * ScrollSpeed;

        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(0.0F, scrollOffset, 0.0F);

        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/title/mclogo.png"));
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        DrawTexturedModalRect(left, logoY, 0, 0, 155, 44);
        DrawTexturedModalRect(left + 155, logoY, 0, 45, 155, 44);
        tessellator.setColorOpaque_I(0xFFFFFF);

        int y = logoY + 200;
        for (int i = 0; i < _lines.Count; ++i)
        {
            if (i == _lines.Count - 1)
            {
                float lastLineOffset = y + scrollOffset - (Height / 2.0F - 6.0F);
                if (lastLineOffset < 0.0F)
                {
                    GLManager.GL.Translate(0.0F, -lastLineOffset, 0.0F);
                }
            }

            if (y + scrollOffset + 20.0F > 0.0F && y + scrollOffset < Height)
            {
                string line = _lines[i];
                if (line.StartsWith("[C]", StringComparison.Ordinal))
                {
                    DrawCenteredString(FontRenderer, line[3..], Width / 2, y, Color.White);
                }
                else
                {
                    FontRenderer.DrawStringWithShadow(line, left, y, Color.White);
                }
            }

            y += 12;
        }

        GLManager.GL.PopMatrix();
        DrawVignette();
        base.Render(mouseX, mouseY, partialTicks);
    }

    private void DrawScrollingBackground(float partialTicks)
    {
        GLManager.GL.Disable(GLEnum.Lighting);
        GLManager.GL.Disable(GLEnum.Fog);

        Tessellator tessellator = Tessellator.instance;
        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/gui/background.png"));
        tessellator.startDrawingQuads();
        tessellator.setColorOpaque_F(GetBackgroundBrightness(partialTicks), GetBackgroundBrightness(partialTicks), GetBackgroundBrightness(partialTicks));

        float scroll = -(_updateCounter + partialTicks) * 0.5F * ScrollSpeed;
        float scrollBottom = Height - (_updateCounter + partialTicks) * 0.5F * ScrollSpeed;
        const float scale = 1.0F / 64.0F;
        tessellator.addVertexWithUV(0.0D, Height, _zLevel, 0.0D, scroll * scale);
        tessellator.addVertexWithUV(Width, Height, _zLevel, Width * scale, scroll * scale);
        tessellator.addVertexWithUV(Width, 0.0D, _zLevel, Width * scale, scrollBottom * scale);
        tessellator.addVertexWithUV(0.0D, 0.0D, _zLevel, 0.0D, scrollBottom * scale);
        tessellator.draw();
    }

    private float GetBackgroundBrightness(float partialTicks)
    {
        float fade = (_scrollHeight + Height + Height + 24) / ScrollSpeed;
        float brightness = ((_updateCounter + partialTicks) - 0.0F) * 0.02F;
        float endFade = (fade - 20.0F - (_updateCounter + partialTicks)) * 0.005F;
        if (endFade < brightness)
        {
            brightness = endFade;
        }

        if (brightness > 1.0F)
        {
            brightness = 1.0F;
        }

        brightness *= brightness;
        return brightness * 96.0F / 255.0F;
    }

    private void DrawVignette()
    {
        Tessellator tessellator = Tessellator.instance;
        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("%blur%/misc/vignette.png"));
        GLManager.GL.Enable(GLEnum.Blend);
        GLManager.GL.BlendFunc(GLEnum.Zero, GLEnum.OneMinusSrcColor);
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        tessellator.startDrawingQuads();
        tessellator.addVertexWithUV(0.0D, Height, _zLevel, 0.0D, 1.0D);
        tessellator.addVertexWithUV(Width, Height, _zLevel, 1.0D, 1.0D);
        tessellator.addVertexWithUV(Width, 0.0D, _zLevel, 1.0D, 0.0D);
        tessellator.addVertexWithUV(0.0D, 0.0D, _zLevel, 0.0D, 0.0D);
        tessellator.draw();
        GLManager.GL.Disable(GLEnum.Blend);
    }

    private void EnsureLinesLoaded()
    {
        if (_lines.Count > 0)
        {
            return;
        }

        try
        {
            string playerName = Game.session?.username ?? Game.player?.name ?? "Player";
            Random obfuscationRandom = new(8124371);

            LoadCreditsFile("title/win.txt", playerName, obfuscationRandom);
            for (int i = 0; i < 8; ++i)
            {
                _lines.Add(string.Empty);
            }

            LoadCreditsFile("title/credits.txt", playerName, obfuscationRandom);
        }
        catch
        {
            _lines.Add("[C]The End");
            _lines.Add(string.Empty);
            _lines.Add("Credits could not be loaded.");
        }

        _scrollHeight = _lines.Count * 12;
    }

    private void LoadCreditsFile(string path, string playerName, Random obfuscationRandom)
    {
        string contents = AssetManager.Instance.getAsset(path).GetTextContent();
        using StringReader reader = new(contents);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Replace("PLAYERNAME", playerName, StringComparison.Ordinal)
                       .Replace("\t", "    ", StringComparison.Ordinal);

            while (line.Contains("§f§k§a§b", StringComparison.Ordinal))
            {
                int index = line.IndexOf("§f§k§a§b", StringComparison.Ordinal);
                string replacement = new('X', obfuscationRandom.Next(3, 7));
                line = line[..index] + "§f" + replacement + line[(index + 8)..];
            }

            AddWrappedLine(line, TextWidth);
            _lines.Add(string.Empty);
        }
    }

    private void AddWrappedLine(string line, int maxWidth)
    {
        foreach (string wrappedLine in WrapLine(line, maxWidth))
        {
            _lines.Add(wrappedLine);
        }
    }

    private IEnumerable<string> WrapLine(string line, int maxWidth)
    {
        if (string.IsNullOrEmpty(line))
        {
            yield return string.Empty;
            yield break;
        }

        string remaining = line.TrimEnd();
        while (remaining.Length > 0)
        {
            if (FontRenderer.GetStringWidth(remaining) <= maxWidth)
            {
                yield return remaining;
                yield break;
            }

            int bestBreak = -1;
            for (int i = 0; i < remaining.Length; ++i)
            {
                if (remaining[i] == ' ')
                {
                    bestBreak = i;
                }

                if (FontRenderer.GetStringWidth(remaining[..(i + 1)]) > maxWidth)
                {
                    break;
                }
            }

            if (bestBreak <= 0)
            {
                bestBreak = 1;
                while (bestBreak < remaining.Length && FontRenderer.GetStringWidth(remaining[..bestBreak]) <= maxWidth)
                {
                    ++bestBreak;
                }

                bestBreak = Math.Max(1, bestBreak - 1);
            }

            yield return remaining[..bestBreak].TrimEnd();
            remaining = remaining[bestBreak..].TrimStart();
        }
    }

    private void CloseCredits()
    {
        Game.displayGuiScreen(null);
        Game.setIngameFocus();
    }
}
