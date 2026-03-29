using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Potions;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public class EntityPotion : Entity
{
    public override EntityType Type => EntityRegistry.Potion;

    private int _xTile = -1;
    private int _yTile = -1;
    private int _zTile = -1;
    private int _inTile;
    private bool _inGround;
    private EntityLiving? _thrower;
    private int _ticksInGround;
    private int _ticksInAir;
    private int _potionDamage;

    public EntityPotion(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(0.25F, 0.25F);
    }

    public EntityPotion(IWorldContext world, EntityLiving thrower, int potionDamage) : this(world)
    {
        _thrower = thrower;
        _potionDamage = potionDamage;
        setPositionAndAnglesKeepPrevAngles(thrower.x, thrower.y + thrower.getEyeHeight(), thrower.z, thrower.yaw, thrower.pitch);
        x -= MathHelper.Cos(yaw / 180.0F * (float)Math.PI) * 0.16F;
        y -= 0.1F;
        z -= MathHelper.Sin(yaw / 180.0F * (float)Math.PI) * 0.16F;
        setPosition(x, y, z);
        standingEyeHeight = 0.0F;
        float speed = 0.5F;
        velocityX = -MathHelper.Sin(yaw / 180.0F * (float)Math.PI) * MathHelper.Cos(pitch / 180.0F * (float)Math.PI) * speed;
        velocityZ = MathHelper.Cos(yaw / 180.0F * (float)Math.PI) * MathHelper.Cos(pitch / 180.0F * (float)Math.PI) * speed;
        velocityY = -MathHelper.Sin(pitch / 180.0F * (float)Math.PI) * speed;
        SetHeading(velocityX, velocityY, velocityZ, 0.5F, -20.0F);
    }

    public EntityPotion(IWorldContext world, double x, double y, double z) : this(world)
    {
        setPosition(x, y, z);
        standingEyeHeight = 0.0F;
    }

    public EntityLiving? Thrower
    {
        get => _thrower;
        set => _thrower = value;
    }

    public int PotionDamage
    {
        get => _potionDamage;
        set => _potionDamage = value;
    }

    public override bool shouldRender(double distance)
    {
        double range = boundingBox.AverageEdgeLength * 4.0D;
        range *= 64.0D;
        return distance < range * range;
    }

    public override void setVelocityClient(double vx, double vy, double vz)
    {
        velocityX = vx;
        velocityY = vy;
        velocityZ = vz;
        if (prevPitch == 0.0F && prevYaw == 0.0F)
        {
            float horizontal = MathHelper.Sqrt(vx * vx + vz * vz);
            prevYaw = yaw = (float)(Math.Atan2(vx, vz) * 180.0D / Math.PI);
            prevPitch = pitch = (float)(Math.Atan2(vy, horizontal) * 180.0D / Math.PI);
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
            end = hit.Pos;
        }

        if (!world.IsRemote)
        {
            Entity? hitEntity = null;
            double bestDistance = 0.0D;
            List<Entity> entities = world.Entities.GetEntities(this, boundingBox.Stretch(velocityX, velocityY, velocityZ).Expand(1.0D, 1.0D, 1.0D));
            for (int i = 0; i < entities.Count; ++i)
            {
                Entity entity = entities[i];
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
            OnImpact(hit);
            return;
        }

        x += velocityX;
        y += velocityY;
        z += velocityZ;
        float horizontalSpeed = MathHelper.Sqrt(velocityX * velocityX + velocityZ * velocityZ);
        yaw = (float)(Math.Atan2(velocityX, velocityZ) * 180.0D / Math.PI);
        pitch = (float)(Math.Atan2(velocityY, horizontalSpeed) * 180.0D / Math.PI);

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
        velocityY -= 0.05D;
        setPosition(x, y, z);
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        nbt.SetShort("xTile", (short)_xTile);
        nbt.SetShort("yTile", (short)_yTile);
        nbt.SetShort("zTile", (short)_zTile);
        nbt.SetByte("inTile", (sbyte)_inTile);
        nbt.SetByte("inGround", (sbyte)(_inGround ? 1 : 0));
        nbt.SetShort("Potion", (short)_potionDamage);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        _xTile = nbt.GetShort("xTile");
        _yTile = nbt.GetShort("yTile");
        _zTile = nbt.GetShort("zTile");
        _inTile = nbt.GetByte("inTile") & 255;
        _inGround = nbt.GetByte("inGround") == 1;
        _potionDamage = nbt.GetShort("Potion");
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
        prevYaw = yaw = (float)(Math.Atan2(vx, vz) * 180.0D / Math.PI);
        prevPitch = pitch = (float)(Math.Atan2(vy, horizontal) * 180.0D / Math.PI) + pitchOffset;
        _ticksInGround = 0;
    }

    private void OnImpact(HitResult hit)
    {
        if (!world.IsRemote)
        {
            List<PotionEffect>? effects = ((ItemPotion)Item.Potion).GetEffects(_potionDamage);
            if (effects != null && effects.Count > 0)
            {
                Box splashArea = boundingBox.Expand(4.0D, 2.0D, 4.0D);
                List<EntityLiving> nearby = world.Entities.CollectEntitiesOfType<EntityLiving>(splashArea);
                for (int i = 0; i < nearby.Count; ++i)
                {
                    EntityLiving target = nearby[i];
                    double distanceSq = getSquaredDistance(target);
                    if (distanceSq >= 16.0D)
                    {
                        continue;
                    }

                    double intensity = 1.0D - Math.Sqrt(distanceSq) / 4.0D;
                    if (target == hit.Entity)
                    {
                        intensity = 1.0D;
                    }

                    for (int j = 0; j < effects.Count; ++j)
                    {
                        PotionEffect effect = effects[j];
                        Potion potion = Potion.PotionTypes[effect.PotionId]!;
                        if (potion.IsInstant())
                        {
                            potion.AffectEntity(_thrower, target, effect.Amplifier, intensity);
                        }
                        else
                        {
                            int duration = (int)(intensity * effect.Duration + 0.5D);
                            if (duration > 20)
                            {
                                target.addPotionEffect(new PotionEffect(effect.PotionId, duration, effect.Amplifier));
                            }
                        }
                    }
                }
            }

            world.Broadcaster.WorldEvent(2002, MathHelper.Floor(x), MathHelper.Floor(y), MathHelper.Floor(z), _potionDamage);
        }

        markDead();
    }
}
