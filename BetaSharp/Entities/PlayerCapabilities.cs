using BetaSharp.NBT;

namespace BetaSharp.Entities;

public class PlayerCapabilities {
	public bool disableDamage = false;
	public bool isFlying = false;
	public bool allowFlying = false;
	public bool isCreativeMode = false;
	public bool allowEdit = true;
	public int gameMode = Worlds.Core.Systems.GameMode.Survival;
	private float flySpeed = 0.05F;
	private float walkSpeed = 0.1F;

	public void SetCreativeMode(bool enabled) {
		SetGameMode(enabled ? Worlds.Core.Systems.GameMode.Creative : Worlds.Core.Systems.GameMode.Survival);
	}

	public void SetGameMode(int mode) {
		int previousMode = gameMode;
		gameMode = mode;
		if(mode == Worlds.Core.Systems.GameMode.Creative) {
			allowFlying = true;
			isCreativeMode = true;
			disableDamage = true;
			isFlying = false;
			if(previousMode != Worlds.Core.Systems.GameMode.Creative) {
				flySpeed = 0.05F;
				walkSpeed = 0.1F;
			}
		} else if(mode == Worlds.Core.Systems.GameMode.Spectator) {
			allowFlying = true;
			isCreativeMode = false;
			disableDamage = true;
			isFlying = true;
		} else {
			allowFlying = false;
			isCreativeMode = false;
			disableDamage = false;
			isFlying = false;
		}

		allowEdit = !Worlds.Core.Systems.GameMode.IsAdventure(mode);
	}

	public bool IsSpectatorMode => gameMode == Worlds.Core.Systems.GameMode.Spectator;

	public bool IsCreativeMode => gameMode == Worlds.Core.Systems.GameMode.Creative;

	public float GetFlySpeed() => flySpeed;

	public void SetFlySpeed(float speed)
	{
		flySpeed = speed;
	}

	public float GetWalkSpeed() => walkSpeed;

	public void SetWalkSpeed(float speed)
	{
		walkSpeed = speed;
	}

	public void RefreshGameModeFromAbilities() {
		if(isCreativeMode) {
			gameMode = Worlds.Core.Systems.GameMode.Creative;
		} else if(disableDamage && allowFlying) {
			gameMode = Worlds.Core.Systems.GameMode.Spectator;
		} else {
			gameMode = Worlds.Core.Systems.GameMode.Survival;
		}
	}

	public void writeCapabilitiesToNBT(NBTTagCompound var1) {
		NBTTagCompound var2 = new NBTTagCompound();
		var2.SetBoolean("invulnerable", this.disableDamage);
		var2.SetBoolean("flying", this.isFlying);
		var2.SetBoolean("mayfly", this.allowFlying);
		var2.SetBoolean("instabuild", this.isCreativeMode);
		var2.SetBoolean("mayBuild", this.allowEdit);
		var2.SetFloat("flySpeed", this.flySpeed);
		var2.SetFloat("walkSpeed", this.walkSpeed);
		var1.SetTag("abilities", var2);
	}

	public void readCapabilitiesFromNBT(NBTTagCompound var1) {
		if(var1.HasKey("abilities")) {
			NBTTagCompound var2 = var1.GetCompoundTag("abilities");
			this.disableDamage = var2.GetBoolean("invulnerable");
			this.isFlying = var2.GetBoolean("flying");
			this.allowFlying = var2.GetBoolean("mayfly");
			this.isCreativeMode = var2.GetBoolean("instabuild");
			this.allowEdit = !var2.HasKey("mayBuild") || var2.GetBoolean("mayBuild");
			if (var2.HasKey("flySpeed"))
			{
				this.flySpeed = var2.GetFloat("flySpeed");
			}

			if (var2.HasKey("walkSpeed"))
			{
				this.walkSpeed = var2.GetFloat("walkSpeed");
			}

			RefreshGameModeFromAbilities();
		}

	}
}
