using BetaSharp.NBT;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityEnderPearl : Entity
{
    public override EntityType Type => EntityRegistry.EnderPearl;

    public EntityLiving? Thrower { get; set; }
    private int _ticksInAir;

    public EntityEnderPearl(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(0.25F, 0.25F);
    }

    public EntityEnderPearl(IWorldContext world, EntityLiving thrower) : this(world)
    {
        Thrower = thrower;
        setPositionAndAnglesKeepPrevAngles(thrower.x, thrower.y + thrower.getEyeHeight(), thrower.z, thrower.yaw, thrower.pitch);
        x -= MathHelper.Cos(yaw / 180.0F * (float)System.Math.PI) * 0.16F;
        y -= 0.1F;
        z -= MathHelper.Sin(yaw / 180.0F * (float)System.Math.PI) * 0.16F;
        setPosition(x, y, z);
        standingEyeHeight = 0.0F;
        float speed = 0.4F;
        velocityX = -MathHelper.Sin(yaw / 180.0F * (float)System.Math.PI) * MathHelper.Cos(pitch / 180.0F * (float)System.Math.PI) * speed;
        velocityZ = MathHelper.Cos(yaw / 180.0F * (float)System.Math.PI) * MathHelper.Cos(pitch / 180.0F * (float)System.Math.PI) * speed;
        velocityY = -MathHelper.Sin(pitch / 180.0F * (float)System.Math.PI) * speed;
        SetHeading(velocityX, velocityY, velocityZ, 1.5F, 1.0F);
    }

    public EntityEnderPearl(IWorldContext world, double x, double y, double z) : this(world)
    {
        setPosition(x, y, z);
        standingEyeHeight = 0.0F;
    }

    public override bool shouldRender(double distance)
    {
        double range = boundingBox.AverageEdgeLength * 4.0D;
        range *= 64.0D;
        return distance < range * range;
    }

    private void SetHeading(double vx, double vy, double vz, float speed, float spread)
    {
        float length = MathHelper.Sqrt(vx * vx + vy * vy + vz * vz);
        vx /= length;
        vy /= length;
        vz /= length;
        vx += random.NextGaussian() * 0.0075F * spread;
        vy += random.NextGaussian() * 0.0075F * spread;
        vz += random.NextGaussian() * 0.0075F * spread;
        velocityX = vx * speed;
        velocityY = vy * speed;
        velocityZ = vz * speed;
        float horizontal = MathHelper.Sqrt(velocityX * velocityX + velocityZ * velocityZ);
        prevYaw = yaw = (float)(System.Math.Atan2(velocityX, velocityZ) * 180.0D / System.Math.PI);
        prevPitch = pitch = (float)(System.Math.Atan2(velocityY, horizontal) * 180.0D / System.Math.PI);
    }

    public override void setVelocityClient(double vx, double vy, double vz)
    {
        velocityX = vx;
        velocityY = vy;
        velocityZ = vz;
    }

    public override void tick()
    {
        lastTickX = x;
        lastTickY = y;
        lastTickZ = z;
        base.tick();
        ++_ticksInAir;

        Vec3D start = new(x, y, z);
        Vec3D end = new(x + velocityX, y + velocityY, z + velocityZ);
        HitResult hit = world.Reader.Raycast(start, end);
        if (hit.Type != HitResultType.MISS)
        {
            end = new Vec3D(hit.Pos.x, hit.Pos.y, hit.Pos.z);
        }

        if (!world.IsRemote)
        {
            Entity? hitEntity = null;
            double bestDistance = 0.0D;
            var entities = world.Entities.GetEntities(this, boundingBox.Stretch(velocityX, velocityY, velocityZ).Expand(1.0D, 1.0D, 1.0D));
            foreach (Entity entity in entities)
            {
                if (!entity.isCollidable() || entity == Thrower && _ticksInAir < 5)
                {
                    continue;
                }

                HitResult entityHit = entity.boundingBox.Expand(0.3D, 0.3D, 0.3D).Raycast(start, end);
                if (entityHit.Type == HitResultType.MISS)
                {
                    continue;
                }

                double distance = start.distanceTo(entityHit.Pos);
                if (distance < bestDistance || bestDistance == 0.0D)
                {
                    hitEntity = entity;
                    bestDistance = distance;
                }
            }

            if (hitEntity != null)
            {
                hit = new HitResult(hitEntity);
            }
        }

        if (hit.Type != HitResultType.MISS)
        {
            if (hit.Entity != null && Thrower != null)
            {
                hit.Entity.damage(Thrower, 0);
            }

            for (int i = 0; i < 32; ++i)
            {
                world.Broadcaster.AddParticle("portal", x, y + random.NextDouble() * 2.0D, z, random.NextGaussian(), 0.0D, random.NextGaussian());
            }

            if (!world.IsRemote)
            {
                if (Thrower is ServerPlayerEntity serverPlayer)
                {
                    serverPlayer.networkHandler.teleport(x, y, z, serverPlayer.yaw, serverPlayer.pitch);
                    serverPlayer.damage(this, 5);
                }
                else if (Thrower != null)
                {
                    Thrower.setPosition(x, y, z);
                    Thrower.damage(this, 5);
                }

            }

            markDead();
            return;
        }

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
        float drag = isInWater() ? 0.8F : 0.99F;
        if (isInWater())
        {
            for (int i = 0; i < 4; ++i)
            {
                world.Broadcaster.AddParticle("bubble", x - velocityX * 0.25D, y - velocityY * 0.25D, z - velocityZ * 0.25D, velocityX, velocityY, velocityZ);
            }
        }

        velocityX *= drag;
        velocityY *= drag;
        velocityZ *= drag;
        velocityY -= 0.03D;
        setPosition(x, y, z);
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
    }

    public override void readNbt(NBTTagCompound nbt)
    {
    }
}
