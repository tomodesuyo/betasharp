using BetaSharp.Blocks;
using BetaSharp.Client.Entities;
using BetaSharp.Client.Network;
using BetaSharp.Client.Sound;
using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.Network.Packets.C2SPlay;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Client.Input;

public class PlayerControllerMP : PlayerController
{

    private int currentBlockX = -1;
    private int currentBlockY = -1;
    private int currentblockZ = -1;
    private float curBlockDamageMP;
    private float prevBlockDamageMP;
    private float field_9441_h;
    private int blockHitDelay;
    private bool isHittingBlock;
    private readonly ClientNetworkHandler netClientHandler;
    private int currentPlayerItem;
    private int creativeBlockHitDelay;
    private int currentGameMode;

    public PlayerControllerMP(BetaSharp var1, ClientNetworkHandler var2) : base(var1)
    {
        netClientHandler = var2;
    }

    public override void flipPlayer(EntityPlayer playerEntity)
    {
        playerEntity.yaw = -180.0F;
        playerEntity.prevYaw = -180.0F;
    }

    public override bool sendBlockRemoved(int x, int y, int z, int var4)
    {
        if (GameMode.IsSpectator(currentGameMode))
        {
            return false;
        }

        if (GameMode.IsCreative(currentGameMode))
        {
            return false;
        }

        int blockId = Game.world.Reader.GetBlockId(x, y, z);
        bool var6 = base.sendBlockRemoved(x, y, z, var4);
        ItemStack var7 = Game.player.getHand();
        if (var7 != null)
        {
            var7.postMine(blockId, x, y, z, Game.player);
            if (var7.count == 0)
            {
                var7.onRemoved(Game.player);
                Game.player.clearStackInHand();
            }
        }

        return var6;
    }

    public override void clickBlock(int var1, int var2, int var3, int var4)
    {
        if (GameMode.IsSpectator(currentGameMode))
        {
            return;
        }

        if (GameMode.IsCreative(currentGameMode))
        {
            if (PlayerControllerCreative.IsBlockBreakingRestricted(Game))
            {
                return;
            }

            netClientHandler.addToSendQueue(PlayerActionC2SPacket.Get(0, var1, var2, var3, var4));
            PlayerControllerCreative.clickBlockCreative(Game, this, var1, var2, var3, var4);
            creativeBlockHitDelay = 5;
            return;
        }

        if (!isHittingBlock || var1 != currentBlockX || var2 != currentBlockY || var3 != currentblockZ)
        {
            netClientHandler.addToSendQueue(PlayerActionC2SPacket.Get(0, var1, var2, var3, var4));
            int var5 = Game.world.Reader.GetBlockId(var1, var2, var3);
            if (var5 > 0 && curBlockDamageMP == 0.0F)
            {
                Block.Blocks[var5].onBlockBreakStart(new OnBlockBreakStartEvent(Game.world, Game.player, var1, var2, var3));
            }

            if (var5 > 0 && Block.Blocks[var5].getHardness(Game.player) >= 1.0F)
            {
                sendBlockRemoved(var1, var2, var3, var4);
            }
            else
            {
                isHittingBlock = true;
                currentBlockX = var1;
                currentBlockY = var2;
                currentblockZ = var3;
                curBlockDamageMP = 0.0F;
                prevBlockDamageMP = 0.0F;
                field_9441_h = 0.0F;
            }
        }

    }

    public override void resetBlockRemoving()
    {
        curBlockDamageMP = 0.0F;
        isHittingBlock = false;
    }

