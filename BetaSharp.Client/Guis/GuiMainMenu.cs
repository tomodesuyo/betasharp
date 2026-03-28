using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Core.Textures;
using BetaSharp.Util.Maths;
using Microsoft.Extensions.Logging;
using GLConst = BetaSharp.Client.Rendering.Core.OpenGL.GLEnum;

namespace BetaSharp.Client.Guis;

public class GuiMainMenu : GuiScreen
{

    private readonly ILogger<GuiMainMenu> _logger = Log.Instance.For<GuiMainMenu>();
    private const int ButtonOptions = 0;
    private const int ButtonSingleplayer = 1;
    private const int ButtonMultiplayer = 2;
    private const int ButtonTexturePacksAndMods = 3;
    private const int ButtonQuit = 4;

    private static readonly JavaRandom s_rand = new();
    private static readonly string[] s_panoramaPaths =
    [
        "/title/bg/panorama0.png",
        "/title/bg/panorama1.png",
        "/title/bg/panorama2.png",
        "/title/bg/panorama3.png",
        "/title/bg/panorama4.png",
        "/title/bg/panorama5.png",
    ];

    private string _splashText = "missingno";
    private GuiButton? _multiplayerButton;
    private int _panoramaTimer;
    private GLTexture? _backgroundTexture;

