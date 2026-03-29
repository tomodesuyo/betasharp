using BetaSharp.Items;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityWither : EntityBlaze, IBossDisplayData
{
    private readonly SyncedProperty<int> _syncedHealth;
    private readonly SyncedProperty<int> _leftHeadTargetId;
    private readonly SyncedProperty<int> _rightHeadTargetId;
    private readonly float[] _headYaw = new float[2];
    private readonly float[] _headPitch = new float[2];

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
            int targetId = playerToAttack is { dead: false } ? playerToAttack.id : 0;
            _leftHeadTargetId.Value = targetId;
            _rightHeadTargetId.Value = targetId;
        }

        UpdateSideHead(0, _leftHeadTargetId.Value);
        UpdateSideHead(1, _rightHeadTargetId.Value);
    }

    public override bool damage(Entity entity, int amount)
    {
        hearts = 0;
        damageForDisplay = 0;
        bool damaged = base.damage(entity, amount);
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