    public override void sendBlockRemoving(int var1, int var2, int var3, int var4)
    {
        if (GameMode.IsSpectator(currentGameMode))
        {
            return;
        }

        if (GameMode.IsCreative(currentGameMode))
        {
            if (PlayerControllerCreative.IsBlockBreakingRestricted(Game))
            {
                return;
            }

            syncCurrentPlayItem();
            if (creativeBlockHitDelay > 0)
            {
                --creativeBlockHitDelay;
            }
            else
            {
                creativeBlockHitDelay = 5;
                netClientHandler.addToSendQueue(PlayerActionC2SPacket.Get(0, var1, var2, var3, var4));
                PlayerControllerCreative.clickBlockCreative(Game, this, var1, var2, var3, var4);
            }

            return;
        }

        if (isHittingBlock)
        {
            syncCurrentPlayItem();
            if (blockHitDelay > 0)
            {
                --blockHitDelay;
            }
            else
            {
                if (var1 == currentBlockX && var2 == currentBlockY && var3 == currentblockZ)
                {
                    int var5 = Game.world.Reader.GetBlockId(var1, var2, var3);
                    if (var5 == 0)
                    {
                        isHittingBlock = false;
                        return;
                    }

                    Block var6 = Block.Blocks[var5];
                    curBlockDamageMP += var6.getHardness(Game.player);
                    if (field_9441_h % 4.0F == 0.0F && var6 != null)
                    {
                        Game.sndManager.PlaySound(var6.soundGroup.StepSound, (float)var1 + 0.5F, (float)var2 + 0.5F, (float)var3 + 0.5F, (var6.soundGroup.Volume + 1.0F) / 8.0F, var6.soundGroup.Pitch * 0.5F);
                    }

                    ++field_9441_h;
                    if (curBlockDamageMP >= 1.0F)
                    {
                        isHittingBlock = false;
                        netClientHandler.addToSendQueue(PlayerActionC2SPacket.Get(2, var1, var2, var3, var4));
                        sendBlockRemoved(var1, var2, var3, var4);
                        curBlockDamageMP = 0.0F;
                        prevBlockDamageMP = 0.0F;
                        field_9441_h = 0.0F;
                        blockHitDelay = 5;
                    }
                }
                else
                {
                    clickBlock(var1, var2, var3, var4);
                }

            }
        }
    }

    public override void setPartialTime(float var1)
    {
        if (curBlockDamageMP <= 0.0F)
        {
            Game.ingameGUI._damageGuiPartialTime = 0.0F;
            Game.terrainRenderer.damagePartialTime = 0.0F;
        }
        else
        {
            float var2 = prevBlockDamageMP + (curBlockDamageMP - prevBlockDamageMP) * var1;
            Game.ingameGUI._damageGuiPartialTime = var2;
            Game.terrainRenderer.damagePartialTime = var2;
        }

    }

    public override float getBlockReachDistance()
    {
        return GameMode.IsCreative(currentGameMode) || GameMode.IsSpectator(currentGameMode) ? 5.0F : 4.0F;
    }

    public override void func_717_a(World var1)
    {
        base.func_717_a(var1);
    }

    public override void updateController()
    {
        syncCurrentPlayItem();
        prevBlockDamageMP = curBlockDamageMP;
        Game.sndManager.PlayRandomMusicIfReady(DefaultMusicCategories.Game);
    }

    private void syncCurrentPlayItem()
    {
        int var1 = Game.player.inventory.selectedSlot;
        if (var1 != currentPlayerItem)
        {
            currentPlayerItem = var1;
            netClientHandler.addToSendQueue(UpdateSelectedSlotC2SPacket.Get(currentPlayerItem));
        }

    }

    public override bool sendPlaceBlock(
        ClientPlayerEntity player,
        World world,
        ItemStack selectedItem,
        int blockX,
        int blockY,
        int blockZ,
        int blockSide
    )
    {
        syncCurrentPlayItem();
        netClientHandler.addToSendQueue(PlayerInteractBlockC2SPacket.Get(blockX, blockY, blockZ, blockSide, player.inventory.getSelectedItem()));
        if (selectedItem?.getItem() == Item.MonsterPlacer)
        {
            return true;
        }

        bool placed;
        if (GameMode.IsSpectator(currentGameMode))
        {
            return false;
        }

        if (GameMode.IsCreative(currentGameMode) && selectedItem != null)
        {
            int damage = selectedItem.getDamage();
            int count = selectedItem.count;
            placed = base.sendPlaceBlock(player, world, selectedItem, blockX, blockY, blockZ, blockSide);
            selectedItem.setDamage(damage);
            selectedItem.count = count;
        }
        else
        {
            placed = base.sendPlaceBlock(player, world, selectedItem, blockX, blockY, blockZ, blockSide);
        }

        return placed;
    }

