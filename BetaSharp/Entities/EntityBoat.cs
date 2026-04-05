using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Items;
using BetaSharp.NBT;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public class EntityBoat : Entity
{
    private enum BoatStatus
    {
        InWater,
        UnderWater,
        UnderFlowingWater,
        OnLand,
        InAir
    }

    public override EntityType Type => EntityRegistry.Boat;
    public int boatCurrentDamage;
    public int boatTimeSinceHit;
    public int boatRockDirection;
    private BoatStatus _status = BoatStatus.InAir;
    private BoatStatus _previousStatus = BoatStatus.InAir;
    private float _boatGlide;
    private float _deltaRotation;
    private double _waterLevel;
    private readonly float[] _paddlePositions = new float[2];
    private readonly bool[] _paddleMoving = new bool[2];
    private int lerpSteps;
    private double targetX;
    private double targetY;
    private double targetZ;
    private double targetYaw;
    private double targetPitch;
    private double boatVelocityX;
    private double boatVelocityY;
    private double boatVelocityZ;
    private double _lastYVelocity;

    public EntityBoat(IWorldContext world) : base(world)
    {
        boatCurrentDamage = 0;
        boatTimeSinceHit = 0;
        boatRockDirection = 1;
        preventEntitySpawning = true;
        setBoundingBoxSpacing(1.375F, 0.5625F);
        standingEyeHeight = height / 2.0F;
    }

    protected override bool bypassesSteppingEffects()
    {
        return false;
    }


    public override Box? getCollisionAgainstShape(Entity entity)
    {
        return entity.boundingBox;
    }

    public override Box? getBoundingBox()
    {
        return boundingBox;
    }

    public override bool isPushable()
    {
        return true;
    }

    public EntityBoat(IWorldContext world, double x, double y, double z) : this(world)
    {
        setPosition(x, y, z);
        velocityX = 0.0D;
        velocityY = 0.0D;
        velocityZ = 0.0D;
        prevX = x;
        prevY = y;
        prevZ = z;
    }

    public override double getPassengerRidingHeight()
    {
        return -0.1D;
    }

    public override bool damage(Entity entity, int amount)
    {
        if (!world.IsRemote && !dead)
        {
            boatRockDirection = -boatRockDirection;
            boatTimeSinceHit = 10;
            boatCurrentDamage += amount * 10;
            scheduleVelocityUpdate();
            if (boatCurrentDamage > 40)
            {
                if (passenger != null)
                {
                    passenger.setVehicle(this);
                }

                dropItem(Item.Boat.id, 1, 0.0F);
                markDead();
            }

            return true;
        }

        return true;
    }

    public override void animateHurt()
    {
        boatRockDirection = -boatRockDirection;
        boatTimeSinceHit = 10;
        boatCurrentDamage += boatCurrentDamage * 10;
    }

    public override bool isCollidable()
    {
        return !dead;
    }

    public override void setPositionAndAnglesAvoidEntities(double targetX, double targetY, double targetZ, float targetYaw, float targetPitch, int lerpSteps)
    {
        this.targetX = targetX;
        this.targetY = targetY;
        this.targetZ = targetZ;
        this.targetYaw = targetYaw;
        this.targetPitch = targetPitch;
        this.lerpSteps = 10;
        velocityX = boatVelocityX;
        velocityY = boatVelocityY;
        velocityZ = boatVelocityZ;
    }

    public override void setVelocityClient(double velocityX, double velocityY, double velocityZ)
    {
        boatVelocityX = base.velocityX = velocityX;
        boatVelocityY = base.velocityY = velocityY;
        boatVelocityZ = base.velocityZ = velocityZ;
    }

    public override void tick()
    {
        base.tick();
        _previousStatus = _status;
        _status = GetBoatStatus();

        if (boatTimeSinceHit > 0)
        {
            --boatTimeSinceHit;
        }

        if (boatCurrentDamage > 0)
        {
            --boatCurrentDamage;
        }

        if (world.IsRemote)
        {
            TickClient();
            return;
        }

        if (passenger != null && passenger.dead)
        {
            passenger = null;
        }

        UpdateMotion();
        ControlBoat();
        TickPaddles();
        move(velocityX, velocityY, velocityZ);
        UpdateBoatRotation();
        SpawnSplashParticles();
        PushOtherBoats();
        ClearSnowLayers();
        _lastYVelocity = velocityY;
    }

    private void TickClient()
    {
        if (passenger is EntityPlayer player && player.IsLocalPlayer)
        {
            lerpSteps = 0;
            UpdateMotion();
            ControlBoat();
            TickPaddles();
            move(velocityX, velocityY, velocityZ);
            UpdateBoatRotation();
            _lastYVelocity = velocityY;
            return;
        }

        UpdatePaddlesFromPassenger();
        if (lerpSteps > 0)
        {
            double x = this.x + (targetX - this.x) / lerpSteps;
            double y = this.y + (targetY - this.y) / lerpSteps;
            double z = this.z + (targetZ - this.z) / lerpSteps;
            double yawDelta = targetYaw - yaw;
            while (yawDelta < -180.0D)
            {
                yawDelta += 360.0D;
            }

            while (yawDelta >= 180.0D)
            {
                yawDelta -= 360.0D;
            }

            yaw = (float)(yaw + yawDelta / lerpSteps);
            pitch = (float)(pitch + (targetPitch - pitch) / lerpSteps);
            --lerpSteps;
            setPosition(x, y, z);
            setRotation(yaw, pitch);
            return;
        }

        setPosition(x + velocityX, y + velocityY, z + velocityZ);
        velocityX *= 0.95D;
        velocityY *= 0.95D;
        velocityZ *= 0.95D;
    }

    private BoatStatus GetBoatStatus()
    {
        BoatStatus? underWaterStatus = GetUnderWaterStatus();
        if (underWaterStatus != null)
        {
            _waterLevel = boundingBox.MaxY;
            return underWaterStatus.Value;
        }

        if (CheckInWater())
        {
            return BoatStatus.InWater;
        }

        _boatGlide = GetBoatGlide();
        return _boatGlide > 0.0F ? BoatStatus.OnLand : BoatStatus.InAir;
    }

    private bool CheckInWater()
    {
        _waterLevel = double.MinValue;
        bool inWater = false;
        int minX = MathHelper.Floor(boundingBox.MinX);
        int maxX = (int)System.Math.Ceiling(boundingBox.MaxX);
        int minY = MathHelper.Floor(boundingBox.MinY);
        int maxY = (int)System.Math.Ceiling(boundingBox.MinY + 0.001D);
        int minZ = MathHelper.Floor(boundingBox.MinZ);
        int maxZ = (int)System.Math.Ceiling(boundingBox.MaxZ);

        for (int x = minX; x < maxX; ++x)
        {
            for (int y = minY; y < maxY; ++y)
            {
                for (int z = minZ; z < maxZ; ++z)
                {
                    double surfaceY = GetWaterSurfaceY(x, y, z);
                    if (surfaceY <= double.MinValue)
                    {
                        continue;
                    }

                    _waterLevel = Math.Max(_waterLevel, surfaceY);
                    inWater |= boundingBox.MinY < surfaceY;
                }
            }
        }

        return inWater;
    }

    private BoatStatus? GetUnderWaterStatus()
    {
        int minX = MathHelper.Floor(boundingBox.MinX);
        int maxX = (int)System.Math.Ceiling(boundingBox.MaxX);
        int minY = MathHelper.Floor(boundingBox.MaxY);
        int maxY = (int)System.Math.Ceiling(boundingBox.MaxY + 0.001D);
        int minZ = MathHelper.Floor(boundingBox.MinZ);
        int maxZ = (int)System.Math.Ceiling(boundingBox.MaxZ);

        for (int x = minX; x < maxX; ++x)
        {
            for (int y = minY; y < maxY; ++y)
            {
                for (int z = minZ; z < maxZ; ++z)
                {
                    int blockId = world.Reader.GetBlockId(x, y, z);
                    if (blockId != Block.Water.id && blockId != Block.FlowingWater.id)
                    {
                        continue;
                    }

                    double surfaceY = GetWaterSurfaceY(x, y, z);
                    if (boundingBox.MaxY < surfaceY)
                    {
                        return world.Reader.GetBlockMeta(x, y, z) == 0
                            ? BoatStatus.UnderWater
                            : BoatStatus.UnderFlowingWater;
                    }
                }
            }
        }

        return null;
    }

    private float GetBoatGlide()
    {
        int minX = MathHelper.Floor(boundingBox.MinX);
        int maxX = MathHelper.Floor(boundingBox.MaxX + 1.0D);
        int y = MathHelper.Floor(boundingBox.MinY) - 1;
        int minZ = MathHelper.Floor(boundingBox.MinZ);
        int maxZ = MathHelper.Floor(boundingBox.MaxZ + 1.0D);
        float slipperiness = 0.0F;
        int count = 0;

        for (int x = minX; x < maxX; ++x)
        {
            for (int z = minZ; z < maxZ; ++z)
            {
                int blockId = world.Reader.GetBlockId(x, y, z);
                if (blockId <= 0)
                {
                    continue;
                }

                slipperiness += Block.Blocks[blockId].slipperiness;
                ++count;
            }
        }

        return count > 0 ? slipperiness / count : 0.0F;
    }

    private void UpdateMotion()
    {
        double gravity = -0.03999999910593033D;
        double buoyancy = 0.0D;
        float momentum = 0.9F;

        if (_previousStatus == BoatStatus.InAir && _status != BoatStatus.InAir && _status != BoatStatus.OnLand)
        {
            setPosition(x, GetWaterLevelAbove() - height + 0.101D, z);
            velocityY = 0.0D;
            _lastYVelocity = 0.0D;
            _status = BoatStatus.InWater;
        }

        switch (_status)
        {
            case BoatStatus.InWater:
                buoyancy = (_waterLevel - boundingBox.MinY) / height;
                momentum = 0.9F;
                break;
            case BoatStatus.UnderFlowingWater:
                gravity = -0.0007D;
                momentum = 0.9F;
                break;
            case BoatStatus.UnderWater:
                buoyancy = 0.01D;
                momentum = 0.45F;
                break;
            case BoatStatus.OnLand:
                momentum = _boatGlide;
                if (passenger is EntityPlayer)
                {
                    _boatGlide *= 0.5F;
                }
                break;
            case BoatStatus.InAir:
                momentum = 0.9F;
                break;
        }

        velocityX *= momentum;
        velocityZ *= momentum;
        _deltaRotation *= momentum;
        velocityY += gravity;
        if (buoyancy > 0.0D)
        {
            velocityY += buoyancy * 0.06153846016296973D;
            velocityY *= 0.75D;
        }

        const double maxSpeed = 0.35D;
        velocityX = Math.Clamp(velocityX, -maxSpeed, maxSpeed);
        velocityZ = Math.Clamp(velocityZ, -maxSpeed, maxSpeed);
    }

    private double GetWaterLevelAbove()
    {
        int minX = MathHelper.Floor(boundingBox.MinX);
        int maxX = (int)System.Math.Ceiling(boundingBox.MaxX);
        int minZ = MathHelper.Floor(boundingBox.MinZ);
        int maxZ = (int)System.Math.Ceiling(boundingBox.MaxZ);
        int minY = MathHelper.Floor(boundingBox.MaxY);
        int maxY = (int)System.Math.Ceiling(boundingBox.MaxY - _lastYVelocity);
        if (maxY <= minY)
        {
            maxY = minY + 1;
        }

        double waterLevel = double.MinValue;
        for (int y = minY; y < maxY; ++y)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int z = minZ; z < maxZ; ++z)
                {
                    double surfaceY = GetWaterSurfaceY(x, y, z);
                    if (surfaceY <= double.MinValue)
                    {
                        continue;
                    }

                    waterLevel = Math.Max(waterLevel, surfaceY);
                }
            }

            if (waterLevel > double.MinValue)
            {
                return waterLevel;
            }
        }

        return boundingBox.MaxY;
    }

    private double GetWaterSurfaceY(int x, int y, int z)
    {
        int blockId = world.Reader.GetBlockId(x, y, z);
        if (blockId != Block.Water.id && blockId != Block.FlowingWater.id)
        {
            return double.MinValue;
        }

        return y + 1.0D - BlockFluid.getFluidHeightFromMeta(world.Reader.GetBlockMeta(x, y, z));
    }

    private void ControlBoat()
    {
        if (passenger is not EntityLiving rider)
        {
            UpdatePaddleInputs(0.0F, 0.0F);
            return;
        }

        float acceleration = 0.0F;
        float sidewaysInput = rider.GetSidewaysInput();
        float forwardInput = rider.GetForwardInput();
        bool leftInput = sidewaysInput < -0.01F;
        bool rightInput = sidewaysInput > 0.01F;
        bool forwardPressed = forwardInput > 0.0F;
        bool backPressed = forwardInput < 0.0F;

        if (leftInput)
        {
            _deltaRotation -= 1.0F;
        }

        if (rightInput)
        {
            _deltaRotation += 1.0F;
        }

        if (leftInput != rightInput && !forwardPressed && !backPressed)
        {
            acceleration += 0.005F;
        }

        yaw += _deltaRotation;

        if (forwardPressed)
        {
            acceleration += 0.04F;
        }

        if (backPressed)
        {
            acceleration -= 0.005F;
        }

        UpdatePaddleInputs(sidewaysInput, forwardInput);

        float yawRadians = -yaw * ((float)Math.PI / 180.0F);
        velocityX += MathHelper.Sin(yawRadians) * acceleration;
        velocityZ += MathHelper.Cos(yawRadians) * acceleration;
    }

    private void UpdateBoatRotation()
    {
        pitch = 0.0F;
        setRotation(yaw, pitch);
    }

    private void SpawnSplashParticles()
    {
        double speed = Math.Sqrt(velocityX * velocityX + velocityZ * velocityZ);
        if (speed <= 0.15D || _status == BoatStatus.OnLand || _status == BoatStatus.InAir)
        {
            return;
        }

        double cosYaw = Math.Cos(yaw * Math.PI / 180.0D);
        double sinYaw = Math.Sin(yaw * Math.PI / 180.0D);
        for (int i = 0; i < 1.0D + speed * 40.0D; ++i)
        {
            double randomOffset = random.NextFloat() * 2.0F - 1.0F;
            double sideOffset = (random.NextInt(2) * 2 - 1) * 0.7D;
            double particleX = x - cosYaw * randomOffset * 0.8D + sinYaw * sideOffset;
            double particleZ = z - sinYaw * randomOffset * 0.8D - cosYaw * sideOffset;
            world.Broadcaster.AddParticle("splash", particleX, y - 0.125D, particleZ, velocityX, velocityY, velocityZ);
        }
    }

    private void PushOtherBoats()
    {
        List<Entity> nearbyEntities = world.Entities.GetEntities(this, boundingBox.Expand(0.2D, 0.0D, 0.2D));
        for (int i = 0; i < nearbyEntities.Count; ++i)
        {
            Entity entity = nearbyEntities[i];
            if (entity != passenger && entity.isPushable() && entity is EntityBoat)
            {
                entity.onCollision(this);
            }
        }
    }

    private void ClearSnowLayers()
    {
        for (int i = 0; i < 4; ++i)
        {
            int blockX = MathHelper.Floor(x + ((i % 2) - 0.5D) * 0.8D);
            int blockY = MathHelper.Floor(y);
            int blockZ = MathHelper.Floor(z + ((i / 2) - 0.5D) * 0.8D);
            if (world.Reader.GetBlockId(blockX, blockY, blockZ) == Block.Snow.id)
            {
                world.Writer.SetBlock(blockX, blockY, blockZ, 0);
            }
        }
    }

    public override void updatePassengerPosition()
    {
        if (passenger != null)
        {
            passenger.setPosition(x, y + getPassengerRidingHeight() + passenger.getStandingEyeHeight(), z);
        }
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
    }

    public override void readNbt(NBTTagCompound nbt)
    {
    }

    public override float getShadowRadius()
    {
        return 0.0F;
    }

    public override bool interact(EntityPlayer player)
    {
        if (player.isSneaking())
        {
            return false;
        }

        if (passenger != null && passenger is EntityPlayer && passenger != player)
        {
            return true;
        }

        if (!world.IsRemote)
        {
            player.setVehicle(this);
        }

        return true;
    }

    private void UpdatePaddlesFromPassenger()
    {
        if (passenger is not EntityLiving rider)
        {
            UpdatePaddleInputs(0.0F, 0.0F);
            TickPaddles();
            return;
        }

        UpdatePaddleInputs(rider.GetSidewaysInput(), rider.GetForwardInput());
        TickPaddles();
    }

    private void UpdatePaddleInputs(float sidewaysInput, float forwardInput)
    {
        bool leftInput = sidewaysInput < -0.01F;
        bool rightInput = sidewaysInput > 0.01F;
        bool forwardPressed = forwardInput > 0.0F;
        _paddleMoving[0] = (rightInput && !leftInput) || forwardPressed;
        _paddleMoving[1] = (leftInput && !rightInput) || forwardPressed;
    }

    private void TickPaddles()
    {
        for (int i = 0; i < _paddlePositions.Length; ++i)
        {
            _paddlePositions[i] = _paddleMoving[i]
                ? _paddlePositions[i] + 0.3926991F
                : 0.0F;
        }
    }

    public float GetRowingTime(int paddle, float tickDelta)
    {
        if ((uint)paddle >= _paddlePositions.Length || !_paddleMoving[paddle])
        {
            return 0.0F;
        }

        return _paddlePositions[paddle] - 0.3926991F + 0.3926991F * tickDelta;
    }
}
