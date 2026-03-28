using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityEnderEye : Entity
{
    public override EntityType Type => EntityRegistry.EyeOfEnderSignal;

    private double _targetX;
    private double _targetY;
    private double _targetZ;
    private int _life;
    private bool _dropOrShatter;

    public EntityEnderEye(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(0.25F, 0.25F);
    }

    public EntityEnderEye(IWorldContext world, double x, double y, double z) : this(world)
    {
        setPosition(x, y, z);
        standingEyeHeight = 0.0F;
    }

    public void SetTarget(double targetX, double targetY, double targetZ)
    {
        double dx = targetX - x;
        double dz = targetZ - z;
        float horizontalDistance = MathHelper.Sqrt(dx * dx + dz * dz);
        if (horizontalDistance > 12.0F)
        {
            _targetX = x + dx / horizontalDistance * 12.0D;
            _targetZ = z + dz / horizontalDistance * 12.0D;
            _targetY = y + 8.0D;
        }
        else
        {
            _targetX = targetX;
            _targetY = targetY;
            _targetZ = targetZ;
        }

        _life = 0;
        _dropOrShatter = random.NextInt(5) > 0;
    }

    public override bool shouldRender(double distance)
    {
        double range = boundingBox.AverageEdgeLength * 4.0D;
        range *= 64.0D;
        return distance < range * range;
    }

    public override void tick()
    {
        lastTickX = x;
        lastTickY = y;
        lastTickZ = z;
        base.tick();

        x += velocityX;
        y += velocityY;
        z += velocityZ;
        float horizontalSpeed = MathHelper.Sqrt(velocityX * velocityX + velocityZ * velocityZ);
        yaw = (float)(System.Math.Atan2(velocityX, velocityZ) * 180.0D / System.Math.PI);
        pitch = (float)(System.Math.Atan2(velocityY, horizontalSpeed) * 180.0D / System.Math.PI);
        while (pitch - prevPitch < -180.0F) prevPitch -= 360.0F;
        while (pitch - prevPitch >= 180.0F) prevPitch += 360.0F;
        while (yaw - prevYaw < -180.0F) prevYaw -= 360.0F;
        while (yaw - prevYaw >= 180.0F) prevYaw += 360.0F;
        pitch = prevPitch + (pitch - prevPitch) * 0.2F;
        yaw = prevYaw + (yaw - prevYaw) * 0.2F;

        if (!world.IsRemote)
        {
            double dx = _targetX - x;
            double dz = _targetZ - z;
            float targetHorizontal = (float)System.Math.Sqrt(dx * dx + dz * dz);
            float targetYaw = (float)System.Math.Atan2(dz, dx);
            double nextSpeed = horizontalSpeed + (targetHorizontal - horizontalSpeed) * 0.0025D;
            if (targetHorizontal < 1.0F)
            {
                nextSpeed *= 0.8D;
                velocityY *= 0.8D;
            }

            velocityX = System.Math.Cos(targetYaw) * nextSpeed;
            velocityZ = System.Math.Sin(targetYaw) * nextSpeed;
            if (y < _targetY)
            {
                velocityY += (1.0D - velocityY) * 0.015F;
            }
            else
            {
                velocityY += (-1.0D - velocityY) * 0.015F;
            }
        }

        float trail = 0.25F;
        if (isInWater())
        {
            for (int i = 0; i < 4; ++i)
            {
                world.Broadcaster.AddParticle("bubble", x - velocityX * trail, y - velocityY * trail, z - velocityZ * trail, velocityX, velocityY, velocityZ);
            }
        }
        else
        {
            world.Broadcaster.AddParticle("portal", x - velocityX * trail + random.NextDouble() * 0.6D - 0.3D, y - velocityY * trail - 0.5D, z - velocityZ * trail + random.NextDouble() * 0.6D - 0.3D, velocityX, velocityY, velocityZ);
        }

        if (!world.IsRemote)
        {
            setPosition(x, y, z);
            ++_life;
            if (_life > 80)
            {
                markDead();
                if (_dropOrShatter)
                {
                    world.SpawnItemDrop(x, y, z, new ItemStack(Item.EyeOfEnder));
                }
                else
                {
                    world.Broadcaster.WorldEvent(2003, MathHelper.Floor(x), MathHelper.Floor(y), MathHelper.Floor(z), 0);
                }
            }
        }
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
    }

    public override void readNbt(NBTTagCompound nbt)
    {
    }
}
