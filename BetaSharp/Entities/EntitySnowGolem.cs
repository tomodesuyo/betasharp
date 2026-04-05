using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntitySnowGolem : EntityCreature, SpawnableEntity
{
    public override EntityType Type => EntityRegistry.SnowGolem;

    public EntitySnowGolem(IWorldContext world) : base(world)
    {
        texture = "/mob/snowman.png";
        setBoundingBoxSpacing(0.4F, 1.8F);
        movementSpeed = 0.25F;
        maxHealth = 4;
        health = 4;
    }

    protected override Entity? findPlayerToAttack()
    {
        List<Entity> nearbyEntities = world.Entities.GetEntities(this, boundingBox.Expand(16.0D, 4.0D, 16.0D));
        EntityLiving? closestMonster = null;
        double closestDistance = double.MaxValue;
        for (int i = 0; i < nearbyEntities.Count; ++i)
        {
            if (nearbyEntities[i] is not EntityLiving living || nearbyEntities[i] is not Monster)
            {
                continue;
            }

            double distance = living.getSquaredDistance(this);
            if (distance < closestDistance)
            {
                closestMonster = living;
                closestDistance = distance;
            }
        }

        return closestMonster;
    }

    public override void tickLiving()
    {
        if (isWet())
        {
            damage(null, 1);
        }

        int blockX = MathHelper.Floor(x);
        int blockY = MathHelper.Floor(y);
        int blockZ = MathHelper.Floor(z);
        if (world.GetTemperature(blockX, blockY, blockZ) > 1.0F)
        {
            damage(null, 1);
        }

        if (!world.IsRemote)
        {
            for (int i = 0; i < 4; ++i)
            {
                int snowX = MathHelper.Floor(x + (i % 2 * 2 - 1) * 0.25D);
                int snowY = MathHelper.Floor(y);
                int snowZ = MathHelper.Floor(z + (i / 2 % 2 * 2 - 1) * 0.25D);
                if (world.Reader.GetBlockId(snowX, snowY, snowZ) == 0 && world.CanSnowAt(snowX, snowY, snowZ))
                {
                    world.Writer.SetBlock(snowX, snowY, snowZ, Block.Snow.id);
                }
            }
        }

        base.tickLiving();
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (distance < 10.0F)
        {
            double dx = entity.x - x;
            double dz = entity.z - z;
            if (attackTime == 0)
            {
                EntitySnowball snowball = new(world, this);
                double targetHeightOffset = entity.y + entity.getEyeHeight() - 1.1D - snowball.y;
                float distanceFactor = MathHelper.Sqrt(dx * dx + dz * dz) * 0.2F;
                world.Broadcaster.PlaySoundAtEntity(this, "random.bow", 1.0F, 1.0F / (random.NextFloat() * 0.4F + 0.8F));
                world.SpawnEntity(snowball);
                snowball.setSnowballHeading(dx, targetHeightOffset + distanceFactor, dz, 1.6F, 12.0F);
                attackTime = 10;
            }

            yaw = (float)(Math.Atan2(dz, dx) * 180.0D / Math.PI) - 90.0F;
            hasAttacked = true;
        }
    }

    protected override string getLivingSound() => "none";

    protected override int getDropItemId() => Item.Snowball.id;

    protected override void dropFewItems()
    {
        int amount = random.NextInt(16);
        for (int i = 0; i < amount; ++i)
        {
            dropItem(Item.Snowball.id, 1);
        }
    }
}
