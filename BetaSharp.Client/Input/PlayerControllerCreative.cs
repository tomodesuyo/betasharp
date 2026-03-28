using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;
using WorldGameMode = BetaSharp.Worlds.Core.Systems.GameMode;

namespace BetaSharp.Client.Input;

public class PlayerControllerCreative : PlayerController {
	private int field_35647_c;

	public PlayerControllerCreative(BetaSharp var1) : base(var1) {
		this.IsTestPlayer = true;
	}

	public static void enableAbilities(EntityPlayer var0) {
		var0.SetGameMode(WorldGameMode.Creative);
	}

	public static void disableAbilities(EntityPlayer var0) {
		var0.SetGameMode(WorldGameMode.Survival);
	}

	public override void fillHotbar(EntityPlayer var1) {
		enableAbilities(var1);

		for(int var2 = 0; var2 < 9; ++var2) {
			if(var1.inventory.main[var2] == null) {
				var1.inventory.main[var2] = new ItemStack((Block)Session.RegisteredBlocksList[var2]);
			}
		}

	}

	public static bool IsBlockBreakingRestricted(BetaSharp game) {
		ItemStack? selectedItem = game.player?.inventory.getSelectedItem();
		if(selectedItem == null) {
			return false;
		}

		int itemId = selectedItem.getItem().id;
		return itemId == Item.WoodenSword.id
			|| itemId == Item.StoneSword.id
			|| itemId == Item.IronSword.id
			|| itemId == Item.DiamondSword.id
			|| itemId == Item.GoldenSword.id;
	}

	public static void clickBlockCreative(BetaSharp var0, PlayerController var1, int var2, int var3, int var4, int var5) {
		if(IsBlockBreakingRestricted(var0)) {
			return;
		}

		var0.world.ExtinguishFire(var0.player, var2, var3, var4, var5);
		var1.sendBlockRemoved(var2, var3, var4, var5);

	}

	public bool onPlayerRightClick(EntityPlayer player, World world, ItemStack stack, int x, int y, int z, int side) {
		int blockId = Game.world.Reader.GetBlockId(x, y, z);
		if(blockId > 0 && Block.Blocks[blockId].onUse(new OnUseEvent(world, player, x, y, z))) {
			return true;
		} else if(stack == null) {
			return false;
		} else {
			int var9 = stack.getDamage();
			int var10 = stack.count;
			bool var11 = stack.useOnBlock(player, world, x, y, z, side);
			stack.setDamage(var9);
			stack.count = var10;
			return var11;
		}
	}

	public override void clickBlock(int var1, int var2, int var3, int var4) {
		clickBlockCreative(this.Game, this, var1, var2, var3, var4);
		this.field_35647_c = 5;
	}

	public void onPlayerDamageBlock(int var1, int var2, int var3, int var4) {
		if(IsBlockBreakingRestricted(Game)) {
			return;
		}

		--this.field_35647_c;
		if(this.field_35647_c <= 0) {
			this.field_35647_c = 5;
			clickBlockCreative(this.Game, this, var1, var2, var3, var4);
		}

	}

	public override void resetBlockRemoving() {
	}

	public override bool shouldDrawHUD() {
		return false;
	}

	public void onWorldChange(World var1) {
		base.func_717_a(var1);
	}

	public override float getBlockReachDistance() {
		return 5.0F;
	}

	public bool isNotCreative() {
		return false;
	}

	public override bool isInCreativeMode() {
		return true;
	}

	public override int getGameMode() {
		return WorldGameMode.Creative;
	}

	public override bool extendedReach() {
		return true;
	}
}
