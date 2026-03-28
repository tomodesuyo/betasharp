using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

public class BlockFence : Block
{
    public BlockFence(int id, int texture) : base(id, texture, Material.Wood)
    {
    }

    public BlockFence(int id, int texture, Material material) : base(id, texture, material)
    {
    }

    public override bool canPlaceAt(CanPlaceAtContext context) => base.canPlaceAt(context);

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        bool north = canConnectFenceTo(world, x, y, z - 1);
        bool south = canConnectFenceTo(world, x, y, z + 1);
        bool west = canConnectFenceTo(world, x - 1, y, z);
        bool east = canConnectFenceTo(world, x + 1, y, z);
        float minX = west ? 0.0F : 6.0F / 16.0F;
        float maxX = east ? 1.0F : 10.0F / 16.0F;
        float minZ = north ? 0.0F : 6.0F / 16.0F;
        float maxZ = south ? 1.0F : 10.0F / 16.0F;
        return new Box(x + minX, y, z + minZ, x + maxX, y + 1.5F, z + maxZ);
    }

    public override void updateBoundingBox(IBlockReader world, EntityManager? entities, int x, int y, int z)
    {
        bool north = canConnectFenceTo(world, x, y, z - 1);
        bool south = canConnectFenceTo(world, x, y, z + 1);
        bool west = canConnectFenceTo(world, x - 1, y, z);
        bool east = canConnectFenceTo(world, x + 1, y, z);
        float minX = west ? 0.0F : 6.0F / 16.0F;
        float maxX = east ? 1.0F : 10.0F / 16.0F;
        float minZ = north ? 0.0F : 6.0F / 16.0F;
        float maxZ = south ? 1.0F : 10.0F / 16.0F;
        setBoundingBox(minX, 0.0F, minZ, maxX, 1.0F, maxZ);
    }

    public override bool isOpaque()
    {
        return false;
    }

    public override bool isFullCube()
    {
        return false;
    }

    public override BlockRendererType getRenderType()
    {
        return BlockRendererType.Fence;
    }

    public bool canConnectFenceTo(IBlockReader world, int x, int y, int z)
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

        Block? block = Blocks[neighborId];
        return block != null && block.material.IsSolid && block.isOpaque() && block.isFullCube() && block.material != Material.Pumpkin;
    }
}
