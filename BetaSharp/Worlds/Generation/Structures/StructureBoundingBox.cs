namespace BetaSharp.Worlds.Generation.Structures;

public class StructureBoundingBox
{
    public int MinX;
    public int MinY;
    public int MinZ;
    public int MaxX;
    public int MaxY;
    public int MaxZ;

    public StructureBoundingBox()
    {
    }

    public StructureBoundingBox(StructureBoundingBox other)
    {
        MinX = other.MinX;
        MinY = other.MinY;
        MinZ = other.MinZ;
        MaxX = other.MaxX;
        MaxY = other.MaxY;
        MaxZ = other.MaxZ;
    }

    public StructureBoundingBox(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
    {
        MinX = minX;
        MinY = minY;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
    }

    public StructureBoundingBox(int minX, int minZ, int maxX, int maxZ)
    {
        MinX = minX;
        MinY = 0;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = 65536;
        MaxZ = maxZ;
    }

    public static StructureBoundingBox CreateUnknownBox()
    {
        return new StructureBoundingBox(int.MaxValue, int.MaxValue, int.MaxValue, int.MinValue, int.MinValue, int.MinValue);
    }

    public static StructureBoundingBox Create(int x, int y, int z, int offsetX, int offsetY, int offsetZ, int sizeX, int sizeY, int sizeZ, int facing)
    {
        return facing switch
        {
            0 => new StructureBoundingBox(x + offsetX, y + offsetY, z + offsetZ, x + sizeX - 1 + offsetX, y + sizeY - 1 + offsetY, z + sizeZ - 1 + offsetZ),
            1 => new StructureBoundingBox(x - sizeZ + 1 + offsetZ, y + offsetY, z + offsetX, x + offsetZ, y + sizeY - 1 + offsetY, z + sizeX - 1 + offsetX),
            2 => new StructureBoundingBox(x + offsetX, y + offsetY, z - sizeZ + 1 + offsetZ, x + sizeX - 1 + offsetX, y + sizeY - 1 + offsetY, z + offsetZ),
            3 => new StructureBoundingBox(x + offsetZ, y + offsetY, z + offsetX, x + sizeZ - 1 + offsetZ, y + sizeY - 1 + offsetY, z + sizeX - 1 + offsetX),
            _ => new StructureBoundingBox(x + offsetX, y + offsetY, z + offsetZ, x + sizeX - 1 + offsetX, y + sizeY - 1 + offsetY, z + sizeZ - 1 + offsetZ),
        };
    }

    public bool Intersects(StructureBoundingBox other)
    {
        return MaxX >= other.MinX && MinX <= other.MaxX && MaxZ >= other.MinZ && MinZ <= other.MaxZ && MaxY >= other.MinY && MinY <= other.MaxY;
    }

    public bool Intersects(int minX, int minZ, int maxX, int maxZ)
    {
        return MaxX >= minX && MinX <= maxX && MaxZ >= minZ && MinZ <= maxZ;
    }

    public void ExpandTo(StructureBoundingBox other)
    {
        MinX = Math.Min(MinX, other.MinX);
        MinY = Math.Min(MinY, other.MinY);
        MinZ = Math.Min(MinZ, other.MinZ);
        MaxX = Math.Max(MaxX, other.MaxX);
        MaxY = Math.Max(MaxY, other.MaxY);
        MaxZ = Math.Max(MaxZ, other.MaxZ);
    }

    public void Offset(int x, int y, int z)
    {
        MinX += x;
        MinY += y;
        MinZ += z;
        MaxX += x;
        MaxY += y;
        MaxZ += z;
    }

    public bool Contains(int x, int y, int z)
    {
        return x >= MinX && x <= MaxX && z >= MinZ && z <= MaxZ && y >= MinY && y <= MaxY;
    }

    public int GetXSize() => MaxX - MinX + 1;
    public int GetYSize() => MaxY - MinY + 1;
    public int GetZSize() => MaxZ - MinZ + 1;
}
