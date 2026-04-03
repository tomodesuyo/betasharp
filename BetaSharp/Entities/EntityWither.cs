using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Util;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityWither : EntityBlaze, IBossDisplayData
{
    private readonly SyncedProperty<int> _syncedHealth;
    private readonly SyncedProperty<int> _leftHeadTargetId;
    private readonly SyncedProperty<int> _rightHeadTargetId;
    private readonly float[] _headYaw = new float[2];
    private readonly float[] _headPitch = new float[2];
    private readonly int[] _sideHeadCooldowns = new int[2];

    public override EntityType Type => EntityRegistry.Wither;
    public int BossHealth => world.IsRemote ? _syncedHealth.Value : health;
    public int MaxBossHealth => maxHealth;
    public string BossName => "Wither";

    public EntityWither(IWorldContext world) : base(world)
    {
        texture = "/mob/wither.png";
        setBoundingBoxSpacing(0.9F, 3.5F);
        maxHealth = 300;
        health = 300;
        movementSpeed = 0.6F;
        scoreAmount = 50;
        _syncedHealth = DataSynchronizer.MakeProperty<int>(16, health);
        _leftHeadTargetId = DataSynchronizer.MakeProperty<int>(17, 0);
        _rightHeadTargetId = DataSynchronizer.MakeProperty<int>(18, 0);
    }

    protected override bool canDespawn() => false;

    public override void tickLiving()
    {
        base.tickLiving();
        hearts = 0;
        damageForDisplay = 0;

        if (world.IsRemote)
        {
            health = _syncedHealth.Value;
        }
        else
        {
            _syncedHealth.Value = health;
            UpdateSideHeadTargets();
            TickSideHeadAttacks();
        }

        UpdateSideHead(0, _leftHeadTargetId.Value);
        UpdateSideHead(1, _rightHeadTargetId.Value);
    }

    public override bool damage(Entity entity, int amount)
    {
        hearts = 0;
        damageForDisplay = 0;
        bool damaged = base.damage(entity, amount);
        if (playerToAttack is EntityLiving living && living.isUndead())
        {
            playerToAttack = null;
        }

        hearts = 0;
        damageForDisplay = 0;
        return damaged;
    }

    public float GetTrackedHeadYaw(int headIndex) => _headYaw[headIndex];

    public float GetTrackedHeadPitch(int headIndex) => _headPitch[headIndex];

    protected override string getLivingSound() => "mob.wither.idle";

    protected override string getHurtSound() => "mob.wither.hurt";

    protected override string getDeathSound() => "mob.wither.death";

    protected override int getDropItemId() => Item.NetherStar.id;

    protected override void dropFewItems() => dropItem(Item.NetherStar.id, 1);

    public override bool canSpawn()
    {
        return false;
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (attackTime <= 0 && distance < 2.0F && entity.boundingBox.MaxY > boundingBox.MinY && entity.boundingBox.MinY < boundingBox.MaxY)
        {
            attackTime = 20;
            entity.damage(this, attackStrength);
            return;
        }

        if (distance > 64.0F)
        {
            return;
        }

        if (attackTime <= 0)
        {
            LaunchWitherSkull(0, entity, false);
            attackTime = 40;
        }

        double deltaX = entity.x - x;
        double deltaZ = entity.z - z;
        bodyYaw = yaw = (float)(Math.Atan2(deltaZ, deltaX) * 180.0D / Math.PI) - 90.0F;
        hasAttacked = true;
    }

    private void UpdateSideHeadTargets()
    {
        _leftHeadTargetId.Value = ResolveSideHeadTarget(_leftHeadTargetId.Value, _rightHeadTargetId.Value);
        _rightHeadTargetId.Value = ResolveSideHeadTarget(_rightHeadTargetId.Value, _leftHeadTargetId.Value);
    }

    private void TickSideHeadAttacks()
    {
        for (int headIndex = 0; headIndex < _sideHeadCooldowns.Length; ++headIndex)
        {
            if (_sideHeadCooldowns[headIndex] > 0)
            {
                --_sideHeadCooldowns[headIndex];
            }

            int targetId = headIndex == 0 ? _leftHeadTargetId.Value : _rightHeadTargetId.Value;
            Entity? target = targetId > 0 ? world.Entities.GetEntityByID(targetId) : null;
            if (!IsValidHeadTarget(target))
            {
                continue;
            }

            if (_sideHeadCooldowns[headIndex] <= 0 && canSee(target!) && getSquaredDistance(target!) <= 900.0D)
            {
                LaunchWitherSkull(headIndex + 1, target!, false);
                _sideHeadCooldowns[headIndex] = 20 + random.NextInt(20);
            }
        }
    }

    private int ResolveSideHeadTarget(int currentTargetId, int otherHeadTargetId)
    {
        Entity? currentTarget = currentTargetId > 0 ? world.Entities.GetEntityByID(currentTargetId) : null;
        if (IsValidHeadTarget(currentTarget))
        {
            return currentTargetId;
        }

        if (playerToAttack != null && IsValidHeadTarget(playerToAttack) && playerToAttack.id != otherHeadTargetId)
        {
            return playerToAttack.id;
        }

        EntityLiving? nearbyTarget = FindNearbyHeadTarget(otherHeadTargetId);
        return nearbyTarget?.id ?? 0;
    }

    private EntityLiving? FindNearbyHeadTarget(int excludedTargetId)
    {
        List<EntityLiving> candidates = world.Entities.CollectEntitiesOfType<EntityLiving>(new Box(x - 20.0D, y - 8.0D, z - 20.0D, x + 20.0D, y + 8.0D, z + 20.0D));
        for (int attempts = 0; attempts < candidates.Count; ++attempts)
        {
            EntityLiving candidate = candidates[random.NextInt(candidates.Count)];
            if (candidate.id == excludedTargetId || !IsValidHeadTarget(candidate))
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    private bool IsValidHeadTarget(Entity? target)
    {
        if (target == null || target == this || target.dead)
        {
            return false;
        }

        if (target is EntityPlayer player && player.IsIgnoredByMonsters)
        {
            return false;
        }

        if (target is EntityLiving living && living.isUndead())
        {
            return false;
        }

        return getSquaredDistance(target) <= 900.0D;
    }

    private void LaunchWitherSkull(int headIndex, Entity target, bool invulnerable)
    {
        double targetY = target.y + target.getEyeHeight() * (target is EntityLiving ? 0.5D : 1.0D);
        LaunchWitherSkull(headIndex, target.x, targetY, target.z, invulnerable);
    }

    private void LaunchWitherSkull(int headIndex, double targetX, double targetY, double targetZ, bool invulnerable)
    {
        double headX = GetHeadX(headIndex);
        double headY = GetHeadY(headIndex);
        double headZ = GetHeadZ(headIndex);
        EntityWitherSkull witherSkull = new(world, this, targetX - headX, targetY - headY, targetZ - headZ)
        {
            x = headX,
            y = headY,
            z = headZ
        };
        witherSkull.setPosition(headX, headY, headZ);
        witherSkull.SetInvulnerable(invulnerable);
        world.SpawnEntity(witherSkull);
        world.Broadcaster.PlaySoundAtEntity(this, "random.bow", 0.8F, 0.8F + random.NextFloat() * 0.2F);
    }

    private void UpdateSideHead(int headIndex, int targetId)
    {
        Entity? target = targetId > 0 ? world.Entities.GetEntityByID(targetId) : null;
        if (target != null && !target.dead)
        {
            double headX = GetHeadX(headIndex + 1);
            double headY = GetHeadY(headIndex + 1);
            double headZ = GetHeadZ(headIndex + 1);
            double deltaX = target.x - headX;
            double deltaY = target.y + target.getEyeHeight() - headY;
            double deltaZ = target.z - headZ;
            double horizontalDistance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
            float targetYaw = (float)(Math.Atan2(deltaZ, deltaX) * 180.0D / Math.PI) - 90.0F;
            float targetPitch = (float)(-(Math.Atan2(deltaY, horizontalDistance) * 180.0D / Math.PI));
            _headPitch[headIndex] = UpdateAngle(_headPitch[headIndex], targetPitch, 40.0F);
            _headYaw[headIndex] = UpdateAngle(_headYaw[headIndex], targetYaw, 10.0F);
            return;
        }

        _headYaw[headIndex] = UpdateAngle(_headYaw[headIndex], bodyYaw, 10.0F);
        _headPitch[headIndex] = UpdateAngle(_headPitch[headIndex], 0.0F, 40.0F);
    }

    private double GetHeadX(int headIndex)
    {
        if (headIndex <= 0)
        {
            return x;
        }

        float yawRadians = (bodyYaw + 180.0F * (headIndex - 1)) * (float)Math.PI / 180.0F;
        return x + MathHelper.Cos(yawRadians) * 1.3D;
    }

    private double GetHeadY(int headIndex) => headIndex <= 0 ? y + 3.0D : y + 2.2D;

    private double GetHeadZ(int headIndex)
    {
        if (headIndex <= 0)
        {
            return z;
        }

        float yawRadians = (bodyYaw + 180.0F * (headIndex - 1)) * (float)Math.PI / 180.0F;
        return z + MathHelper.Sin(yawRadians) * 1.3D;
    }

    private static float UpdateAngle(float current, float target, float maxDelta)
    {
        float delta = target - current;
        while (delta < -180.0F)
        {
            delta += 360.0F;
        }

        while (delta >= 180.0F)
        {
            delta -= 360.0F;
        }

        delta = Math.Clamp(delta, -maxDelta, maxDelta);
        return current + delta;
    }
}
