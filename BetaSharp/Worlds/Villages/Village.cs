using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Villages;

public sealed class Village
{
    private readonly IWorldContext _world;
    private readonly List<VillageDoorInfo> _villageDoorInfoList = [];
    private readonly List<VillageAggressor> _villageAggressors = [];

    private Vec3i _centerHelper = Vec3i.Zero;

    public Village(IWorldContext world)
    {
        _world = world;
    }

    public Vec3i Center { get; private set; } = Vec3i.Zero;
    public int VillageRadius { get; private set; }
    public int LastAddDoorTimestamp { get; private set; }
    public int TickCounter { get; private set; }
    public int NumVillagers { get; private set; }
    public int NumIronGolems { get; private set; }

    public IReadOnlyList<VillageDoorInfo> VillageDoorInfoList => _villageDoorInfoList;

    public void Tick(int tickCounter)
    {
        TickCounter = tickCounter;
        RemoveDeadAndOutOfRangeDoors();
        RemoveDeadAndOldAggressors();
        if (tickCounter % 20 == 0)
        {
            UpdateNumVillagers();
        }

        if (tickCounter % 30 == 0)
        {
            UpdateNumIronGolems();
        }

        int desiredIronGolems = NumVillagers / 16;
        if (NumIronGolems < desiredIronGolems && _villageDoorInfoList.Count > 20 && _world.Random.NextInt(7000) == 0)
        {
            Vec3D? spawnPos = TryGetIronGolemSpawningLocation(Center.X, Center.Y, Center.Z, 2, 4, 2);
            if (spawnPos != null)
            {
                EntityIronGolem golem = new(_world);
                golem.setPositionAndAnglesKeepPrevAngles(spawnPos.Value.x, spawnPos.Value.y, spawnPos.Value.z, 0.0F, 0.0F);
                if (_world.SpawnEntity(golem))
                {
                    ++NumIronGolems;
                }
            }
        }
    }

    private Vec3D? TryGetIronGolemSpawningLocation(int x, int y, int z, int width, int height, int depth)
    {
        for (int i = 0; i < 10; ++i)
        {
            int candidateX = x + _world.Random.NextInt(16) - 8;
            int candidateY = y + _world.Random.NextInt(6) - 3;
            int candidateZ = z + _world.Random.NextInt(16) - 8;
            if (IsInRange(candidateX, candidateY, candidateZ) && IsValidIronGolemSpawningLocation(candidateX, candidateY, candidateZ, width, height, depth))
            {
                return new Vec3D(candidateX + 0.5D, candidateY, candidateZ + 0.5D);
            }
        }

        return null;
    }

