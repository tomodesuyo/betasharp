using BetaSharp.Blocks;
using BetaSharp.Items;
using BetaSharp.Potions;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityRabbit : EntityAnimal
{
    private enum RabbitMoveType
    {
        None,
        Hop,
        Step,
        Sprint,
        Attack
    }

    private readonly record struct RabbitMoveData(float SpeedMultiplier, float JumpVelocity, int HopDelay, int AnimationDuration);

    private const float BaseMovementSpeed = 0.3F;
    private const int WolfAvoidRange = 16;
    private const int CarrotRaidRange = 16;

    private static readonly string[] s_rabbitTextures =
    [
        "/mob/rabbit/brown.png",
        "/mob/rabbit/white.png",
        "/mob/rabbit/black.png",
        "/mob/rabbit/white_splotched.png",
        "/mob/rabbit/gold.png",
        "/mob/rabbit/salt.png",
    ];

    private int _jumpTicks;
    private int _jumpDuration;
    private int _hopDelay;
    private int _panicTicks;
    private int _carrotCooldown;
    private bool _hasRaidTarget;
    private int _raidTargetX;
    private int _raidTargetY;
    private int _raidTargetZ;
    private RabbitMoveType _moveType = RabbitMoveType.Hop;

    public override EntityType Type => EntityRegistry.Rabbit;
    public readonly SyncedProperty<byte> RabbitType;

    public EntityRabbit(IWorldContext world) : base(world)
    {
        texture = "/mob/rabbit/brown.png";
        setBoundingBoxSpacing(0.6F, 0.7F);
        maxHealth = 10;
        health = 10;
        movementSpeed = BaseMovementSpeed;
        RabbitType = DataSynchronizer.MakeProperty<byte>(16, GetRandomRabbitType(world));
    }

    public override void tick()
    {
        base.tick();

        if (_jumpTicks != _jumpDuration)
        {
            ++_jumpTicks;
        }
        else if (_jumpDuration != 0)
        {
            _jumpTicks = 0;
            _jumpDuration = 0;
        }
    }

    public override void tickLiving()
    {
        if (_panicTicks > 0)
        {
            --_panicTicks;
        }

        if (_carrotCooldown > 0)
        {
            _carrotCooldown -= random.NextInt(3);
            if (_carrotCooldown < 0)
            {
                _carrotCooldown = 0;
            }
        }

        bool panicking = !IsKillerBunny && TryAvoidNearbyWolf();
        if (panicking)
        {
            ClearRaidTarget();
        }
        else if (!IsKillerBunny)
        {
            UpdateRaidTarget();
        }

        base.tickLiving();
        UpdateMoveState(panicking);
    }

    public float GetJumpProgress(float partialTick)
    {
        if (_jumpDuration == 0)
        {
            return 0.0F;
        }

        float progress = (_jumpTicks + partialTick) / (float)_jumpDuration;
        return Math.Clamp(progress, 0.0F, 1.0F);
    }

    protected override void jump()
    {
        RabbitMoveData moveData = GetMoveData(_moveType);
        if (moveData.JumpVelocity <= 0.0F)
        {
            return;
        }

        velocityY = moveData.JumpVelocity;
        if (hasPotionEffect(Potion.Jump))
        {
            velocityY += (getActivePotionEffect(Potion.Jump)!.Amplifier + 1) * 0.1F;
        }

        if (_moveType == RabbitMoveType.Attack && playerToAttack != null)
        {
            double dx = playerToAttack.x - x;
            double dz = playerToAttack.z - z;
            float horizontalDistance = MathHelper.Sqrt(dx * dx + dz * dz);
            if (horizontalDistance > 1.0E-4F)
            {
                velocityX = dx / horizontalDistance * 0.5D * 0.8D + velocityX * 0.2D;
                velocityZ = dz / horizontalDistance * 0.5D * 0.8D + velocityZ * 0.2D;
            }
        }

        _jumpDuration = moveData.AnimationDuration;
        _jumpTicks = 0;
        _hopDelay = moveData.HopDelay;

        if (!world.IsRemote)
        {
            world.Broadcaster.EntityEvent(this, 1);
        }
    }

    public override void processServerEntityStatus(sbyte statusId)
    {
        if (statusId == 1)
        {
            _jumpDuration = GetMoveData(_moveType).AnimationDuration;
            _jumpTicks = 0;
            return;
        }

        base.processServerEntityStatus(statusId);
    }

    protected override Entity? findPlayerToAttack()
    {
        if (!IsKillerBunny)
        {
            return null;
        }

        Entity? closestTarget = null;
        double closestDistance = 16.0D * 16.0D;

        EntityPlayer? player = world.Entities.GetClosestPlayerTarget(x, y, z, 16.0D);
        if (player != null && canSee(player))
        {
            closestTarget = player;
            closestDistance = player.getSquaredDistance(this);
        }

        List<EntityWolf> wolves = world.Entities.CollectEntitiesOfType<EntityWolf>(boundingBox.Expand(16.0D, 4.0D, 16.0D));
        for (int i = 0; i < wolves.Count; ++i)
        {
            EntityWolf wolf = wolves[i];
            if (!wolf.isAlive() || !canSee(wolf))
            {
                continue;
            }

            double distance = wolf.getSquaredDistance(this);
            if (distance < closestDistance)
            {
                closestTarget = wolf;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (!IsKillerBunny)
        {
            return;
        }

        if (distance > 2.0F && distance < 6.0F)
        {
            _moveType = RabbitMoveType.Attack;
            if (onGround && _hopDelay <= 0)
            {
                jumping = true;
            }

            return;
        }

        if (attackTime <= 0 && distance < 1.5F && entity.boundingBox.MaxY > boundingBox.MinY && entity.boundingBox.MinY < boundingBox.MaxY)
        {
            attackTime = 20;
            entity.damage(this, 8);
        }
    }

    public override bool damage(Entity entity, int amount)
    {
        bool damaged = base.damage(entity, amount);
        if (damaged && !IsKillerBunny)
        {
            _panicTicks = Math.Max(_panicTicks, 40);
            ClearRaidTarget();
        }

        return damaged;
    }

    protected override string getLivingSound()
    {
        return "mob.chicken";
    }

    protected override string getHurtSound()
    {
        return "mob.chickenhurt";
    }

    protected override string getDeathSound()
    {
        return "mob.chickenhurt";
    }

    public override string getTexture()
    {
        return IsKillerBunny
            ? "/mob/rabbit/caerbannog.png"
            : s_rabbitTextures[RabbitType.Value % s_rabbitTextures.Length];
    }

    public override void writeNbt(BetaSharp.NBT.NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetByte("RabbitType", (sbyte)RabbitType.Value);
    }

    public override void readNbt(BetaSharp.NBT.NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        byte rabbitType = (byte)nbt.GetByte("RabbitType");
        RabbitType.Value = rabbitType == 99 || rabbitType < s_rabbitTextures.Length ? rabbitType : (byte)0;
    }

    protected override int getDropItemId()
    {
        return fireTicks > 0 ? Item.CookedRabbit.id : Item.RawRabbit.id;
    }

    protected override void dropFewItems()
    {
        dropItem(getDropItemId(), 1);

        if (random.NextInt(2) == 0)
        {
            dropItem(Item.RabbitHide.id, 1);
        }

        if (random.NextInt(10) == 0)
        {
            dropItem(Item.RabbitFoot.id, 1);
        }
    }

    private static byte GetRandomRabbitType(IWorldContext world)
    {
        return world.Random.NextInt(200) == 0 ? (byte)99 : (byte)world.Random.NextInt(s_rabbitTextures.Length);
    }

    private bool IsKillerBunny => RabbitType.Value == 99;

    private void UpdateMoveState(bool panicking)
    {
        bool wantsToMove = hasPath() || playerToAttack != null;
        RabbitMoveType desiredMoveType = SelectMoveType(panicking);
        float desiredSpeed = BaseMovementSpeed * GetMoveData(desiredMoveType).SpeedMultiplier;
        movementSpeed = desiredSpeed;

        if (forwardSpeed > 0.0F)
        {
            forwardSpeed = desiredSpeed;
        }

        if (!onGround)
        {
            _moveType = desiredMoveType;
            return;
        }

        if (!wantsToMove)
        {
            _moveType = RabbitMoveType.None;
            jumping = false;
            forwardSpeed = 0.0F;
            sidewaysSpeed = 0.0F;
            return;
        }

        if (_hopDelay > 0)
        {
            --_hopDelay;
            jumping = false;
            forwardSpeed = 0.0F;
            sidewaysSpeed = 0.0F;
            return;
        }

        _moveType = desiredMoveType;
        jumping = true;
    }

    private RabbitMoveType SelectMoveType(bool panicking)
    {
        if (IsKillerBunny && playerToAttack != null)
        {
            return RabbitMoveType.Attack;
        }

        if (panicking || _panicTicks > 0)
        {
            return RabbitMoveType.Sprint;
        }

        if (horizontalCollison)
        {
            return RabbitMoveType.Step;
        }

        return RabbitMoveType.Hop;
    }

    private bool TryAvoidNearbyWolf()
    {
        EntityWolf? wolf = FindNearestWolf();
        if (wolf == null)
        {
            return false;
        }

        _panicTicks = 40;
        if (random.NextInt(4) == 0 || !hasPath())
        {
            TrySetFleePath(wolf);
        }

        return true;
    }

    private EntityWolf? FindNearestWolf()
    {
        List<EntityWolf> wolves = world.Entities.CollectEntitiesOfType<EntityWolf>(boundingBox.Expand(WolfAvoidRange, 4.0D, WolfAvoidRange));
        EntityWolf? nearestWolf = null;
        double closestDistance = double.MaxValue;

        for (int i = 0; i < wolves.Count; ++i)
        {
            EntityWolf wolf = wolves[i];
            if (!wolf.isAlive())
            {
                continue;
            }

            double distance = wolf.getSquaredDistance(this);
            if (distance < closestDistance)
            {
                nearestWolf = wolf;
                closestDistance = distance;
            }
        }

        return nearestWolf;
    }

    private bool TrySetFleePath(EntityWolf wolf)
    {
        double awayX = x - wolf.x;
        double awayZ = z - wolf.z;
        if (awayX * awayX + awayZ * awayZ < 1.0E-4D)
        {
            awayX = random.NextFloat() - 0.5F;
            awayZ = random.NextFloat() - 0.5F;
        }

        for (int attempt = 0; attempt < 8; ++attempt)
        {
            double scale = 8.0D + random.NextInt(8);
            int targetX = MathHelper.Floor(x + awayX * scale + (random.NextFloat() - 0.5F) * 4.0F);
            int targetY = MathHelper.Floor(boundingBox.MinY);
            int targetZ = MathHelper.Floor(z + awayZ * scale + (random.NextFloat() - 0.5F) * 4.0F);
            var path = world.Pathing.findPath(this, targetX, targetY, targetZ, 16.0F);
            if (path != null)
            {
                setPathToEntity(path);
                return true;
            }
        }

        return false;
    }

    private void UpdateRaidTarget()
    {
        if (_carrotCooldown > 0)
        {
            ClearRaidTarget();
            return;
        }

        if (_hasRaidTarget)
        {
            if (!IsRipeCarrot(_raidTargetX, _raidTargetY, _raidTargetZ))
            {
                ClearRaidTarget();
                return;
            }

            double targetDx = _raidTargetX + 0.5D - x;
            double targetDz = _raidTargetZ + 0.5D - z;
            if (targetDx * targetDx + targetDz * targetDz < 2.25D)
            {
                EatRaidTarget();
                return;
            }

            if (!hasPath() || random.NextInt(20) == 0)
            {
                var path = world.Pathing.findPath(this, _raidTargetX, _raidTargetY, _raidTargetZ, CarrotRaidRange);
                if (path != null)
                {
                    setPathToEntity(path);
                }
            }

            return;
        }

        if (TryFindRaidTarget(out int targetX, out int targetY, out int targetZ))
        {
            _hasRaidTarget = true;
            _raidTargetX = targetX;
            _raidTargetY = targetY;
            _raidTargetZ = targetZ;
            var path = world.Pathing.findPath(this, targetX, targetY, targetZ, CarrotRaidRange);
            if (path != null)
            {
                setPathToEntity(path);
            }
        }
    }

    private bool TryFindRaidTarget(out int targetX, out int targetY, out int targetZ)
    {
        targetX = targetY = targetZ = 0;
        int bestDistance = int.MaxValue;
        int floorY = MathHelper.Floor(boundingBox.MinY);

        for (int dx = -CarrotRaidRange; dx <= CarrotRaidRange; ++dx)
        {
            for (int dz = -CarrotRaidRange; dz <= CarrotRaidRange; ++dz)
            {
                int worldX = MathHelper.Floor(x) + dx;
                int worldZ = MathHelper.Floor(z) + dz;

                for (int soilY = floorY - 1; soilY <= floorY + 1; ++soilY)
                {
                    if (world.Reader.GetBlockId(worldX, soilY, worldZ) != Block.Farmland.id)
                    {
                        continue;
                    }

                    int cropY = soilY + 1;
                    if (!IsRipeCarrot(worldX, cropY, worldZ))
                    {
                        continue;
                    }

                    int distance = dx * dx + dz * dz;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        targetX = worldX;
                        targetY = cropY;
                        targetZ = worldZ;
                    }
                }
            }
        }

        return bestDistance != int.MaxValue;
    }

    private bool IsRipeCarrot(int x, int y, int z)
    {
        return world.Reader.GetBlockId(x, y, z) == Block.Carrot.id && world.Reader.GetBlockMeta(x, y, z) >= 7;
    }

    private void EatRaidTarget()
    {
        if (IsRipeCarrot(_raidTargetX, _raidTargetY, _raidTargetZ))
        {
            int meta = world.Reader.GetBlockMeta(_raidTargetX, _raidTargetY, _raidTargetZ);
            Block.Blocks[Block.Carrot.id].dropStacks(new OnDropEvent(world, _raidTargetX, _raidTargetY, _raidTargetZ, meta));
            world.Writer.SetBlock(_raidTargetX, _raidTargetY, _raidTargetZ, 0);
            _carrotCooldown = 100;
        }

        ClearRaidTarget();
    }

    private void ClearRaidTarget()
    {
        _hasRaidTarget = false;
    }

    private static RabbitMoveData GetMoveData(RabbitMoveType moveType)
    {
        return moveType switch
        {
            RabbitMoveType.None => new RabbitMoveData(0.0F, 0.0F, 30, 1),
            RabbitMoveType.Hop => new RabbitMoveData(1.2F, 0.2F, 20, 10),
            RabbitMoveType.Step => new RabbitMoveData(1.5F, 0.45F, 14, 14),
            RabbitMoveType.Sprint => new RabbitMoveData(2.625F, 0.4F, 1, 8),
            RabbitMoveType.Attack => new RabbitMoveData(3.0F, 0.7F, 7, 8),
            _ => new RabbitMoveData(1.2F, 0.2F, 20, 10)
        };
    }
}
