using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Villages;

public sealed class VillageCollection(IWorldContext world)
{
    private readonly List<Vec3i> _villagerPositionsList = [];
    private readonly List<VillageDoorInfo> _newDoors = [];
    private readonly List<Village> _villageList = [];
    private int _tickCounter;

    public IReadOnlyList<Village> Villages => _villageList;

    public void AddVillagerPosition(int x, int y, int z)
    {
        if (_villagerPositionsList.Count > 64 || IsVillagerPositionPresent(x, y, z))
        {
            return;
        }

        _villagerPositionsList.Add(new Vec3i(x, y, z));
    }

    public void Tick()
    {
        ++_tickCounter;
        for (int i = 0; i < _villageList.Count; ++i)
        {
            _villageList[i].Tick(_tickCounter);
        }

        RemoveAnnihilatedVillages();
        DropOldestVillagerPosition();
        AddNewDoorsToVillageOrCreateVillage();
    }

    private void RemoveAnnihilatedVillages()
    {
        for (int i = 0; i < _villageList.Count; ++i)
        {
            if (_villageList[i].IsAnnihilated())
            {
                _villageList.RemoveAt(i--);
            }
        }
    }

    public Village? FindNearestVillage(int x, int y, int z, int radius)
    {
        Village? closestVillage = null;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < _villageList.Count; ++i)
        {
            Village village = _villageList[i];
            int dx = village.Center.X - x;
            int dy = village.Center.Y - y;
            int dz = village.Center.Z - z;
            float distance = dx * dx + dy * dy + dz * dz;
            if (distance < closestDistance)
            {
                int allowedDistance = radius + village.VillageRadius;
                if (distance <= allowedDistance * allowedDistance)
                {
                    closestVillage = village;
                    closestDistance = distance;
                }
            }
        }

        return closestVillage;
    }

    private void DropOldestVillagerPosition()
    {
        if (_villagerPositionsList.Count == 0)
        {
            return;
        }

        Vec3i pos = _villagerPositionsList[0];
        _villagerPositionsList.RemoveAt(0);
        AddUnassignedWoodenDoorsAroundToNewDoorsList(pos);
    }

    private void AddNewDoorsToVillageOrCreateVillage()
    {
        for (int i = 0; i < _newDoors.Count; ++i)
        {
            VillageDoorInfo door = _newDoors[i];
            bool addedToVillage = false;
            for (int j = 0; j < _villageList.Count; ++j)
            {
                Village village = _villageList[j];
                int dx = village.Center.X - door.PosX;
                int dy = village.Center.Y - door.PosY;
                int dz = village.Center.Z - door.PosZ;
                if (Math.Sqrt(dx * dx + dy * dy + dz * dz) <= 32 + village.VillageRadius)
                {
                    village.AddVillageDoorInfo(door);
                    addedToVillage = true;
                    break;
                }
            }

            if (!addedToVillage)
            {
                Village village = new(world);
                village.AddVillageDoorInfo(door);
                _villageList.Add(village);
            }
        }

        _newDoors.Clear();
    }

    private void AddUnassignedWoodenDoorsAroundToNewDoorsList(Vec3i villagerPos)
    {
        const int horizontalRadius = 16;
        const int verticalRadius = 4;
        for (int x = villagerPos.X - horizontalRadius; x < villagerPos.X + horizontalRadius; ++x)
        {
            for (int y = villagerPos.Y - verticalRadius; y < villagerPos.Y + verticalRadius; ++y)
            {
                for (int z = villagerPos.Z - horizontalRadius; z < villagerPos.Z + horizontalRadius; ++z)
                {
                    if (!IsWoodenDoorAt(x, y, z))
                    {
                        continue;
                    }

                    VillageDoorInfo? door = GetVillageDoorAt(x, y, z);
                    if (door == null)
                    {
                        AddDoorToNewListIfAppropriate(x, y, z);
                    }
                    else
                    {
                        door.LastActivityTimestamp = _tickCounter;
                    }
                }
            }
        }
    }

    private VillageDoorInfo? GetVillageDoorAt(int x, int y, int z)
    {
        for (int i = 0; i < _newDoors.Count; ++i)
        {
            VillageDoorInfo door = _newDoors[i];
            if (door.PosX == x && door.PosZ == z && Math.Abs(door.PosY - y) <= 1)
            {
                return door;
            }
        }

        for (int i = 0; i < _villageList.Count; ++i)
        {
            VillageDoorInfo? door = _villageList[i].GetVillageDoorAt(x, y, z);
            if (door != null)
            {
                return door;
            }
        }

        return null;
    }

    private void AddDoorToNewListIfAppropriate(int x, int y, int z)
    {
        int orientation = ((BlockDoor)Block.Door).GetDoorOrientation(world.Reader, x, y, z);
        int skyDifference;
        if (orientation != 0 && orientation != 2)
        {
            skyDifference = 0;
            for (int offset = -5; offset < 0; ++offset)
            {
                if (world.Reader.IsTopY(x, y, z + offset))
                {
                    --skyDifference;
                }
            }

            for (int offset = 1; offset <= 5; ++offset)
            {
                if (world.Reader.IsTopY(x, y, z + offset))
                {
                    ++skyDifference;
                }
            }

            if (skyDifference != 0)
            {
                _newDoors.Add(new VillageDoorInfo(x, y, z, 0, skyDifference > 0 ? -2 : 2, _tickCounter));
            }
        }
        else
        {
            skyDifference = 0;
            for (int offset = -5; offset < 0; ++offset)
            {
                if (world.Reader.IsTopY(x + offset, y, z))
                {
                    --skyDifference;
                }
            }

            for (int offset = 1; offset <= 5; ++offset)
            {
                if (world.Reader.IsTopY(x + offset, y, z))
                {
                    ++skyDifference;
                }
            }

            if (skyDifference != 0)
            {
                _newDoors.Add(new VillageDoorInfo(x, y, z, skyDifference > 0 ? -2 : 2, 0, _tickCounter));
            }
        }
    }

    private bool IsVillagerPositionPresent(int x, int y, int z)
    {
        for (int i = 0; i < _villagerPositionsList.Count; ++i)
        {
            Vec3i pos = _villagerPositionsList[i];
            if (pos.X == x && pos.Y == y && pos.Z == z)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsWoodenDoorAt(int x, int y, int z) => world.Reader.GetBlockId(x, y, z) == Block.Door.id;
}
