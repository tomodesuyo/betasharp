using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityFireworkRocket : Entity, SpawnableEntity
{
    public override EntityType Type => EntityRegistry.FireworkRocket;

    private int _life;
    private int _lifetime;
    private bool _playedLaunchSound;
    private readonly SyncedProperty<int> _boostedEntityId;
    private EntityLiving? _boostedEntity;

    public EntityFireworkRocket(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(0.25F, 0.25F);
        standingEyeHeight = height / 2.0F;
        preventEntitySpawning = true;
        keepVelocityOnCollision = false;
        _boostedEntityId = DataSynchronizer.MakeProperty(16, -1);
        _lifetime = 30 + random.NextInt(12);
    }

    public EntityFireworkRocket(IWorldContext world, double x, double y, double z) : this(world)
    {
        setPosition(x, y, z);
        velocityX = (random.NextDouble() - 0.5D) * 0.02D;
        velocityY = 0.05D;
        velocityZ = (random.NextDouble() - 0.5D) * 0.02D;
    }

    public EntityFireworkRocket(IWorldContext world, EntityLiving boostedEntity) : this(world, boostedEntity.x, boostedEntity.y, boostedEntity.z)
    {
        _boostedEntity = boostedEntity;
        _boostedEntityId.Value = boostedEntity.id;
    }

    public override bool shouldRender(double distance)
    {
        if (IsAttachedToEntity())
        {
            return distance < 4096.0D;
        }

        double range = boundingBox.AverageEdgeLength * 8.0D;
        range *= 64.0D;
        return distance < range * range;
    }

    public override void tick()
    {
        lastTickX = x;
        lastTickY = y;
        lastTickZ = z;
        base.tick();

        if (!_playedLaunchSound && !world.IsRemote)
        {
            world.Broadcaster.PlaySoundAtPos(x, y, z, "random.fuse", 1.0F, 1.0F);
            _playedLaunchSound = true;
        }

        if (IsAttachedToEntity())
        {
            UpdateAttachedBoost();
        }
        else
        {
            velocityX *= 1.15D;
            velocityZ *= 1.15D;
            velocityY += 0.04D;
            move(velocityX, velocityY, velocityZ);
        }

        UpdateRotation();
        world.Broadcaster.AddParticle("smoke", x, y - 0.1D, z, -velocityX * 0.1D, -velocityY * 0.1D, -velocityZ * 0.1D);

        ++_life;
        if (_life > _lifetime || hasCollided)
        {
            Explode();
        }
    }

    private void UpdateAttachedBoost()
    {
        if (_boostedEntity == null)
        {
            _boostedEntity = world.Entities.GetEntityByID(_boostedEntityId.Value) as EntityLiving;
        }

        if (_boostedEntity == null)
        {
            return;
        }

        if (_boostedEntity.dead)
        {
            markDead();
            return;
        }

        if (_boostedEntity is EntityPlayer player && player.IsGlidingWithElytra())
        {
            Vec3D look = _boostedEntity.getLook(1.0F);
            _boostedEntity.velocityX += look.x * 0.1D + (look.x * 1.5D - _boostedEntity.velocityX) * 0.5D;
            _boostedEntity.velocityY += look.y * 0.1D + (look.y * 1.5D - _boostedEntity.velocityY) * 0.5D;
            _boostedEntity.velocityZ += look.z * 0.1D + (look.z * 1.5D - _boostedEntity.velocityZ) * 0.5D;
            _boostedEntity.velocityModified = true;
        }

        setPosition(_boostedEntity.x, _boostedEntity.y, _boostedEntity.z);
        velocityX = _boostedEntity.velocityX;
        velocityY = _boostedEntity.velocityY;
        velocityZ = _boostedEntity.velocityZ;
    }

    private void UpdateRotation()
    {
        float horizontalSpeed = MathHelper.Sqrt(velocityX * velocityX + velocityZ * velocityZ);
        yaw = (float)(System.Math.Atan2(velocityX, velocityZ) * 180.0D / System.Math.PI);
        pitch = (float)(System.Math.Atan2(velocityY, horizontalSpeed) * 180.0D / System.Math.PI);
        while (pitch - prevPitch < -180.0F) prevPitch -= 360.0F;
        while (pitch - prevPitch >= 180.0F) prevPitch += 360.0F;
        while (yaw - prevYaw < -180.0F) prevYaw -= 360.0F;
        while (yaw - prevYaw >= 180.0F) prevYaw += 360.0F;
        pitch = prevPitch + (pitch - prevPitch) * 0.2F;
        yaw = prevYaw + (yaw - prevYaw) * 0.2F;
    }

    private void Explode()
    {
        if (dead)
        {
            return;
        }

        if (!world.IsRemote)
        {
            world.Broadcaster.PlaySoundAtPos(x, y, z, "random.explode", 1.5F, 1.0F + (random.NextFloat() - random.NextFloat()) * 0.2F);
            for (int i = 0; i < 12; ++i)
            {
                double velocityX = random.NextGaussian() * 0.12D;
                double velocityY = random.NextGaussian() * 0.12D;
                double velocityZ = random.NextGaussian() * 0.12D;
                world.Broadcaster.AddParticle("explode", x, y, z, velocityX, velocityY, velocityZ);
                world.Broadcaster.AddParticle("reddust", x, y, z, 1.0D, 0.2D + random.NextDouble() * 0.3D, random.NextDouble());
            }
        }

        markDead();
    }

    private bool IsAttachedToEntity() => _boostedEntityId.Value >= 0 || _boostedEntity != null;

    public override void writeNbt(NBTTagCompound nbt)
    {
        nbt.SetInteger("Life", _life);
        nbt.SetInteger("Lifetime", _lifetime);
        nbt.SetInteger("BoostedEntityId", _boostedEntityId.Value);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        _life = nbt.GetInteger("Life");
        _lifetime = nbt.GetInteger("Lifetime");
        _boostedEntityId.Value = nbt.GetInteger("BoostedEntityId");
    }
}