    public GuiMainMenu()
    {
        try
        {
            List<string> splashLines = [];
            string splashesText = AssetManager.Instance.getAsset("title/splashes.txt").GetTextContent();
            using (StringReader reader = new(splashesText))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0)
                    {
                        splashLines.Add(line);
                    }
                }
            }

            if (splashLines.Count > 0)
            {
                _splashText = splashLines[s_rand.NextInt(splashLines.Count)];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading splash text");
        }
    }

    public override void UpdateScreen()
    {
        ++_panoramaTimer;
    }

    protected override void KeyTyped(char eventChar, int eventKey)
    {
    }

    public override void InitGui()
    {
        EnsureBackgroundTexture();

        // Special days
        DateTime now = DateTime.Now;
        if (now.Month == 11 && now.Day == 9) _splashText = "Happy birthday, ez!";
        else if (now.Month == 6 && now.Day == 1) _splashText = "Happy birthday, Notch!";
        else if (now.Month == 12 && now.Day == 24) _splashText = "Merry X-mas!";
        else if (now.Month == 1 && now.Day == 1) _splashText = "Happy new year!";

        TranslationStorage translator = TranslationStorage.Instance;
        int buttonTopY = Height / 4 + 48;

        _controlList.Add(new GuiButton(ButtonSingleplayer, Width / 2 - 100, buttonTopY, translator.TranslateKey("menu.singleplayer")));
        _controlList.Add(_multiplayerButton =
            new GuiButton(ButtonMultiplayer, Width / 2 - 100, buttonTopY + 24, translator.TranslateKey("menu.multiplayer")));
        _controlList.Add(new GuiButton(ButtonTexturePacksAndMods, Width / 2 - 100, buttonTopY + 48, translator.TranslateKey("menu.mods")));

        if (Game.hideQuitButton)
        {
            _controlList.Add(new GuiButton(ButtonOptions, Width / 2 - 100, buttonTopY + 72, translator.TranslateKey("menu.options")));
        }
        else
        {
            _controlList.Add(new GuiButton(ButtonOptions, Width / 2 - 100, buttonTopY + 72 + 12, 98, 20,
                translator.TranslateKey("menu.options")));

            _controlList.Add(new GuiButton(ButtonQuit, Width / 2 + 2, buttonTopY + 72 + 12, 98, 20,
                translator.TranslateKey("menu.quit")));
        }

        if (Game.session == null || Game.session.sessionId == "-")
        {
            _multiplayerButton.Enabled = false;
        }
    }

    protected override void ActionPerformed(GuiButton button)
    {
        switch (button.Id)
        {
            case ButtonOptions:
                Game.displayGuiScreen(new GuiOptions(this, Game.options));
                break;
            case ButtonSingleplayer:
                Game.displayGuiScreen(new GuiSelectWorld(this));
                break;
            case ButtonMultiplayer:
                Game.displayGuiScreen(new GuiMultiplayer(this, Game.options));
                break;
            case ButtonTexturePacksAndMods:
                Game.displayGuiScreen(new GuiTexturePacks(this));
                break;
            case ButtonQuit:
                Game.shutdown();
                break;
        }
    }

    public override void Render(int mouseX, int mouseY, float partialTicks)
    {
        GLManager.GL.Disable(GLConst.AlphaTest);
        RenderSkybox(partialTicks);
        GLManager.GL.Enable(GLConst.AlphaTest);
        DrawGradientRect(0, 0, Width, Height, (Color)(-2130706433), (Color)16777215);
        DrawGradientRect(0, 0, Width, Height, (Color)0, (Color)int.MinValue);
        Tessellator tess = Tessellator.instance;
        short logoWidth = 274;
        int logoX = Width / 2 - logoWidth / 2;
        byte logoY = 30;
        Game.textureManager.BindTexture(Game.textureManager.GetTextureId("/title/mclogo.png"));
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        DrawTexturedModalRect(logoX + 0, logoY + 0, 0, 0, 155, 44);
        DrawTexturedModalRect(logoX + 155, logoY + 0, 0, 45, 155, 44);
        tess.setColorOpaque_I(0xFFFFFF);
        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(Width / 2 + 90, 70.0F, 0.0F);
        GLManager.GL.Rotate(-20.0F, 0.0F, 0.0F, 1.0F);
        float splashScale = 1.8F - MathHelper.Abs(MathHelper.Sin(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
 % 1000L / 1000.0F * (float)Math.PI * 2.0F) * 0.1F);
        splashScale = splashScale * 100.0F / (FontRenderer.GetStringWidth(_splashText) + 32);
        GLManager.GL.Scale(splashScale, splashScale, splashScale);
        DrawCenteredString(FontRenderer, _splashText, 0, -8, Color.Yellow);
        GLManager.GL.PopMatrix();
        string text = "BetaSharp " + BetaSharp.Version;
        int width = FontRenderer.GetStringWidth(text);
        bool mouseOver = mouseX >= 2 && mouseY >= 2 && mouseX <= 2 + width && mouseY <= 9;
        DrawString(FontRenderer, text, 2, 2, mouseOver ? Color.HoverYellow : Color.White);
        string copyrightText = "Copyright Mojang Studios. Not an official Minecraft product.";
        DrawString(FontRenderer, copyrightText, Width - FontRenderer.GetStringWidth(copyrightText) - 2, Height - 20, Color.White);
        string disclaimerText = "Not approved by or associated with Mojang Studios or Microsoft.";
        DrawString(FontRenderer, disclaimerText, Width - FontRenderer.GetStringWidth(disclaimerText) - 2, Height - 10, Color.White);
        base.Render(mouseX, mouseY, partialTicks);
    }

    private void EnsureBackgroundTexture()
    {
        if (_backgroundTexture != null)
        {
            return;
        }

        _backgroundTexture = new GLTexture("main_menu_panorama");
        unsafe
        {
            _backgroundTexture.Upload(256, 256, null, 0, Silk.NET.OpenGL.PixelFormat.Rgba, Silk.NET.OpenGL.InternalFormat.Rgba);
        }
        _backgroundTexture.SetFilter(Silk.NET.OpenGL.TextureMinFilter.Linear, Silk.NET.OpenGL.TextureMagFilter.Linear);
        _backgroundTexture.SetWrap(Silk.NET.OpenGL.TextureWrapMode.ClampToEdge, Silk.NET.OpenGL.TextureWrapMode.ClampToEdge);
    }

    private void RenderSkybox(float partialTicks)
    {
        EnsureBackgroundTexture();

        Framebuffer.Unbind();
        GLManager.GL.Viewport(0, 0, 256, 256);
        GLManager.GL.ClearColor(0.0F, 0.0F, 0.0F, 0.0F);
        GLManager.GL.Clear(Silk.NET.OpenGL.ClearBufferMask.ColorBufferBit | Silk.NET.OpenGL.ClearBufferMask.DepthBufferBit);
        DrawPanorama(partialTicks);
        for (int i = 0; i < 7; ++i)
        {
            RotateAndBlurSkybox();
        }

        Game.PostProcessManager.BindMainFramebuffer();
        GLManager.GL.Viewport(0, 0, (uint)Display.getFramebufferWidth(), (uint)Display.getFramebufferHeight());

        float scale = Width > Height ? 120.0F / Width : 120.0F / Height;
        float texV = Height * scale / 256.0F;
        float texU = Width * scale / 256.0F;

        _backgroundTexture!.Bind();
        Tessellator tess = Tessellator.instance;
        tess.startDrawingQuads();
        tess.setColorOpaque_F(1.0F, 1.0F, 1.0F);
        tess.addVertexWithUV(0.0D, Height, _zLevel, 0.5F - texV, 0.5F + texU);
        tess.addVertexWithUV(Width, Height, _zLevel, 0.5F - texV, 0.5F - texU);
        tess.addVertexWithUV(Width, 0.0D, _zLevel, 0.5F + texV, 0.5F - texU);
        tess.addVertexWithUV(0.0D, 0.0D, _zLevel, 0.5F + texV, 0.5F + texU);
        tess.draw();
    }

    private void DrawPanorama(float partialTicks)
    {
        GLManager.GL.MatrixMode(GLConst.Projection);
        GLManager.GL.PushMatrix();
        GLManager.GL.LoadIdentity();
        GLU.gluPerspective(120.0F, 1.0F, 0.05F, 10.0F);

        GLManager.GL.MatrixMode(GLConst.Modelview);
        GLManager.GL.PushMatrix();
        GLManager.GL.LoadIdentity();
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        GLManager.GL.Rotate(180.0F, 1.0F, 0.0F, 0.0F);
        GLManager.GL.Rotate(90.0F, 0.0F, 0.0F, 1.0F);
        GLManager.GL.Enable(GLConst.Blend);
        GLManager.GL.Disable(GLConst.AlphaTest);
        GLManager.GL.Disable(GLConst.CullFace);
        GLManager.GL.DepthMask(false);
        GLManager.GL.BlendFunc(GLConst.SrcAlpha, GLConst.OneMinusSrcAlpha);

        Tessellator tess = Tessellator.instance;
        const int passes = 8;
        for (int pass = 0; pass < passes * passes; ++pass)
        {
            GLManager.GL.PushMatrix();
            float offsetX = ((pass % passes) / (float)passes - 0.5F) / 64.0F;
            float offsetY = ((pass / passes) / (float)passes - 0.5F) / 64.0F;
            GLManager.GL.Translate(offsetX, offsetY, 0.0F);
            GLManager.GL.Rotate(MathHelper.Sin((_panoramaTimer + partialTicks) / 400.0F) * 25.0F + 20.0F, 1.0F, 0.0F, 0.0F);
            GLManager.GL.Rotate(-(_panoramaTimer + partialTicks) * 0.1F, 0.0F, 1.0F, 0.0F);

            for (int face = 0; face < 6; ++face)
            {
                GLManager.GL.PushMatrix();
                ApplyPanoramaFaceRotation(face);
                Game.textureManager.BindTexture(Game.textureManager.GetTextureId(s_panoramaPaths[face]));
                tess.startDrawingQuads();
                tess.setColorRGBA(255, 255, 255, 255 / (pass + 1));
                tess.addVertexWithUV(-1.0D, -1.0D, 1.0D, 0.0D, 0.0D);
                tess.addVertexWithUV(1.0D, -1.0D, 1.0D, 1.0D, 0.0D);
                tess.addVertexWithUV(1.0D, 1.0D, 1.0D, 1.0D, 1.0D);
                tess.addVertexWithUV(-1.0D, 1.0D, 1.0D, 0.0D, 1.0D);
                tess.draw();
                GLManager.GL.PopMatrix();
            }

            GLManager.GL.PopMatrix();
            GLManager.GL.ColorMask(true, true, true, false);
        }

        GLManager.GL.ColorMask(true, true, true, true);
        GLManager.GL.DepthMask(true);
        GLManager.GL.Enable(GLConst.CullFace);
        GLManager.GL.Enable(GLConst.AlphaTest);
        GLManager.GL.Disable(GLConst.Blend);
        GLManager.GL.Enable(GLConst.DepthTest);

        GLManager.GL.MatrixMode(GLConst.Projection);
        GLManager.GL.PopMatrix();
        GLManager.GL.MatrixMode(GLConst.Modelview);
        GLManager.GL.PopMatrix();
    }

    private static void ApplyPanoramaFaceRotation(int face)
    {
        if (face == 1)
        {
            GLManager.GL.Rotate(90.0F, 0.0F, 1.0F, 0.0F);
        }
        else if (face == 2)
        {
            GLManager.GL.Rotate(180.0F, 0.0F, 1.0F, 0.0F);
        }
        else if (face == 3)
        {
            GLManager.GL.Rotate(-90.0F, 0.0F, 1.0F, 0.0F);
        }
        else if (face == 4)
        {
            GLManager.GL.Rotate(90.0F, 1.0F, 0.0F, 0.0F);
        }
        else if (face == 5)
        {
            GLManager.GL.Rotate(-90.0F, 1.0F, 0.0F, 0.0F);
        }
    }

    private void RotateAndBlurSkybox()
    {
        _backgroundTexture!.Bind();
        _backgroundTexture.SetFilter(Silk.NET.OpenGL.TextureMinFilter.Linear, Silk.NET.OpenGL.TextureMagFilter.Linear);
        GLManager.GL.CopyTexSubImage2D(GLConst.Texture2D, 0, 0, 0, 0, 0, 256, 256);
        GLManager.GL.Enable(GLConst.Blend);
        GLManager.GL.BlendFunc(GLConst.SrcAlpha, GLConst.OneMinusSrcAlpha);
        GLManager.GL.ColorMask(true, true, true, false);
        GLManager.GL.Disable(GLConst.AlphaTest);

        Tessellator tess = Tessellator.instance;
        tess.startDrawingQuads();
        for (int i = 0; i < 3; ++i)
        {
            float alpha = 1.0F / (i + 1);
            float offset = (i - 1) / 256.0F;
            tess.setColorRGBA_F(1.0F, 1.0F, 1.0F, alpha);
            tess.addVertexWithUV(Width, Height, _zLevel, 0.0F + offset, 1.0F);
            tess.addVertexWithUV(Width, 0.0D, _zLevel, 1.0F + offset, 1.0F);
            tess.addVertexWithUV(0.0D, 0.0D, _zLevel, 1.0F + offset, 0.0F);
            tess.addVertexWithUV(0.0D, Height, _zLevel, 0.0F + offset, 0.0F);
        }
        tess.draw();

        GLManager.GL.Enable(GLConst.AlphaTest);
        GLManager.GL.ColorMask(true, true, true, true);
        GLManager.GL.Disable(GLConst.Blend);
    }

    protected override void MouseClicked(int mouseX, int mouseY, int button)
    {
        string text = "BetaSharp " + BetaSharp.Version;
        int width = FontRenderer.GetStringWidth(text);
        bool mouseOver = mouseX >= 2 && mouseY >= 2 && mouseX <= 2 + width && mouseY <= 9;
        if (mouseOver)
        {
            var ps = new System.Diagnostics.ProcessStartInfo("https://github.com/betasharp-official/betasharp")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            System.Diagnostics.Process.Start(ps);
        }

        base.MouseClicked(mouseX, mouseY, button);
    }
}
