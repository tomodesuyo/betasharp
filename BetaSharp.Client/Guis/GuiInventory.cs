using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Core.Textures;
using BetaSharp.Client.Rendering.Entities;
using BetaSharp.Entities;

namespace BetaSharp.Client.Guis;

public class GuiInventory : GuiContainer
{

    private float _mouseX;
    private float _mouseY;

    public GuiInventory(EntityPlayer player) : base(player.playerScreenHandler)
    {
        AllowUserInput = true;
        player.increaseStat(global::BetaSharp.Achievements.OpenInventory, 1);
    }

    public override void InitGui()
    {
        if (Game.playerController.isInCreativeMode())
        {
            Game.displayGuiScreen(new GuiContainerCreative(Game.player));
            return;
        }

        _controlList.Clear();
    }

    public override void UpdateScreen()
    {
        base.UpdateScreen();
        if (Game.playerController.isInCreativeMode())
        {
            Game.displayGuiScreen(new GuiContainerCreative(Game.player));
        }
    }

    protected override void DrawGuiContainerForegroundLayer()
    {
        FontRenderer.DrawString(TranslationStorage.Instance.TranslateKey("container.crafting"), 86, 16, Color.Gray40);
    }

    public override void Render(int mouseX, int mouseY, float partialTicks)
    {
        base.Render(mouseX, mouseY, partialTicks);
        _mouseX = mouseX;
        _mouseY = mouseY;
    }

    protected override void DrawGuiContainerBackgroundLayer(float partialTicks)
    {
        TextureHandle texture = Game.textureManager.GetTextureId("/gui/inventory.png");
        GLManager.GL.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        Game.textureManager.BindTexture(texture);

        int guiLeft = (Width - _xSize) / 2;
        int guiTop = (Height - _ySize) / 2;

        DrawTexturedModalRect(guiLeft, guiTop, 0, 0, _xSize, _ySize);
        DrawPlayerOnGui(Game, guiLeft + 51, guiTop + 75, 30, guiLeft + 51 - _mouseX, guiTop + 25 - _mouseY);
    }

    public static void DrawPlayerOnGui(BetaSharp game, int x, int y, int scale, float lookX, float lookY)
    {
        GLManager.GL.Enable(GLEnum.RescaleNormal);
        GLManager.GL.Enable(GLEnum.ColorMaterial);
        GLManager.GL.PushMatrix();
        GLManager.GL.Translate(x, y, 50.0F);
        GLManager.GL.Scale(-scale, scale, scale);
        GLManager.GL.Rotate(180.0F, 0.0F, 0.0F, 1.0F);

        float bodyYaw = game.player.bodyYaw;
        float headYaw = game.player.yaw;
        float headPitch = game.player.pitch;

        GLManager.GL.Rotate(135.0F, 0.0F, 1.0F, 0.0F);
        Lighting.turnOn();
        GLManager.GL.Rotate(-135.0F, 0.0F, 1.0F, 0.0F);
        GLManager.GL.Rotate(-(float)Math.Atan(lookY / 40.0F) * 20.0F, 1.0F, 0.0F, 0.0F);

        game.player.bodyYaw = (float)Math.Atan(lookX / 40.0F) * 20.0F;
        game.player.yaw = (float)Math.Atan(lookX / 40.0F) * 40.0F;
        game.player.pitch = -(float)Math.Atan(lookY / 40.0F) * 20.0F;
        game.player.minBrightness = 1.0F;

        GLManager.GL.Translate(0.0F, game.player.standingEyeHeight, 0.0F);
        EntityRenderDispatcher.instance.playerViewY = 180.0F;
        EntityRenderDispatcher.instance.renderEntityWithPosYaw(game.player, 0.0D, 0.0D, 0.0D, 0.0F, 1.0F);

        game.player.minBrightness = 0.0F;
        game.player.bodyYaw = bodyYaw;
        game.player.yaw = headYaw;
        game.player.pitch = headPitch;

        GLManager.GL.PopMatrix();
        Lighting.turnOff();
        GLManager.GL.Disable(GLEnum.RescaleNormal);
    }

    protected override void ActionPerformed(GuiButton btt)
    {
        if (btt.Id == 0)
        {
            Game.displayGuiScreen(new GuiAchievements(Game.statFileWriter));
        }

        if (btt.Id == 1)
        {
            Game.displayGuiScreen(new GuiStats(this, Game.statFileWriter));
        }

    }

}
