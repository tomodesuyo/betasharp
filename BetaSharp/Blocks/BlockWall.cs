using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

public sealed class BlockWall : Block
{
    public BlockWall(int id) : base(id, 16, Material.Stone)
    {
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Wall;

    public override int getTexture(int side, int meta)
    {
        return (meta & 1) == 1 ? Block.MossyCobblestone.getTexture(side) : Block.Cobblestone.getTexture(side);
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta & 1;
    }

    public bool canConnectWallTo(IBlockReader world, int x, int y, int z)
    {
        int neighborId = world.GetBlockId(x, y, z);
        if (neighborId == id || neighborId == Block.FenceGate.id)
        {
            return true;
        }

        if (neighborId <= 0)
        {
            return false;
        }

        Block neighbor = Blocks[neighborId];
        return neighbor.material.IsSolid && neighbor.isOpaque() && neighbor.isFullCube() && neighbor.material != Material.Pumpkin;
    }

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        updateBoundingBox(world, entities, x, y, z);
        return new Box(
            x + BoundingBox.MinX,
            y + BoundingBox.MinY,
            z + BoundingBox.MinZ,
            x + BoundingBox.MaxX,
            y + 1.5F,
            z + BoundingBox.MaxZ);
    }

    public override void updateBoundingBox(IBlockReader world, EntityManager? entities, int x, int y, int z)
    {
        bool north = canConnectWallTo(world, x, y, z - 1);
        bool south = canConnectWallTo(world, x, y, z + 1);
        bool west = canConnectWallTo(world, x - 1, y, z);
        bool east = canConnectWallTo(world, x + 1, y, z);
        float minX = 0.25F;
        float maxX = 0.75F;
        float minZ = 0.25F;
        float maxZ = 0.75F;
        float maxY = 1.0F;

        if (north)
        {
            minZ = 0.0F;
        }

        if (south)
        {
            maxZ = 1.0F;
        }

        if (west)
        {
            minX = 0.0F;
        }

        if (east)
        {
            maxX = 1.0F;
        }

        if (north && south && !west && !east)
        {
            maxY = 0.8125F;
            minX = 0.3125F;
            maxX = 0.6875F;
        }
        else if (!north && !south && west && east)
        {
            maxY = 0.8125F;
            minZ = 0.3125F;
            maxZ = 0.6875F;
        }

        setBoundingBox(minX, 0.0F, minZ, maxX, maxY, maxZ);
    }
}
