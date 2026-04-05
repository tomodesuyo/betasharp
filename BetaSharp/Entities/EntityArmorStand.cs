using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityArmorStand : EntityLiving, SpawnableEntity
{
    public readonly record struct ArmorStandPose(float X, float Y, float Z);

    private static readonly ArmorStandPose s_defaultHeadRotation = new(0.0F, 0.0F, 0.0F);
    private static readonly ArmorStandPose s_defaultBodyRotation = new(0.0F, 0.0F, 0.0F);
    private static readonly ArmorStandPose s_defaultLeftArmRotation = new(-10.0F, 0.0F, -10.0F);
    private static readonly ArmorStandPose s_defaultRightArmRotation = new(-15.0F, 0.0F, 10.0F);
    private static readonly ArmorStandPose s_defaultLeftLegRotation = new(-1.0F, 0.0F, -1.0F);
    private static readonly ArmorStandPose s_defaultRightLegRotation = new(1.0F, 0.0F, 1.0F);

    public override EntityType Type => EntityRegistry.ArmorStand;

    private readonly ItemStack?[] _equipment = [null, null, null, null, null];
    private readonly SyncedProperty<int>[] _syncedEquipmentIds;
    private readonly SyncedProperty<int>[] _syncedEquipmentDamage;
    private readonly SyncedProperty<bool> _showArms;
    private ArmorStandPose _headRotation = s_defaultHeadRotation;
    private ArmorStandPose _bodyRotation = s_defaultBodyRotation;
    private ArmorStandPose _leftArmRotation = s_defaultLeftArmRotation;
    private ArmorStandPose _rightArmRotation = s_defaultRightArmRotation;
    private ArmorStandPose _leftLegRotation = s_defaultLeftLegRotation;
    private ArmorStandPose _rightLegRotation = s_defaultRightLegRotation;

    public EntityArmorStand(IWorldContext world) : base(world)
    {
        texture = "/mob/armorstand/wood.png";
        setBoundingBoxSpacing(0.5F, 1.975F);
        maxHealth = 10;
        health = 10;
        pushSpeedReduction = 1.0F;
        _syncedEquipmentIds =
        [
            DataSynchronizer.MakeProperty(16, -1),
            DataSynchronizer.MakeProperty(17, -1),
            DataSynchronizer.MakeProperty(18, -1),
            DataSynchronizer.MakeProperty(19, -1),
            DataSynchronizer.MakeProperty(20, -1)
        ];
        _syncedEquipmentDamage =
        [
            DataSynchronizer.MakeProperty(21, 0),
            DataSynchronizer.MakeProperty(22, 0),
            DataSynchronizer.MakeProperty(23, 0),
            DataSynchronizer.MakeProperty(24, 0),
            DataSynchronizer.MakeProperty(25, 0)
        ];
        _showArms = DataSynchronizer.MakeProperty(26, false);
    }

    protected override bool canDespawn() => false;

    public override bool isPushable() => false;

    public override void tickMovement()
    {
        if (world.IsRemote)
        {
            int minChunkX = MathHelper.Floor(boundingBox.MinX) >> 4;
            int maxChunkX = MathHelper.Floor(boundingBox.MaxX) >> 4;
            int minChunkZ = MathHelper.Floor(boundingBox.MinZ) >> 4;
            int maxChunkZ = MathHelper.Floor(boundingBox.MaxZ) >> 4;

            for (int chunkX = minChunkX; chunkX <= maxChunkX; ++chunkX)
            {
                for (int chunkZ = minChunkZ; chunkZ <= maxChunkZ; ++chunkZ)
                {
                    if (!world.ChunkHost.GetChunk(chunkX, chunkZ).Loaded)
                    {
                        velocityX = velocityY = velocityZ = 0.0D;
                        return;
                    }
                }
            }
        }

        if (newPosRotationIncrements > 0)
        {
            double newX = x + (newPosX - x) / newPosRotationIncrements;
            double newY = y + (newPosY - y) / newPosRotationIncrements;
            double newZ = z + (newPosZ - z) / newPosRotationIncrements;
            double yawDelta = newRotationYaw - yaw;

            while (yawDelta < -180.0D)
            {
                yawDelta += 360.0D;
            }

            while (yawDelta >= 180.0D)
            {
                yawDelta -= 360.0D;
            }

            yaw = (float)(yaw + yawDelta / newPosRotationIncrements);
            pitch = (float)(pitch + (newRotationPitch - pitch) / newPosRotationIncrements);
            --newPosRotationIncrements;
            setPosition(newX, newY, newZ);
            setRotation(yaw, pitch);
        }

        tickLiving();
        sidewaysSpeed *= 0.98F;
        forwardSpeed *= 0.98F;
        rotationSpeed *= 0.9F;
        travel(sidewaysSpeed, forwardSpeed);
    }

    public override void tickLiving()
    {
        ApplySyncedEquipment();
        forwardSpeed = 0.0F;
        sidewaysSpeed = 0.0F;
        jumping = false;
        rotationSpeed = 0.0F;
        velocityX = 0.0D;
        velocityZ = 0.0D;
        bodyYaw = yaw;
        lastBodyYaw = yaw;
    }

    public override bool damage(Entity entity, int amount)
    {
        if (world.IsRemote || dead || amount <= 0)
        {
            return false;
        }

        dropFewItems();
        markDead();
        return true;
    }

    public override ItemStack[] getEquipment()
    {
        ApplySyncedEquipment();
        return _equipment;
    }

    public override void setEquipmentStack(int slot, int itemRawId, int itemDamage)
    {
        if ((uint)slot >= _equipment.Length)
        {
            return;
        }

        _equipment[slot] = itemRawId < 0 ? null : new ItemStack(itemRawId, 1, itemDamage);
        _syncedEquipmentIds[slot].Value = itemRawId;
        _syncedEquipmentDamage[slot].Value = itemDamage;
        UpdateArmVisibility();
    }

    public override bool interact(EntityPlayer player)
    {
        if (world.IsRemote)
        {
            return true;
        }

        ItemStack? heldStack = player.getHand();
        int slot = heldStack != null ? GetPreferredSlot(heldStack) : GetClickedSlot(player);
        if ((uint)slot >= _equipment.Length)
        {
            return false;
        }

        SwapEquipment(player, slot, heldStack);
        player.inventory.markDirty();
        player.currentScreenHandler.SendContentUpdates();
        return true;
    }

    protected override string getLivingSound() => "none";

    protected override void dropFewItems()
    {
        for (int slot = 0; slot < _equipment.Length; ++slot)
        {
            if (_equipment[slot] != null)
            {
                dropItem(_equipment[slot]!.copy(), 0.0F);
            }
        }

        dropItem(Item.ArmorStand.id, 1);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);

        for (int slot = 0; slot < _equipment.Length; ++slot)
        {
            _equipment[slot] = null;
            SyncEquipmentSlot(slot, null);
        }

        NBTTagList equipment = nbt.GetTagList("Equipment");
        for (int i = 0; i < equipment.TagCount(); ++i)
        {
            if (equipment.TagAt(i) is not NBTTagCompound itemTag)
            {
                continue;
            }

            int slot = itemTag.GetByte("Slot") & 255;
            if ((uint)slot >= _equipment.Length)
            {
                continue;
            }

            ItemStack stack = new(itemTag);
            _equipment[slot] = stack;
            SyncEquipmentSlot(slot, stack);
        }

        _showArms.Value = nbt.GetBoolean("ShowArms");
        _headRotation = ReadPose(nbt, "Head", s_defaultHeadRotation);
        _bodyRotation = ReadPose(nbt, "Body", s_defaultBodyRotation);
        _leftArmRotation = ReadPose(nbt, "LeftArm", s_defaultLeftArmRotation);
        _rightArmRotation = ReadPose(nbt, "RightArm", s_defaultRightArmRotation);
        _leftLegRotation = ReadPose(nbt, "LeftLeg", s_defaultLeftLegRotation);
        _rightLegRotation = ReadPose(nbt, "RightLeg", s_defaultRightLegRotation);
        UpdateArmVisibility();
    }

    public bool GetShowArms() => _showArms.Value;

    public bool HasNoBasePlate() => false;

    public ArmorStandPose GetHeadRotation() => _headRotation;

    public ArmorStandPose GetBodyRotation() => _bodyRotation;

    public ArmorStandPose GetLeftArmRotation() => _leftArmRotation;

    public ArmorStandPose GetRightArmRotation() => _rightArmRotation;

    public ArmorStandPose GetLeftLegRotation() => _leftLegRotation;

    public ArmorStandPose GetRightLegRotation() => _rightLegRotation;

    public void ResetPose()
    {
        _headRotation = s_defaultHeadRotation;
        _bodyRotation = s_defaultBodyRotation;
        _leftArmRotation = s_defaultLeftArmRotation;
        _rightArmRotation = s_defaultRightArmRotation;
        _leftLegRotation = s_defaultLeftLegRotation;
        _rightLegRotation = s_defaultRightLegRotation;
    }

    public ItemStack? GetEquipmentStack(int slot)
    {
        ApplySyncedEquipment();
        return (uint)slot < _equipment.Length ? _equipment[slot] : null;
    }

    private void ApplySyncedEquipment()
    {
        if (!world.IsRemote)
        {
            return;
        }

        for (int slot = 0; slot < _equipment.Length; ++slot)
        {
            int itemId = _syncedEquipmentIds[slot].Value;
            int itemDamage = _syncedEquipmentDamage[slot].Value;
            if (itemId < 0)
            {
                _equipment[slot] = null;
                continue;
            }

            ItemStack? current = _equipment[slot];
            if (current == null || current.itemId != itemId || current.getDamage() != itemDamage)
            {
                _equipment[slot] = new ItemStack(itemId, 1, itemDamage);
            }
        }

        UpdateArmVisibility();
    }

    private void SwapEquipment(EntityPlayer player, int slot, ItemStack? heldStack)
    {
        ItemStack? standStack = _equipment[slot];
        if (heldStack == null)
        {
            if (standStack == null)
            {
                return;
            }

            _equipment[slot] = null;
            player.inventory.setStack(player.inventory.selectedSlot, standStack);
            SyncEquipmentSlot(slot, null);
            UpdateArmVisibility();
            return;
        }

        if (standStack == null)
        {
            ItemStack placed = heldStack.copy();
            placed.count = 1;
            _equipment[slot] = placed;
            SyncEquipmentSlot(slot, placed);
            if (!player.capabilities.IsCreativeMode)
            {
                if (heldStack.count > 1)
                {
                    heldStack.count--;
                }
                else
                {
                    player.clearStackInHand();
                }
            }

            UpdateArmVisibility();
            return;
        }

        player.inventory.setStack(player.inventory.selectedSlot, standStack);
        ItemStack swapped = heldStack.copy();
        swapped.count = 1;
        _equipment[slot] = swapped;
        SyncEquipmentSlot(slot, swapped);
        UpdateArmVisibility();
    }

    private void SyncEquipmentSlot(int slot, ItemStack? stack)
    {
        _syncedEquipmentIds[slot].Value = stack?.itemId ?? -1;
        _syncedEquipmentDamage[slot].Value = stack?.getDamage() ?? 0;
    }

    private int GetClickedSlot(EntityPlayer player)
    {
        double relativeY = player.y + player.getStandingEyeHeight() - y;
        if (relativeY >= 1.6D && _equipment[4] != null)
        {
            return 4;
        }

        if (relativeY >= 0.9D && relativeY < 1.6D && _equipment[3] != null)
        {
            return 3;
        }

        if (relativeY >= 0.4D && relativeY < 1.4D && _equipment[2] != null)
        {
            return 2;
        }

        if (relativeY >= 0.1D && relativeY < 0.55D && _equipment[1] != null)
        {
            return 1;
        }

        return _equipment[0] != null ? 0 : 4;
    }

    private static int GetPreferredSlot(ItemStack stack)
    {
        Item item = stack.getItem();
        if (item is ItemArmor armor)
        {
            return armor.armorType switch
            {
                0 => 4,
                1 => 3,
                2 => 2,
                3 => 1,
                _ => 0
            };
        }

        if (item.id == Item.Elytra.id)
        {
            return 3;
        }

        if (item.id == Item.Skull.id || item.id == Block.Pumpkin.id || item.id == Block.JackLantern.id)
        {
            return 4;
        }

        return 0;
    }

    private void UpdateArmVisibility()
    {
        _showArms.Value = _equipment[0] != null;
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        NBTTagList equipment = new();
        for (int slot = 0; slot < _equipment.Length; ++slot)
        {
            ItemStack? stack = _equipment[slot];
            if (stack == null)
            {
                continue;
            }

            NBTTagCompound itemTag = new();
            itemTag.SetByte("Slot", (sbyte)slot);
            stack.writeToNBT(itemTag);
            equipment.SetTag(itemTag);
        }

        nbt.SetTag("Equipment", equipment);
        nbt.SetBoolean("ShowArms", _showArms.Value);
        WritePose(nbt, "Head", _headRotation, s_defaultHeadRotation);
        WritePose(nbt, "Body", _bodyRotation, s_defaultBodyRotation);
        WritePose(nbt, "LeftArm", _leftArmRotation, s_defaultLeftArmRotation);
        WritePose(nbt, "RightArm", _rightArmRotation, s_defaultRightArmRotation);
        WritePose(nbt, "LeftLeg", _leftLegRotation, s_defaultLeftLegRotation);
        WritePose(nbt, "RightLeg", _rightLegRotation, s_defaultRightLegRotation);
    }

    private static void WritePose(NBTTagCompound nbt, string key, ArmorStandPose pose, ArmorStandPose defaultPose)
    {
        if (pose.Equals(defaultPose))
        {
            return;
        }

        NBTTagList list = new();
        list.SetTag(new NBTTagFloat(pose.X));
        list.SetTag(new NBTTagFloat(pose.Y));
        list.SetTag(new NBTTagFloat(pose.Z));
        nbt.SetTag(key, list);
    }

    private static ArmorStandPose ReadPose(NBTTagCompound nbt, string key, ArmorStandPose defaultPose)
    {
        if (!nbt.HasKey(key))
        {
            return defaultPose;
        }

        NBTTagList list = nbt.GetTagList(key);
        if (list.TagCount() < 3)
        {
            return defaultPose;
        }

        return new ArmorStandPose(
            ((NBTTagFloat)list.TagAt(0)).Value,
            ((NBTTagFloat)list.TagAt(1)).Value,
            ((NBTTagFloat)list.TagAt(2)).Value);
    }
}
