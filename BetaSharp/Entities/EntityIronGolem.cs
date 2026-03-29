using BetaSharp;
using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Villages;

namespace BetaSharp.Entities;

public sealed class EntityIronGolem : EntityCreature, SpawnableEntity
{
    private int _villageSearchTicks;
    private int _attackTicks;
    private int _offerRoseTicks;
    private readonly SyncedProperty<byte> _golemFlags;

    public override EntityType Type => EntityRegistry.IronGolem;
    public Village? Village { get; private set; }
    public int AttackTicks => _attackTicks;
    public int OfferRoseTicks => _offerRoseTicks;

    public EntityIronGolem(IWorldContext world) : base(world)
    {
        texture = "/mob/villager_golem.png";
        setBoundingBoxSpacing(1.4F, 2.9F);
        movementSpeed = 0.25F;
        maxHealth = 100;
        health = 100;
        _golemFlags = DataSynchronizer.MakeProperty<byte>(16, 0);
    }

    protected override bool canDespawn() => false;

    public override int getMaxSpawnedInChunk() => 1;

    public override void tick()
    {
        base.tick();

        if (_attackTicks > 0)
        {
            --_attackTicks;
        }

        if (_offerRoseTicks > 0)
        {
            --_offerRoseTicks;
        }
    }

    public override void tickLiving()
    {
        if (--_villageSearchTicks <= 0)
        {
            if (world is BetaSharp.Worlds.Core.World runtimeWorld)
            {
                Village = runtimeWorld.Villages.FindNearestVillage(MathHelper.Floor(x), MathHelper.Floor(y), MathHelper.Floor(z), 32);
            }

            _villageSearchTicks = 70 + random.NextInt(50);
        }

        if (velocityX * velocityX + velocityZ * velocityZ > 2.5000003E-7F && random.NextInt(5) == 0)
        {
            int blockX = MathHelper.Floor(x);
            int blockY = MathHelper.Floor(boundingBox.MinY - 0.2D);
            int blockZ = MathHelper.Floor(z);
            int blockId = world.Reader.GetBlockId(blockX, blockY, blockZ);
            if (blockId > 0)
            {
                world.Broadcaster.AddParticle(
                    "tilecrack_" + blockId,
                    x + (random.NextFloat() - 0.5D) * width,
                    boundingBox.MinY + 0.1D,
                    z + (random.NextFloat() - 0.5D) * width,
                    4.0D * (random.NextFloat() - 0.5D),
                    0.5D,
                    4.0D * (random.NextFloat() - 0.5D));
            }
        }

        base.tickLiving();
    }

    protected override Entity? findPlayerToAttack()
    {
        if (Village != null)
        {
            EntityLiving? aggressor = Village.FindNearestVillageAggressor(this);
            if (aggressor != null && aggressor.isAlive())
            {
                return aggressor;
            }
        }

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

    protected override void attackEntity(Entity entity, float distance)
    {
        if (attackTime > 0 || distance >= 4.0F || entity.boundingBox.MaxY <= boundingBox.MinY || entity.boundingBox.MinY >= boundingBox.MaxY)
        {
            return;
        }

        attackTime = 20;
        _attackTicks = 10;
        world.Broadcaster.EntityEvent(this, 4);
        bool hit = entity.damage(this, 7 + random.NextInt(15));
        if (hit)
        {
            entity.velocityY += 0.4D;
        }

        world.Broadcaster.PlaySoundAtEntity(this, "mob.irongolem.throw", 1.0F, 1.0F);
    }

    public override bool damage(Entity entity, int amount)
    {
        bool damaged = base.damage(entity, amount);
        if (damaged && entity is EntityLiving living)
        {
            playerToAttack = living;
            Village?.AddOrRenewAggressor(living);
        }

        return damaged;
    }

    public override void processServerEntityStatus(sbyte statusId)
    {
        if (statusId == 4)
        {
            _attackTicks = 10;
            world.Broadcaster.PlaySoundAtEntity(this, "mob.irongolem.throw", 1.0F, 1.0F);
        }
        else if (statusId == 11)
        {
            _offerRoseTicks = 400;
        }
        else
        {
            base.processServerEntityStatus(statusId);
        }
    }

    public void SetHoldingRose(bool holdingRose)
    {
        _offerRoseTicks = holdingRose ? 400 : 0;
        world.Broadcaster.EntityEvent(this, 11);
    }

    public bool IsPlayerCreated => (_golemFlags.Value & 1) != 0;

    public void SetPlayerCreated(bool playerCreated)
    {
        byte flags = _golemFlags.Value;
        _golemFlags.Value = playerCreated ? (byte)(flags | 1) : (byte)(flags & ~1);
    }

    protected override void dropFewItems()
    {
        int roseCount = random.NextInt(3);
        for (int i = 0; i < roseCount; ++i)
        {
            dropItem(Block.Rose.id, 1);
        }

        int ironCount = 3 + random.NextInt(3);
        for (int i = 0; i < ironCount; ++i)
        {
            dropItem(Item.IronIngot.id, 1);
        }
    }

    protected override string getLivingSound() => "none";

    protected override string getHurtSound() => "mob.irongolem.hit";

    protected override string getDeathSound() => "mob.irongolem.death";

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetBoolean("PlayerCreated", IsPlayerCreated);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        SetPlayerCreated(nbt.GetBoolean("PlayerCreated"));
    }
}
