using BetaSharp.Items;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public class EntityBlaze : EntityMonster
{
    private float _heightOffset = 0.5F;
    private int _heightOffsetUpdateTicks;
    private int _attackStage;

    public override EntityType Type => EntityRegistry.Blaze;

    public EntityBlaze(IWorldContext world) : base(world)
    {
        texture = "/mob/fire.png";
        isImmuneToFire = true;
        attackStrength = 6;
        health = 20;
        movementSpeed = 0.23F;
    }

    public override void tickLiving()
    {
        if (!world.IsRemote)
        {
            if (isWet())
            {
                damage(null!, 1);
            }

            if (--_heightOffsetUpdateTicks <= 0)
            {
                _heightOffsetUpdateTicks = 100;
                _heightOffset = 0.5F + (float)random.NextGaussian() * 3.0F;
            }

            if (playerToAttack != null && playerToAttack.y + playerToAttack.getEyeHeight() > y + getEyeHeight() + _heightOffset)
            {
                velocityY += (0.3D - velocityY) * 0.3D;
            }

            SetFlag(0, _attackStage > 0);
        }

        if (random.NextInt(24) == 0)
        {
            world.Broadcaster.PlaySoundAtPos(x + 0.5D, y + 0.5D, z + 0.5D, "fire.fire", 1.0F + random.NextFloat(), random.NextFloat() * 0.7F + 0.3F);
        }

        if (!onGround && velocityY < 0.0D)
        {
            velocityY *= 0.6D;
        }

        for (int i = 0; i < 2; ++i)
        {
            world.Broadcaster.AddParticle(
                "largesmoke",
                x + (random.NextDouble() - 0.5D) * width,
                y + random.NextDouble() * height,
                z + (random.NextDouble() - 0.5D) * width,
                0.0D,
                0.0D,
                0.0D);
        }

        base.tickLiving();
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (attackTime <= 0 && distance < 2.0F && entity.boundingBox.MaxY > boundingBox.MinY && entity.boundingBox.MinY < boundingBox.MaxY)
        {
            attackTime = 20;
            entity.damage(this, attackStrength);
            return;
        }

        if (distance >= 30.0F)
        {
            return;
        }

        double deltaX = entity.x - x;
        double deltaY = entity.boundingBox.MinY + entity.height / 2.0F - (y + height / 2.0F);
        double deltaZ = entity.z - z;
        if (attackTime == 0)
        {
            ++_attackStage;
            if (_attackStage == 1)
            {
                attackTime = 60;
            }
            else if (_attackStage <= 4)
            {
                attackTime = 6;
            }
            else
            {
                attackTime = 100;
                _attackStage = 0;
            }

            if (_attackStage > 1)
            {
                float spread = MathHelper.Sqrt(distance) * 0.5F;
                Vec3D look = getLook(1.0F);
                EntitySmallFireball fireball = new(
                    world,
                    this,
                    deltaX + random.NextGaussian() * spread,
                    deltaY,
                    deltaZ + random.NextGaussian() * spread);
                fireball.x = x + look.x;
                fireball.y = y + height / 2.0F + 0.5D;
                fireball.z = z + look.z;
                world.SpawnEntity(fireball);
            }
        }

        yaw = bodyYaw = (float)(System.Math.Atan2(deltaZ, deltaX) * 180.0D / (double)((float)System.Math.PI)) - 90.0F;
        hasAttacked = true;
    }

    protected override string getLivingSound()
    {
        return "mob.blaze.breathe";
    }

    protected override string getHurtSound()
    {
        return "mob.blaze.hit";
    }

    protected override string getDeathSound()
    {
        return "mob.blaze.death";
    }

    protected override int getDropItemId()
    {
        return Item.BlazeRod.id;
    }

    protected override void dropFewItems()
    {
        int count = random.NextInt(2);
        for (int i = 0; i < count; ++i)
        {
            dropItem(Item.BlazeRod.id, 1);
        }
    }

    public override bool canSpawn()
    {
        return world.Difficulty > 0 &&
               world.Entities.CanSpawnEntity(boundingBox) &&
               world.Entities.GetEntityCollisionsScratch(this, boundingBox).Count == 0 &&
               !world.Reader.IsMaterialInBox(boundingBox, material => material.IsFluid);
    }
}