    private bool IsValidIronGolemSpawningLocation(int x, int y, int z, int width, int height, int depth)
    {
        if (!_world.Reader.ShouldSuffocate(x, y - 1, z))
        {
            return false;
        }

        int minX = x - width / 2;
        int minZ = z - depth / 2;
        for (int checkX = minX; checkX < minX + width; ++checkX)
        {
            for (int checkY = y; checkY < y + height; ++checkY)
            {
                for (int checkZ = minZ; checkZ < minZ + depth; ++checkZ)
                {
                    if (_world.Reader.ShouldSuffocate(checkX, checkY, checkZ) || _world.Reader.GetMaterial(checkX, checkY, checkZ).IsFluid)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void UpdateNumIronGolems()
    {
        Box area = new(
            Center.X - VillageRadius,
            Center.Y - 4,
            Center.Z - VillageRadius,
            Center.X + VillageRadius,
            Center.Y + 4,
            Center.Z + VillageRadius);
        NumIronGolems = _world.Entities.CollectEntitiesOfType<EntityIronGolem>(area).Count;
    }

    private void UpdateNumVillagers()
    {
        Box area = new(
            Center.X - VillageRadius,
            Center.Y - 4,
            Center.Z - VillageRadius,
            Center.X + VillageRadius,
            Center.Y + 4,
            Center.Z + VillageRadius);
        NumVillagers = _world.Entities.CollectEntitiesOfType<EntityVillager>(area).Count;
    }

    public int GetNumVillageDoors() => _villageDoorInfoList.Count;

    public int GetTicksSinceLastDoorAdding() => TickCounter - LastAddDoorTimestamp;

    public bool IsInRange(int x, int y, int z)
    {
        int dx = Center.X - x;
        int dy = Center.Y - y;
        int dz = Center.Z - z;
        return dx * dx + dy * dy + dz * dz < VillageRadius * VillageRadius;
    }

    public VillageDoorInfo? FindNearestDoor(int x, int y, int z)
    {
        VillageDoorInfo? closestDoor = null;
        int closestDistance = int.MaxValue;
        for (int i = 0; i < _villageDoorInfoList.Count; ++i)
        {
            VillageDoorInfo door = _villageDoorInfoList[i];
            int distance = door.GetDistanceSquared(x, y, z);
            if (distance < closestDistance)
            {
                closestDoor = door;
                closestDistance = distance;
            }
        }

        return closestDoor;
    }

    public VillageDoorInfo? FindNearestDoorUnrestricted(int x, int y, int z)
    {
        VillageDoorInfo? closestDoor = null;
        int closestValue = int.MaxValue;
        for (int i = 0; i < _villageDoorInfoList.Count; ++i)
        {
            VillageDoorInfo door = _villageDoorInfoList[i];
            int distance = door.GetDistanceSquared(x, y, z);
            int value = distance > 256 ? distance * 1000 : door.GetDoorOpeningRestrictionCounter();
            if (value < closestValue)
            {
                closestDoor = door;
                closestValue = value;
            }
        }

        return closestDoor;
    }

    public VillageDoorInfo? GetVillageDoorAt(int x, int y, int z)
    {
        if (!IsInRange(x, y, z))
        {
            return null;
        }

        for (int i = 0; i < _villageDoorInfoList.Count; ++i)
        {
            VillageDoorInfo door = _villageDoorInfoList[i];
            if (door.PosX == x && door.PosZ == z && Math.Abs(door.PosY - y) <= 1)
            {
                return door;
            }
        }

        return null;
    }

    public void AddVillageDoorInfo(VillageDoorInfo doorInfo)
    {
        _villageDoorInfoList.Add(doorInfo);
        _centerHelper = new Vec3i(_centerHelper.X + doorInfo.PosX, _centerHelper.Y + doorInfo.PosY, _centerHelper.Z + doorInfo.PosZ);
        UpdateVillageRadiusAndCenter();
        LastAddDoorTimestamp = doorInfo.LastActivityTimestamp;
    }

    public bool IsAnnihilated() => _villageDoorInfoList.Count == 0;

    public void AddOrRenewAggressor(EntityLiving aggressor)
    {
        for (int i = 0; i < _villageAggressors.Count; ++i)
        {
            if (_villageAggressors[i].Aggressor == aggressor)
            {
                _villageAggressors[i].AggressionTime = TickCounter;
                return;
            }
        }

        _villageAggressors.Add(new VillageAggressor(aggressor, TickCounter));
    }

    public EntityLiving? FindNearestVillageAggressor(EntityLiving target)
    {
        EntityLiving? closest = null;
        double closestDistance = double.MaxValue;
        for (int i = 0; i < _villageAggressors.Count; ++i)
        {
            EntityLiving aggressor = _villageAggressors[i].Aggressor;
            double distance = aggressor.getSquaredDistance(target);
            if (distance <= closestDistance)
            {
                closest = aggressor;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private void RemoveDeadAndOldAggressors()
    {
        for (int i = 0; i < _villageAggressors.Count; ++i)
        {
            VillageAggressor aggressor = _villageAggressors[i];
            if (!aggressor.Aggressor.isAlive() || Math.Abs(TickCounter - aggressor.AggressionTime) > 300)
            {
                _villageAggressors.RemoveAt(i--);
            }
        }
    }

    private void RemoveDeadAndOutOfRangeDoors()
    {
        bool changed = false;
        bool resetRestrictionCounters = _world.Random.NextInt(50) == 0;
        for (int i = 0; i < _villageDoorInfoList.Count; ++i)
        {
            VillageDoorInfo door = _villageDoorInfoList[i];
            if (resetRestrictionCounters)
            {
                door.ResetDoorOpeningRestrictionCounter();
            }

            if (IsWoodenDoorAt(door.PosX, door.PosY, door.PosZ) && Math.Abs(TickCounter - door.LastActivityTimestamp) <= 1200)
            {
                continue;
            }

            _centerHelper = new Vec3i(_centerHelper.X - door.PosX, _centerHelper.Y - door.PosY, _centerHelper.Z - door.PosZ);
            door.IsDetachedFromVillage = true;
            _villageDoorInfoList.RemoveAt(i--);
            changed = true;
        }

        if (changed)
        {
            UpdateVillageRadiusAndCenter();
        }
    }

    private bool IsWoodenDoorAt(int x, int y, int z) => _world.Reader.GetBlockId(x, y, z) == Block.Door.id;

    private void UpdateVillageRadiusAndCenter()
    {
        int doorCount = _villageDoorInfoList.Count;
        if (doorCount == 0)
        {
            Center = Vec3i.Zero;
            VillageRadius = 0;
            return;
        }

        Center = new Vec3i(_centerHelper.X / doorCount, _centerHelper.Y / doorCount, _centerHelper.Z / doorCount);
        int radiusSq = 0;
        for (int i = 0; i < _villageDoorInfoList.Count; ++i)
        {
            VillageDoorInfo door = _villageDoorInfoList[i];
            int dx = door.PosX - Center.X;
            int dy = door.PosY - Center.Y;
            int dz = door.PosZ - Center.Z;
            int distance = dx * dx + dy * dy + dz * dz;
            if (distance > radiusSq)
            {
                radiusSq = distance;
            }
        }

        VillageRadius = Math.Max(32, (int)Math.Sqrt(radiusSq) + 1);
    }
}