    public override bool sendUseItem(EntityPlayer var1, World var2, ItemStack var3)
    {
        if (GameMode.IsSpectator(currentGameMode))
        {
            return false;
        }

        syncCurrentPlayItem();
        netClientHandler.addToSendQueue(PlayerInteractBlockC2SPacket.Get(-1, -1, -1, 255, var1.inventory.getSelectedItem()));
        if (var3.getItem() == Item.MonsterPlacer)
        {
            return true;
        }

        bool var4 = base.sendUseItem(var1, var2, var3);
        return var4;
    }

    public override EntityPlayer createPlayer(World var1)
    {
        EntityClientPlayerMP player = new(Game, var1, Game.session, netClientHandler);
        player.capabilities.SetGameMode(currentGameMode);
        return player;
    }

    public override void attackEntity(EntityPlayer var1, Entity var2)
    {
        syncCurrentPlayItem();
        netClientHandler.addToSendQueue(PlayerInteractEntityC2SPacket.Get(var1.id, var2.id, 1));
        var1.attack(var2);
    }

    public override void interactWithEntity(EntityPlayer var1, Entity var2)
    {
        syncCurrentPlayItem();
        netClientHandler.addToSendQueue(PlayerInteractEntityC2SPacket.Get(var1.id, var2.id, 0));
        var1.interact(var2);
    }

    public override ItemStack func_27174_a(int var1, int var2, int var3, bool var4, EntityPlayer var5)
    {
        short var6 = var5.currentScreenHandler.nextRevision(var5.inventory);
        ItemStack var7 = base.func_27174_a(var1, var2, var3, var4, var5);
        netClientHandler.addToSendQueue(ClickSlotC2SPacket.Get(var1, var2, var3, var4, var7, var6));
        return var7;
    }

    public override void func_20086_a(int var1, EntityPlayer var2)
    {
        if (var1 != -9999)
        {
        }
    }

    public void SetCreativeMode(bool enabled)
    {
        SetGameMode(enabled ? GameMode.Creative : GameMode.Survival);
    }

    public void SetGameMode(int gameMode)
    {
        bool enteringSpectator = !GameMode.IsSpectator(currentGameMode) && GameMode.IsSpectator(gameMode);
        currentGameMode = gameMode;
        if (Game.player != null)
        {
            Game.player.capabilities.SetGameMode(gameMode);
            if (enteringSpectator && Game.player is ClientPlayerEntity clientPlayer)
            {
                clientPlayer.EnterSpectatorModeClient();
            }
        }
    }

    public override bool shouldDrawHUD()
    {
        return !(GameMode.IsCreative(currentGameMode) || GameMode.IsSpectator(currentGameMode));
    }

    public override bool isInCreativeMode()
    {
        return GameMode.IsCreative(currentGameMode);
    }

    public override int getGameMode()
    {
        return currentGameMode;
    }

    public override bool extendedReach()
    {
        return GameMode.IsCreative(currentGameMode) || GameMode.IsSpectator(currentGameMode);
    }

    public void SendCreativeSlotAction(ItemStack? stack, int slot)
    {
        if (!GameMode.IsCreative(currentGameMode))
        {
            return;
        }

        netClientHandler.addToSendQueue(CreativeInventoryActionC2SPacket.Get(slot, stack));
    }

    public void SendCreativeDropAction(ItemStack stack)
    {
        if (!GameMode.IsCreative(currentGameMode))
        {
            return;
        }

        netClientHandler.addToSendQueue(CreativeInventoryActionC2SPacket.Get(-1, stack));
    }
}
