using System;
using BetaSharp.NBT;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks.Entities;

public sealed class BlockEntitySkull : BlockEntity
{
    public const int Skeleton = 0;
    public const int WitherSkeleton = 1;
    public const int Zombie = 2;
    public const int Player = 3;
    public const int Creeper = 4;
    public const int Dragon = 5;

    public override BlockEntityType Type => BlockEntity.Skull;

    private int _skullType;
    private int _rotation;
    private int _dragonAnimatedTicks;
    private bool _dragonAnimated;

    public int GetSkullType() => _skullType;

    public void SetSkullType(int skullType)
    {
        _skullType = Math.Clamp(skullType, Skeleton, Dragon);
    }

    public int GetSkullRotation() => _rotation;

    public void SetSkullRotation(int rotation)
    {
        _rotation = rotation & 15;
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetByte("SkullType", (sbyte)_skullType);
        nbt.SetByte("Rot", (sbyte)_rotation);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        SetSkullType(nbt.GetByte("SkullType"));
        SetSkullRotation(nbt.GetByte("Rot"));
    }

    public override void tick(EntityManager entities)
    {
        if (_skullType != Dragon)
        {
            _dragonAnimated = false;
            return;
        }

        if (World.Redstone.IsPowered(X, Y, Z))
        {
            _dragonAnimated = true;
            ++_dragonAnimatedTicks;
        }
        else
        {
            _dragonAnimated = false;
        }
    }

    public float GetAnimationProgress(float tickDelta)
    {
        return _dragonAnimated ? _dragonAnimatedTicks + tickDelta : _dragonAnimatedTicks;
    }
}
