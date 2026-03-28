using BetaSharp.NBT;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public class EntityExpBottle : Entity
{
    public override EntityType Type => EntityRegistry.ExpBottle;

    private int _xTile = -1;
    private int _yTile = -1;
    private int _zTile = -1;
    private int _inTile;
    private bool _inGround;
    private EntityLiving? _thrower;
    private int _ticksInGround;
    private int _ticksInAir;

    public EntityExpBottle(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(0.25F, 0.25F);
    }

    public EntityExpBottle(IWorldContext world, EntityLiving thrower) : base(world)
    {
        _thrower = thrower;
        setBoundingBoxSpacing(0.25F, 0.25F);
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
        SetHeading(velocityX, velocityY, velocityZ, 0.7F, -20.0F);
    }

    public EntityLiving? Thrower
    {
        get => _thrower;
        set => _thrower = value;
    }

    public EntityExpBottle(IWorldContext world, double x, double y, double z) : base(world)
    {
        setBoundingBoxSpacing(0.25F, 0.25F);
        setPosition(x, y, z);
        standingEyeHeight = 0.0F;
    }

    public override bool shouldRender(double distance)
    {
        double range = boundingBox.AverageEdgeLength * 4.0D;
        range *= 64.0D;
        return distance < range * range;
    }

    private void SetHeading(double vx, double vy, double vz, float speed, float pitchOffset)
    {
        float length = MathHelper.Sqrt(vx * vx + vy * vy + vz * vz);
        vx /= length;
        vy /= length;
        vz /= length;
        vx += random.NextGaussian() * 0.0075F;
        vy += random.NextGaussian() * 0.0075F;
        vz += random.NextGaussian() * 0.0075F;
        vx *= speed;
        vy *= speed;
        vz *= speed;
        velocityX = vx;
        velocityY = vy;
        velocityZ = vz;
        float horizontal = MathHelper.Sqrt(vx * vx + vz * vz);
        prevYaw = yaw = (float)(System.Math.Atan2(vx, vz) * 180.0D / System.Math.PI);
        prevPitch = pitch = (float)(System.Math.Atan2(vy, horizontal) * 180.0D / System.Math.PI) + pitchOffset;
        _ticksInGround = 0;
    }

    public override void setVelocityClient(double vx, double vy, double vz)
    {
        velocityX = vx;
        velocityY = vy;
        velocityZ = vz;
        if (prevPitch == 0.0F && prevYaw == 0.0F)
        {
            float horizontal = MathHelper.Sqrt(vx * vx + vz * vz);
            prevYaw = yaw = (float)(System.Math.Atan2(vx, vz) * 180.0D / System.Math.PI);
            prevPitch = pitch = (float)(System.Math.Atan2(vy, horizontal) * 180.0D / System.Math.PI);
        }
    }

    public override void tick()
    {
        lastTickX = x;
        lastTickY = y;
        lastTickZ = z;
        base.tick();

        if (_inGround)
        {
            if (world.Reader.GetBlockId(_xTile, _yTile, _zTile) == _inTile)
            {
                if (++_ticksInGround >= 1200)
                {
                    markDead();
                }

                return;
            }

            _inGround = false;
            velocityX *= random.NextFloat() * 0.2F;
            velocityY *= random.NextFloat() * 0.2F;
            velocityZ *= random.NextFloat() * 0.2F;
            _ticksInGround = 0;
            _ticksInAir = 0;
        }
        else
        {
            ++_ticksInAir;
        }

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
                if (!entity.isCollidable() || entity == _thrower && _ticksInAir < 5)
                {
                    continue;
                }

                Box box = entity.boundingBox.Expand(0.3D, 0.3D, 0.3D);
                HitResult entityHit = box.Raycast(start, end);
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
            if (hit.Entity != null)
            {
                hit.Entity.damage(_thrower, 0);
            }

            if (!world.IsRemote)
            {
                world.Broadcaster.WorldEvent(2002, MathHelper.Floor(x), MathHelper.Floor(y), MathHelper.Floor(z), 0);
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
        nbt.SetShort("xTile", (short)_xTile);
        nbt.SetShort("yTile", (short)_yTile);
        nbt.SetShort("zTile", (short)_zTile);
        nbt.SetByte("inTile", (sbyte)_inTile);
        nbt.SetByte("inGround", (sbyte)(_inGround ? 1 : 0));
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        _xTile = nbt.GetShort("xTile");
        _yTile = nbt.GetShort("yTile");
        _zTile = nbt.GetShort("zTile");
        _inTile = nbt.GetByte("inTile") & 255;
        _inGround = nbt.GetByte("inGround") == 1;
    }
}
