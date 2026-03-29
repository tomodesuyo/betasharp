namespace BetaSharp.Worlds.Villages;

public sealed class VillageDoorInfo(int posX, int posY, int posZ, int insideDirectionX, int insideDirectionZ, int lastActivityTimestamp)
{
    public int PosX { get; } = posX;
    public int PosY { get; } = posY;
    public int PosZ { get; } = posZ;
    public int InsideDirectionX { get; } = insideDirectionX;
    public int InsideDirectionZ { get; } = insideDirectionZ;
    public int LastActivityTimestamp { get; set; } = lastActivityTimestamp;
    public bool IsDetachedFromVillage { get; set; }
    private int DoorOpeningRestrictionCounter { get; set; }

    public int GetDistanceSquared(int x, int y, int z)
    {
        int dx = x - PosX;
        int dy = y - PosY;
        int dz = z - PosZ;
        return dx * dx + dy * dy + dz * dz;
    }

    public int GetInsideDistanceSquare(int x, int y, int z)
    {
        int dx = x - PosX - InsideDirectionX;
        int dy = y - PosY;
        int dz = z - PosZ - InsideDirectionZ;
        return dx * dx + dy * dy + dz * dz;
    }

    public int GetInsidePosX() => PosX + InsideDirectionX;

    public int GetInsidePosY() => PosY;

    public int GetInsidePosZ() => PosZ + InsideDirectionZ;

    public bool IsInside(int x, int z)
    {
        int dx = x - PosX;
        int dz = z - PosZ;
        return dx * InsideDirectionX + dz * InsideDirectionZ >= 0;
    }

    public void ResetDoorOpeningRestrictionCounter()
    {
        DoorOpeningRestrictionCounter = 0;
    }

    public void IncrementDoorOpeningRestrictionCounter()
    {
        ++DoorOpeningRestrictionCounter;
    }

    public int GetDoorOpeningRestrictionCounter() => DoorOpeningRestrictionCounter;
}
