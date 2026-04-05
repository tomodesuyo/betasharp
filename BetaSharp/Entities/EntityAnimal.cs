using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public abstract class EntityAnimal : EntityCreature, SpawnableEntity
{
    private readonly SyncedProperty<int> _syncedLeashHolderId;
    private readonly SyncedProperty<Vec3i> _syncedLeashFencePos;
    private readonly SyncedProperty<byte> _syncedLeashFlags;
    private EntityPlayer? _leashHolder;
    private Vec3i? _leashFencePos;
    private string? _pendingLeashHolderName;

    public EntityAnimal(IWorldContext world) : base(world)
    {
        _syncedLeashHolderId = DataSynchronizer.MakeProperty(20, -1);
        _syncedLeashFencePos = DataSynchronizer.MakeProperty(21, new Vec3i(0, 0, 0));
        _syncedLeashFlags = DataSynchronizer.MakeProperty<byte>(22, 0);
    }

    protected override float getBlockPathWeight(int x, int y, int z)
    {
        return world.Reader.GetBlockId(x, y - 1, z) == Block.GrassBlock.id ? 10.0F : world.Lighting.GetLuminance(x, y, z) - 0.5F;
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        if (_leashHolder != null)
        {
            nbt.SetString("LeashHolder", _leashHolder.name);
        }

        if (_leashFencePos is { } leashFencePos)
        {
            nbt.SetBoolean("LeashedToFence", true);
            nbt.SetInteger("LeashX", leashFencePos.X);
            nbt.SetInteger("LeashY", leashFencePos.Y);
            nbt.SetInteger("LeashZ", leashFencePos.Z);
        }
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        string leashHolderName = nbt.GetString("LeashHolder");
        _pendingLeashHolderName = leashHolderName.Length > 0 ? leashHolderName : null;
        _leashHolder = null;
        _leashFencePos = nbt.GetBoolean("LeashedToFence")
            ? new Vec3i(nbt.GetInteger("LeashX"), nbt.GetInteger("LeashY"), nbt.GetInteger("LeashZ"))
            : null;
        _syncedLeashHolderId.Value = -1;
        _syncedLeashFencePos.Value = _leashFencePos ?? new Vec3i(0, 0, 0);
        _syncedLeashFlags.Value = (byte)(_leashFencePos != null ? 3 : _pendingLeashHolderName != null ? 1 : 0);
    }

    public override void tickMovement()
    {
        base.tickMovement();
        if (!world.IsRemote)
        {
            UpdateLeash();
        }
    }

    public override void onKilledBy(Entity entity)
    {
        if (IsLeashed)
        {
            ClearLeash(true);
        }

        base.onKilledBy(entity);
    }

    public override bool canSpawn()
    {
        int x = MathHelper.Floor(base.x);
        int y = MathHelper.Floor(boundingBox.MinY);
        int z = MathHelper.Floor(base.z);
        return world.Reader.GetBlockId(x, y - 1, z) == Block.GrassBlock.id && world.Reader.GetBrightness(x, y, z) > 8 && base.canSpawn();
    }

    public override int getTalkInterval()
    {
        return 120;
    }

    public bool IsLeashed => (_syncedLeashFlags.Value & 1) != 0 || _leashHolder != null || _leashFencePos != null || _pendingLeashHolderName != null;

    public bool IsLeashedTo(EntityPlayer player)
    {
        return _leashHolder == player;
    }

    public void SetLeashedTo(EntityPlayer player)
    {
        _leashHolder = player;
        _pendingLeashHolderName = null;
        _leashFencePos = null;
        _syncedLeashHolderId.Value = player.id;
        _syncedLeashFencePos.Value = new Vec3i(0, 0, 0);
        _syncedLeashFlags.Value = 1;
    }

    public void AttachLeashToFence(int x, int y, int z)
    {
        _leashHolder = null;
        _pendingLeashHolderName = null;
        _leashFencePos = new Vec3i(x, y, z);
        _syncedLeashHolderId.Value = -1;
        _syncedLeashFencePos.Value = _leashFencePos.Value;
        _syncedLeashFlags.Value = 3;
    }

    public void ClearLeash(bool dropLead)
    {
        _leashHolder = null;
        _pendingLeashHolderName = null;
        _leashFencePos = null;
        _syncedLeashHolderId.Value = -1;
        _syncedLeashFencePos.Value = new Vec3i(0, 0, 0);
        _syncedLeashFlags.Value = 0;
        if (dropLead && !world.IsRemote)
        {
            dropItem(Item.Lead.id, 1);
        }
    }

    public bool TryGetLeashAnchor(float tickDelta, out Vec3D anchor)
    {
        byte leashFlags = _syncedLeashFlags.Value;
        if ((leashFlags & 1) == 0)
        {
            anchor = default;
            return false;
        }

        if ((leashFlags & 2) != 0)
        {
            Vec3i fencePos = _leashFencePos ?? _syncedLeashFencePos.Value;
            anchor = new Vec3D(fencePos.X + 0.5D, fencePos.Y + 0.5D, fencePos.Z + 0.5D);
            return true;
        }

        Entity? holder = _leashHolder ?? world.Entities.GetEntityByID(_syncedLeashHolderId.Value);
        if (holder == null || holder.dead)
        {
            anchor = default;
            return false;
        }

        double holderX = holder.lastTickX + (holder.x - holder.lastTickX) * tickDelta;
        double holderY = holder.lastTickY + (holder.y - holder.lastTickY) * tickDelta + holder.getStandingEyeHeight() * 0.7D;
        double holderZ = holder.lastTickZ + (holder.z - holder.lastTickZ) * tickDelta;
        anchor = new Vec3D(holderX, holderY, holderZ);
        return true;
    }

    private void UpdateLeash()
    {
        if (_pendingLeashHolderName != null)
        {
            EntityPlayer? player = world.Entities.GetPlayer(_pendingLeashHolderName);
            if (player != null)
            {
                _leashHolder = player;
                _pendingLeashHolderName = null;
                _syncedLeashHolderId.Value = player.id;
                _syncedLeashFlags.Value = 1;
            }
        }

        if (_leashHolder == null && _leashFencePos == null)
        {
            return;
        }

        double anchorX;
        double anchorY;
        double anchorZ;
        if (_leashHolder != null)
        {
            if (_leashHolder.dead)
            {
                ClearLeash(true);
                return;
            }

            anchorX = _leashHolder.x;
            anchorY = _leashHolder.y + _leashHolder.getStandingEyeHeight();
            anchorZ = _leashHolder.z;
        }
        else
        {
            Vec3i fencePos = _leashFencePos!.Value;
            int blockId = world.Reader.GetBlockId(fencePos.X, fencePos.Y, fencePos.Z);
            if (blockId != Block.Fence.id && blockId != Block.NetherFence.id)
            {
                ClearLeash(true);
                return;
            }

            anchorX = fencePos.X + 0.5D;
            anchorY = fencePos.Y + 0.5D;
            anchorZ = fencePos.Z + 0.5D;
        }

        double dx = anchorX - x;
        double dy = anchorY - y;
        double dz = anchorZ - z;
        double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
        if (distance > 10.0D)
        {
            ClearLeash(true);
            return;
        }

        if (distance <= 2.0D)
        {
            return;
        }

        double pullStrength = _leashFencePos != null ? 0.03D : 0.02D;
        velocityX += Math.Sign(dx) * dx * dx * pullStrength / distance;
        velocityY += Math.Sign(dy) * dy * dy * 0.02D / distance;
        velocityZ += Math.Sign(dz) * dz * dz * pullStrength / distance;
    }
}
