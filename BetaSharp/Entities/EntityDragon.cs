using BetaSharp.Blocks;
using BetaSharp.NBT;
using BetaSharp.Util;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityDragon : EntityDragonBase, IBossDisplayData
{
    private readonly double[][] _movementOffsets = new double[64][];
    private int _movementOffsetIndex = -1;
    private bool _forceNewTarget;
    private bool _slowedByBlocks;
    private Entity? _targetEntity;
    private bool _isDying;
    private int _deathTicks;
    private readonly SyncedProperty<int> _syncedHealth;

    public override EntityType Type => EntityRegistry.EnderDragon;

    public EntityDragonPart[] DragonPartArray { get; }
    public EntityDragonPart DragonPartHead { get; }
    public EntityDragonPart DragonPartBody { get; }
    public EntityDragonPart DragonPartTail1 { get; }
    public EntityDragonPart DragonPartTail2 { get; }
    public EntityDragonPart DragonPartTail3 { get; }
    public EntityDragonPart DragonPartWing1 { get; }
    public EntityDragonPart DragonPartWing2 { get; }

    public float LastFlapTime { get; private set; }
    public float FlapTime { get; private set; }
    public EntityEnderCrystal? HealingEnderCrystal { get; private set; }
    public int BossHealth => world.IsRemote ? _syncedHealth.Value : health;
    public int MaxBossHealth => maxHealth;
    public string BossName => "Ender Dragon";
    public int DeathTicks => _deathTicks;

    public double TargetX { get; private set; }
    public double TargetY { get; private set; }
    public double TargetZ { get; private set; }

    public EntityDragon(IWorldContext world) : base(world)
    {
        for (int i = 0; i < _movementOffsets.Length; ++i)
        {
            _movementOffsets[i] = new double[3];
        }

        DragonPartHead = new EntityDragonPart(this, "head", 6.0F, 6.0F);
        DragonPartBody = new EntityDragonPart(this, "body", 8.0F, 8.0F);
        DragonPartTail1 = new EntityDragonPart(this, "tail", 4.0F, 4.0F);
        DragonPartTail2 = new EntityDragonPart(this, "tail", 4.0F, 4.0F);
        DragonPartTail3 = new EntityDragonPart(this, "tail", 4.0F, 4.0F);
        DragonPartWing1 = new EntityDragonPart(this, "wing", 4.0F, 4.0F);
        DragonPartWing2 = new EntityDragonPart(this, "wing", 4.0F, 4.0F);
        DragonPartArray =
        [
            DragonPartHead,
            DragonPartBody,
            DragonPartTail1,
            DragonPartTail2,
            DragonPartTail3,
            DragonPartWing1,
            DragonPartWing2,
        ];

        maxHealth = 200;
        health = 200;
        _syncedHealth = DataSynchronizer.MakeProperty<int>(16, health);
        texture = "/mob/enderdragon/ender.png";
        setBoundingBoxSpacing(16.0F, 8.0F);
        noClip = true;
        isImmuneToFire = true;
        TargetY = 100.0D;
        ignoreFrustumCheck = true;
        SyncHealthWatcher();
    }

    protected override bool canDespawn() => false;

    public override int getMaxSpawnedInChunk() => 1;

    public override bool canSpawn() => true;

    protected override string getHurtSound() => null!;

    protected override string getDeathSound() => null!;

    public double[] GetMovementOffsets(int index, float tickDelta)
    {
        if (_isDying)
        {
            tickDelta = 0.0F;
        }

        if (_movementOffsetIndex < 0)
        {
            return new[] { (double)yaw, y, z };
        }

        int currentIndex = _movementOffsetIndex - index & 63;
        int previousIndex = _movementOffsetIndex - index - 1 & 63;
        double[] current = _movementOffsets[currentIndex];
        double[] previous = _movementOffsets[previousIndex];
        double[] values = new double[3];
        double currentYaw = current[0];
        double yawDelta = previous[0] - currentYaw;
        while (yawDelta < -180.0D)
        {
            yawDelta += 360.0D;
        }

        while (yawDelta >= 180.0D)
        {
            yawDelta -= 360.0D;
        }

        values[0] = currentYaw + yawDelta * (1.0F - tickDelta);
        values[1] = current[1] + (previous[1] - current[1]) * (1.0F - tickDelta);
        values[2] = current[2] + (previous[2] - current[2]) * (1.0F - tickDelta);
        return values;
    }

    public override bool damage(Entity entity, int amount)
    {
        if (world.IsRemote || dead || _isDying || amount <= 0)
        {
            return false;
        }

        hurtTime = maxHurtTime = 10;
        attackedAtYaw = 0.0F;
        world.Broadcaster.EntityEvent(this, 2);
        health -= amount;
        SyncHealthWatcher();
        if (health <= 0)
        {
            health = 1;
            _isDying = true;
            _deathTicks = 0;
            deathTime = 1;
            SyncHealthWatcher();
            world.Broadcaster.EntityEvent(this, 3);
        }

        return true;
    }

    public override bool DamageFromPart(EntityDragonPart part, Entity? source, int amount)
    {
        if (part != DragonPartHead)
        {
            amount = amount / 4 + 1;
        }

        float yawRadians = yaw * (float)Math.PI / 180.0F;
        float sinYaw = MathHelper.Sin(yawRadians);
        float cosYaw = MathHelper.Cos(yawRadians);
        TargetX = x + sinYaw * 5.0F + (random.NextFloat() - 0.5F) * 2.0F;
        TargetY = y + random.NextFloat() * 3.0F + 1.0D;
        TargetZ = z - cosYaw * 5.0F + (random.NextFloat() - 0.5F) * 2.0F;
        _targetEntity = null;
        if (source is EntityArrow arrow && arrow.owner != null)
        {
            source = arrow.owner;
        }

        if (source is EntityPlayer || source == null)
        {
            return damage(source!, amount);
        }

        return true;
    }

    public override void processServerEntityStatus(sbyte statusId)
    {
        if (statusId == 2)
        {
            hurtTime = maxHurtTime = 10;
        }
        else if (statusId == 3)
        {
            _isDying = true;
            _deathTicks = Math.Max(_deathTicks, 1);
            deathTime = _deathTicks;
        }
        else
        {
            base.processServerEntityStatus(statusId);
        }
    }

    public override void tickMovement()
    {
        if (world.IsRemote)
        {
            health = _syncedHealth.Value;
        }
        else
        {
            SyncHealthWatcher();
        }

        LastFlapTime = FlapTime;
        if (_isDying)
        {
            TickDeathSequence();
        }
        else
        {
            UpdateHealingCrystal();
            float flapSpeed = 0.2F / (MathHelper.Sqrt(velocityX * velocityX + velocityZ * velocityZ) * 10.0F + 1.0F);
            flapSpeed *= (float)Math.Pow(2.0D, velocityY);
            FlapTime += _slowedByBlocks ? flapSpeed * 0.5F : flapSpeed;

            while (yaw >= 180.0F)
            {
                yaw -= 360.0F;
            }

            while (yaw < -180.0F)
            {
                yaw += 360.0F;
            }

            if (_movementOffsetIndex < 0)
            {
                for (int i = 0; i < _movementOffsets.Length; ++i)
                {
                    _movementOffsets[i] = new[] { (double)yaw, y, 0.0D };
                }
            }

            if (++_movementOffsetIndex == _movementOffsets.Length)
            {
                _movementOffsetIndex = 0;
            }

            _movementOffsets[_movementOffsetIndex][0] = yaw;
            _movementOffsets[_movementOffsetIndex][1] = y;
            _movementOffsets[_movementOffsetIndex][2] = z;

            if (world.IsRemote)
            {
                if (newPosRotationIncrements > 0)
                {
                    double targetPosX = x + (newPosX - x) / newPosRotationIncrements;
                    double targetPosY = y + (newPosY - y) / newPosRotationIncrements;
                    double targetPosZ = z + (newPosZ - z) / newPosRotationIncrements;
                    double yawDelta = newRotationYaw - yaw;
                    while (yawDelta < -180.0D)
                    {
                        yawDelta += 360.0D;
                    }

                    while (yawDelta >= 180.0D)
                    {
                        yawDelta -= 360.0D;
                    }

                    yaw = (float)(yaw + yawDelta / newPosRotationIncrements);
                    pitch = (float)(pitch + (newRotationPitch - pitch) / newPosRotationIncrements);
                    --newPosRotationIncrements;
                    setPosition(targetPosX, targetPosY, targetPosZ);
                    setRotation(yaw, pitch);
                }
            }
            else
            {
                UpdateFlightTarget();
            }

            UpdateDragonParts();

            if (!world.IsRemote)
            {
                _slowedByBlocks = DestroyBlocksInAABB(DragonPartHead.boundingBox) | DestroyBlocksInAABB(DragonPartBody.boundingBox);
                if (hurtTime == 0)
                {
                    PushNearbyEntities(world.Entities.GetEntities(this, DragonPartWing1.boundingBox.Expand(4.0D, 2.0D, 4.0D).Offset(0.0D, -2.0D, 0.0D)));
                    PushNearbyEntities(world.Entities.GetEntities(this, DragonPartWing2.boundingBox.Expand(4.0D, 2.0D, 4.0D).Offset(0.0D, -2.0D, 0.0D)));
                    AttackNearbyEntities(world.Entities.GetEntities(this, DragonPartHead.boundingBox.Expand(1.0D, 1.0D, 1.0D)));
                }
            }
        }
    }

    private void TickDeathSequence()
    {
        ++_deathTicks;
        deathTime = _deathTicks;
        SyncHealthWatcher();
        if (_deathTicks >= 180 && _deathTicks <= 200)
        {
            float offsetX = (random.NextFloat() - 0.5F) * 8.0F;
            float offsetY = (random.NextFloat() - 0.5F) * 4.0F;
            float offsetZ = (random.NextFloat() - 0.5F) * 8.0F;
            world.Broadcaster.AddParticle("hugeexplosion", x + offsetX, y + 2.0D + offsetY, z + offsetZ, 0.0D, 0.0D, 0.0D);
        }

        move(0.0D, 0.1D, 0.0D);
        bodyYaw = yaw += 20.0F;
        UpdateDragonParts();
        if (!world.IsRemote && _deathTicks == 200)
        {
            CreateExitPortal(MathHelper.Floor(x), MathHelper.Floor(z));
            markDead();
        }
    }

    private void UpdateFlightTarget()
    {
        double deltaX = TargetX - x;
        double deltaY = TargetY - y;
        double deltaZ = TargetZ - z;
        double distanceSq = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        if (_targetEntity != null)
        {
            TargetX = _targetEntity.x;
            TargetZ = _targetEntity.z;
            double targetDeltaX = TargetX - x;
            double targetDeltaZ = TargetZ - z;
            double targetDistance = Math.Sqrt(targetDeltaX * targetDeltaX + targetDeltaZ * targetDeltaZ);
            double heightOffset = 0.4F + targetDistance / 80.0D - 1.0D;
            if (heightOffset > 10.0D)
            {
                heightOffset = 10.0D;
            }

            TargetY = _targetEntity.boundingBox.MinY + heightOffset;
            deltaX = TargetX - x;
            deltaY = TargetY - y;
            deltaZ = TargetZ - z;
            distanceSq = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        }
        else
        {
            TargetX += random.NextGaussian() * 2.0D;
            TargetZ += random.NextGaussian() * 2.0D;
        }

        if (_forceNewTarget || distanceSq < 100.0D || distanceSq > 22500.0D || horizontalCollison || verticalCollision)
        {
            ChooseNewTarget();
            deltaX = TargetX - x;
            deltaY = TargetY - y;
            deltaZ = TargetZ - z;
        }

        deltaY /= MathHelper.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
        deltaY = Math.Clamp(deltaY, -0.6D, 0.6D);
        velocityY += deltaY * 0.1D;

        double targetYaw = 180.0D - Math.Atan2(deltaX, deltaZ) * 180.0D / Math.PI;
        double yawDelta = SimplifyAngle(targetYaw - yaw);
        yawDelta = Math.Clamp(yawDelta, -50.0D, 50.0D);

        Vec3D targetVector = new(TargetX - x, TargetY - y, TargetZ - z);
        targetVector = targetVector.normalize();
        Vec3D motionVector = new(MathHelper.Sin(yaw * (float)Math.PI / 180.0F), velocityY, -MathHelper.Cos(yaw * (float)Math.PI / 180.0F));
        motionVector = motionVector.normalize();
        float alignment = (float)(motionVector.dotProduct(targetVector) + 0.5D) / 1.5F;
        if (alignment < 0.0F)
        {
            alignment = 0.0F;
        }

        rotationOffset *= 0.8F;
        float horizontalSpeed = MathHelper.Sqrt(velocityX * velocityX + velocityZ * velocityZ) + 1.0F;
        double horizontalSpeedExact = Math.Sqrt(velocityX * velocityX + velocityZ * velocityZ) + 1.0D;
        if (horizontalSpeedExact > 40.0D)
        {
            horizontalSpeedExact = 40.0D;
        }

        rotationOffset = (float)(rotationOffset + yawDelta * (0.7F / horizontalSpeedExact / horizontalSpeed));
        yaw += rotationOffset * 0.1F;
        float accelerationFactor = (float)(2.0D / (horizontalSpeedExact + 1.0D));
        moveNonSolid(0.0F, -1.0F, 0.06F * (alignment * accelerationFactor + (1.0F - accelerationFactor)));
        if (_slowedByBlocks)
        {
            move(velocityX * 0.8D, velocityY * 0.8D, velocityZ * 0.8D);
        }
        else
        {
            move(velocityX, velocityY, velocityZ);
        }

        Vec3D adjustedMotion = new Vec3D(velocityX, velocityY, velocityZ).normalize();
        float drag = (float)(adjustedMotion.dotProduct(motionVector) + 1.0D) / 2.0F;
        drag = 0.8F + 0.15F * drag;
        velocityX *= drag;
        velocityZ *= drag;
        velocityY *= 0.91D;
        bodyYaw = yaw;
    }

    private void UpdateDragonParts()
    {
        DragonPartHead.width = DragonPartHead.height = 3.0F;
        DragonPartTail1.width = DragonPartTail1.height = 2.0F;
        DragonPartTail2.width = DragonPartTail2.height = 2.0F;
        DragonPartTail3.width = DragonPartTail3.height = 2.0F;
        DragonPartBody.height = 3.0F;
        DragonPartBody.width = 5.0F;
        DragonPartWing1.height = 2.0F;
        DragonPartWing1.width = 4.0F;
        DragonPartWing2.height = 3.0F;
        DragonPartWing2.width = 4.0F;

        float neckPitch = (float)(GetMovementOffsets(5, 1.0F)[1] - GetMovementOffsets(10, 1.0F)[1]) * 10.0F / 180.0F * (float)Math.PI;
        float cosPitch = MathHelper.Cos(neckPitch);
        float sinPitch = -MathHelper.Sin(neckPitch);
        float yawRadians = yaw * (float)Math.PI / 180.0F;
        float sinYaw = MathHelper.Sin(yawRadians);
        float cosYaw = MathHelper.Cos(yawRadians);

        UpdatePart(DragonPartBody, x + sinYaw * 0.5F, y, z - cosYaw * 0.5F);
        UpdatePart(DragonPartWing1, x + cosYaw * 4.5F, y + 2.0D, z + sinYaw * 4.5F);
        UpdatePart(DragonPartWing2, x - cosYaw * 4.5F, y + 2.0D, z - sinYaw * 4.5F);

        double[] headOffsets = GetMovementOffsets(5, 1.0F);
        double[] baseOffsets = GetMovementOffsets(0, 1.0F);
        float headYaw = MathHelper.Sin(yawRadians - rotationOffset * 0.01F);
        float headCosYaw = MathHelper.Cos(yawRadians - rotationOffset * 0.01F);
        UpdatePart(
            DragonPartHead,
            x + headYaw * 5.5F * cosPitch,
            y + (baseOffsets[1] - headOffsets[1]) + sinPitch * 5.5F,
            z - headCosYaw * 5.5F * cosPitch);

        for (int i = 0; i < 3; ++i)
        {
            EntityDragonPart part = i switch
            {
                0 => DragonPartTail1,
                1 => DragonPartTail2,
                _ => DragonPartTail3,
            };

            double[] tailOffsets = GetMovementOffsets(12 + i * 2, 1.0F);
            float tailYaw = yawRadians + SimplifyAngle(tailOffsets[0] - headOffsets[0]) * (float)Math.PI / 180.0F;
            float tailSin = MathHelper.Sin(tailYaw);
            float tailCos = MathHelper.Cos(tailYaw);
            float tailBase = 1.5F;
            float tailOffset = (i + 1) * 2.0F;
            UpdatePart(
                part,
                x - (sinYaw * tailBase + tailSin * tailOffset) * cosPitch,
                y + (tailOffsets[1] - headOffsets[1]) - (tailOffset + tailBase) * sinPitch + 1.5D,
                z + (cosYaw * tailBase + tailCos * tailOffset) * cosPitch);
        }
    }

    private void UpdatePart(EntityDragonPart part, double partX, double partY, double partZ)
    {
        part.prevX = part.x;
        part.prevY = part.y;
        part.prevZ = part.z;
        part.setPosition(partX, partY, partZ);
    }

    private void UpdateHealingCrystal()
    {
        if (HealingEnderCrystal != null)
        {
            if (HealingEnderCrystal.dead)
            {
                HealingEnderCrystal = null;
                damage(null!, 10);
            }
            else if (age % 10 == 0 && health < maxHealth)
            {
                ++health;
                SyncHealthWatcher();
            }
        }

        if (random.NextInt(10) == 0)
        {
            List<EntityEnderCrystal> crystals = world.Entities.CollectEntitiesOfType<EntityEnderCrystal>(boundingBox.Expand(32.0D, 32.0D, 32.0D));
            EntityEnderCrystal? closestCrystal = null;
            double closestDistance = double.MaxValue;
            for (int i = 0; i < crystals.Count; ++i)
            {
                EntityEnderCrystal crystal = crystals[i];
                double distance = crystal.getSquaredDistance(this);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCrystal = crystal;
                }
            }

            HealingEnderCrystal = closestCrystal;
        }
    }

    private void PushNearbyEntities(List<Entity> entities)
    {
        double centerX = (DragonPartBody.boundingBox.MinX + DragonPartBody.boundingBox.MaxX) / 2.0D;
        double centerZ = (DragonPartBody.boundingBox.MinZ + DragonPartBody.boundingBox.MaxZ) / 2.0D;
        for (int i = 0; i < entities.Count; ++i)
        {
            Entity entity = entities[i];
            if (entity is EntityLiving)
            {
                double deltaX = entity.x - centerX;
                double deltaZ = entity.z - centerZ;
                double magnitude = deltaX * deltaX + deltaZ * deltaZ;
                if (magnitude < 1.0E-4D)
                {
                    continue;
                }

                entity.addVelocity(deltaX / magnitude * 4.0D, 0.2F, deltaZ / magnitude * 4.0D);
            }
        }
    }

    private void AttackNearbyEntities(List<Entity> entities)
    {
        for (int i = 0; i < entities.Count; ++i)
        {
            Entity entity = entities[i];
            if (entity is EntityLiving)
            {
                entity.damage(this, 10);
            }
        }
    }

    private void ChooseNewTarget()
    {
        _forceNewTarget = false;
        List<EntityPlayer> targetablePlayers = [];
        for (int i = 0; i < world.Entities.Players.Count; ++i)
        {
            if (world.Entities.Players[i].GameMode.CanBeTargeted)
            {
                targetablePlayers.Add(world.Entities.Players[i]);
            }
        }

        if (random.NextInt(2) == 0 && targetablePlayers.Count > 0)
        {
            _targetEntity = targetablePlayers[random.NextInt(targetablePlayers.Count)];
        }
        else
        {
            bool validTarget;
            do
            {
                TargetX = random.NextFloat() * 120.0F - 60.0F;
                TargetY = 70.0F + random.NextFloat() * 50.0F;
                TargetZ = random.NextFloat() * 120.0F - 60.0F;
                double deltaX = x - TargetX;
                double deltaY = y - TargetY;
                double deltaZ = z - TargetZ;
                validTarget = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ > 100.0D;
            } while (!validTarget);

            _targetEntity = null;
        }
    }

    private bool DestroyBlocksInAABB(Box area)
    {
        int minX = MathHelper.Floor(area.MinX);
        int minY = MathHelper.Floor(area.MinY);
        int minZ = MathHelper.Floor(area.MinZ);
        int maxX = MathHelper.Floor(area.MaxX);
        int maxY = MathHelper.Floor(area.MaxY);
        int maxZ = MathHelper.Floor(area.MaxZ);
        bool hitIndestructible = false;
        bool destroyed = false;

        for (int x = minX; x <= maxX; ++x)
        {
            for (int y = minY; y <= maxY; ++y)
            {
                for (int z = minZ; z <= maxZ; ++z)
                {
                    int blockId = world.Reader.GetBlockId(x, y, z);
                    if (blockId == 0)
                    {
                        continue;
                    }

                    if (blockId == Block.Obsidian.id || blockId == Block.EndStone.id || blockId == Block.Bedrock.id)
                    {
                        hitIndestructible = true;
                    }
                    else
                    {
                        destroyed = true;
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, 0, 0, false);
                    }
                }
            }
        }

        if (destroyed)
        {
            double particleX = area.MinX + (area.MaxX - area.MinX) * random.NextFloat();
            double particleY = area.MinY + (area.MaxY - area.MinY) * random.NextFloat();
            double particleZ = area.MinZ + (area.MaxZ - area.MinZ) * random.NextFloat();
            world.Broadcaster.AddParticle("largeexplode", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
        }

        return hitIndestructible;
    }

    private void CreateExitPortal(int centerX, int centerZ)
    {
        const int portalY = 64;
        BlockEndPortal.BossDefeated = true;
        const int radius = 4;

        for (int y = portalY - 1; y <= portalY + 32; ++y)
        {
            for (int x = centerX - radius; x <= centerX + radius; ++x)
            {
                for (int z = centerZ - radius; z <= centerZ + radius; ++z)
                {
                    double deltaX = x - centerX;
                    double deltaZ = z - centerZ;
                    double distance = Math.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
                    if (distance > radius - 0.5D)
                    {
                        continue;
                    }

                    if (y < portalY)
                    {
                        if (distance <= radius - 1 - 0.5D)
                        {
                            world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Bedrock.id, 0, false);
                        }
                    }
                    else if (y > portalY)
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, 0, 0, false);
                    }
                    else if (distance > radius - 1 - 0.5D)
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.Bedrock.id, 0, false);
                    }
                    else
                    {
                        world.Writer.SetBlockWithoutNotifyingNeighbors(x, y, z, Block.EndPortal.id, 0, true);
                    }
                }
            }
        }

        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY, centerZ, Block.Bedrock.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY + 1, centerZ, Block.Bedrock.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY + 2, centerZ, Block.Bedrock.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX - 1, portalY + 2, centerZ, Block.Torch.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX + 1, portalY + 2, centerZ, Block.Torch.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY + 2, centerZ - 1, Block.Torch.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY + 2, centerZ + 1, Block.Torch.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY + 3, centerZ, Block.Bedrock.id, 0, false);
        world.Writer.SetBlockWithoutNotifyingNeighbors(centerX, portalY + 4, centerZ, Block.DragonEgg.id, 0, false);
        BlockEndPortal.BossDefeated = false;
    }

    public void ForceKillForTesting()
    {
        if (dead)
        {
            return;
        }

        _isDying = false;
        _deathTicks = 200;
        deathTime = _deathTicks;
        health = 0;
        SyncHealthWatcher();

        if (!world.IsRemote)
        {
            CreateExitPortal(MathHelper.Floor(x), MathHelper.Floor(z));
        }

        markDead();
    }

    private static float SimplifyAngle(double angle)
    {
        while (angle >= 180.0D)
        {
            angle -= 360.0D;
        }

        while (angle < -180.0D)
        {
            angle += 360.0D;
        }

        return (float)angle;
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetBoolean("DragonDying", _isDying);
        nbt.SetInteger("DragonDeathTicks", _deathTicks);
        nbt.SetDouble("DragonTargetX", TargetX);
        nbt.SetDouble("DragonTargetY", TargetY);
        nbt.SetDouble("DragonTargetZ", TargetZ);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        _isDying = nbt.GetBoolean("DragonDying");
        _deathTicks = nbt.GetInteger("DragonDeathTicks");
        TargetX = nbt.GetDouble("DragonTargetX");
        TargetY = nbt.GetDouble("DragonTargetY");
        TargetZ = nbt.GetDouble("DragonTargetZ");
        deathTime = _deathTicks;
        SyncHealthWatcher();
    }

    private void SyncHealthWatcher()
    {
        if (!world.IsRemote)
        {
            _syncedHealth.Value = health;
        }
    }
}
