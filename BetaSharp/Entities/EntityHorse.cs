using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Blocks;

namespace BetaSharp.Entities;

public sealed class EntityHorse : EntityAnimal
{
    private static readonly string[] s_horseTextures =
    [
        "/mob/horse/horse_white.png",
        "/mob/horse/horse_creamy.png",
        "/mob/horse/horse_chestnut.png",
        "/mob/horse/horse_brown.png",
        "/mob/horse/horse_black.png",
        "/mob/horse/horse_gray.png",
        "/mob/horse/horse_darkbrown.png",
    ];

    public override EntityType Type => EntityRegistry.Horse;
    public readonly SyncedProperty<bool> Saddled;
    public readonly SyncedProperty<byte> HorseType;
    public readonly SyncedProperty<int> Variant;
    public readonly SyncedProperty<byte> ArmorType;

    public EntityHorse(IWorldContext world) : base(world)
    {
        texture = "/mob/horse/horse_brown.png";
        setBoundingBoxSpacing(1.4F, 1.6F);
        maxHealth = 25;
        health = 25;
        movementSpeed = 0.35F;
        Saddled = DataSynchronizer.MakeProperty(16, false);
        HorseType = DataSynchronizer.MakeProperty<byte>(17, GetRandomHorseType(world));
        Variant = DataSynchronizer.MakeProperty<int>(18, GetRandomHorseVariant(HorseType.Value));
        ArmorType = DataSynchronizer.MakeProperty<byte>(19, 0);
    }

    public override bool interact(EntityPlayer player)
    {
        ItemStack? heldItem = player.inventory.getSelectedItem();
        if (heldItem != null && heldItem.itemId == Item.Lead.id)
        {
            return false;
        }

        if (heldItem != null && heldItem.itemId == Block.Hay.id && health < maxHealth)
        {
            heal(20);
            heldItem.ConsumeItem(player);
            if (heldItem.count <= 0)
            {
                player.clearStackInHand();
            }

            return true;
        }

        if (!Saddled.Value && heldItem != null && heldItem.itemId == Item.Saddle.id)
        {
            Saddled.Value = true;
            heldItem.ConsumeItem(player);
            if (heldItem.count <= 0)
            {
                player.clearStackInHand();
            }

            return true;
        }

        if (heldItem != null && CanWearArmor() && TryGetHorseArmorType(heldItem.itemId, out byte armorType) && ArmorType.Value != armorType)
        {
            if (!world.IsRemote)
            {
                int previousArmorItemId = GetArmorItemId();
                ArmorType.Value = armorType;
                heldItem.ConsumeItem(player);
                if (heldItem.count <= 0)
                {
                    player.clearStackInHand();
                }

                if (previousArmorItemId != 0)
                {
                    dropItem(previousArmorItemId, 1);
                }
            }

            return true;
        }

        if (Saddled.Value)
        {
            if (!world.IsRemote && (passenger == null || passenger == player))
            {
                player.setVehicle(this);
            }

            return true;
        }

        return false;
    }

    public override string getTexture()
    {
        return HorseType.Value switch
        {
            1 => "/mob/horse/donkey.png",
            2 => "/mob/horse/mule.png",
            3 => "/mob/horse/horse_zombie.png",
            4 => "/mob/horse/horse_skeleton.png",
            _ => s_horseTextures[Variant.Value % s_horseTextures.Length],
        };
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetBoolean("Saddle", Saddled.Value);
        nbt.SetByte("Type", (sbyte)HorseType.Value);
        nbt.SetInteger("Variant", Variant.Value);
        nbt.SetByte("ArmorType", (sbyte)ArmorType.Value);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        Saddled.Value = nbt.GetBoolean("Saddle");
        HorseType.Value = (byte)nbt.GetByte("Type");
        Variant.Value = nbt.GetInteger("Variant");
        ArmorType.Value = (byte)nbt.GetByte("ArmorType");
    }

    protected override string getLivingSound()
    {
        return "mob.cow";
    }

    protected override string getHurtSound()
    {
        return "mob.cowhurt";
    }

    protected override string getDeathSound()
    {
        return "mob.cowhurt";
    }

    protected override float getSoundVolume()
    {
        return 0.6F;
    }

    protected override int getDropItemId()
    {
        return Item.Leather.id;
    }

    protected override void dropFewItems()
    {
        int count = 1 + random.NextInt(2);
        for (int i = 0; i < count; ++i)
        {
            dropItem(Item.Leather.id, 1);
        }

        int armorItemId = GetArmorItemId();
        if (armorItemId != 0)
        {
            dropItem(armorItemId, 1);
        }
    }

    public bool CanWearArmor()
    {
        return HorseType.Value != 1 && HorseType.Value != 2;
    }

    public string? GetArmorTexture()
    {
        return ArmorType.Value switch
        {
            1 => "/mob/horse/armor/horse_armor_iron.png",
            2 => "/mob/horse/armor/horse_armor_gold.png",
            3 => "/mob/horse/armor/horse_armor_diamond.png",
            _ => null
        };
    }

    public int GetArmorItemId()
    {
        return ArmorType.Value switch
        {
            1 => Item.IronHorseArmor.id,
            2 => Item.GoldenHorseArmor.id,
            3 => Item.DiamondHorseArmor.id,
            _ => 0
        };
    }

    private static bool TryGetHorseArmorType(int itemId, out byte armorType)
    {
        armorType = itemId switch
        {
            int id when id == Item.IronHorseArmor.id => 1,
            int id when id == Item.GoldenHorseArmor.id => 2,
            int id when id == Item.DiamondHorseArmor.id => 3,
            _ => 0
        };

        return armorType != 0;
    }

    private static byte GetRandomHorseType(IWorldContext world)
    {
        int roll = world.Random.NextInt(100);
        return roll switch
        {
            < 78 => 0,
            < 88 => 1,
            < 94 => 2,
            < 97 => 3,
            _ => 4,
        };
    }

    private static int GetRandomHorseVariant(byte type)
    {
        return type == 0 ? Random.Shared.Next(7) : 0;
    }
}
