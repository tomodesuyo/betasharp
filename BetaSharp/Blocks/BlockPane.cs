using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockPane : BlockBreakable
{
    private readonly int _edgeTextureId;
    private readonly bool _canDropItself;

    public BlockPane(int id, int textureId, int edgeTextureId, Material material, bool canDropItself) : base(id, textureId, material, false)
    {
        _edgeTextureId = edgeTextureId;
        _canDropItself = canDropItself;
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);
    }

    public override bool isFullCube() => false;

    public override bool isOpaque() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Pane;

    public override int getDroppedItemId(int blockMeta) => _canDropItself ? base.getDroppedItemId(blockMeta) : 0;

    public bool CanConnectTo(IBlockReader world, int x, int y, int z)
    {
        int blockId = world.GetBlockId(x, y, z);
        return blockId > 0 && (world.ShouldSuffocate(x, y, z) || blockId == id || blockId == Block.Glass.id);
    }

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        bool north = CanConnectTo(world, x, y, z - 1);
        bool south = CanConnectTo(world, x, y, z + 1);
        bool west = CanConnectTo(world, x - 1, y, z);
        bool east = CanConnectTo(world, x + 1, y, z);

        if ((!west || !east) && (west || east || north || south))
        {
            if (west && !east)
            {
                setBoundingBox(0.0F, 0.0F, 7.0F / 16.0F, 0.5F, 1.0F, 9.0F / 16.0F);
                base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
            }
            else if (!west && east)
            {
                setBoundingBox(0.5F, 0.0F, 7.0F / 16.0F, 1.0F, 1.0F, 9.0F / 16.0F);
                base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
            }
        }
        else
        {
            setBoundingBox(0.0F, 0.0F, 7.0F / 16.0F, 1.0F, 1.0F, 9.0F / 16.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }

        if ((!north || !south) && (west || east || north || south))
        {
            if (north && !south)
            {
                setBoundingBox(7.0F / 16.0F, 0.0F, 0.0F, 9.0F / 16.0F, 1.0F, 0.5F);
                base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
            }
            else if (!north && south)
            {
                setBoundingBox(7.0F / 16.0F, 0.0F, 0.5F, 9.0F / 16.0F, 1.0F, 1.0F);
                base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
            }
        }
        else
        {
            setBoundingBox(7.0F / 16.0F, 0.0F, 0.0F, 9.0F / 16.0F, 1.0F, 1.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }

        setupRenderBoundingBox();
    }

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        updateBoundingBox(world, entities, x, y, z);
        return base.getCollisionShape(world, entities, x, y, z);
    }

    public override int getTextureId(IBlockReader world, int x, int y, int z, int side)
    {
        return side == 0 || side == 1 ? textureId : _edgeTextureId;
    }

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);

    public override void updateBoundingBox(IBlockReader world, EntityManager? entities, int x, int y, int z)
    {
        float minX = 7.0F / 16.0F;
        float maxX = 9.0F / 16.0F;
        float minZ = 7.0F / 16.0F;
        float maxZ = 9.0F / 16.0F;
        bool north = CanConnectTo(world, x, y, z - 1);
        bool south = CanConnectTo(world, x, y, z + 1);
        bool west = CanConnectTo(world, x - 1, y, z);
        bool east = CanConnectTo(world, x + 1, y, z);

        if ((!west || !east) && (west || east || north || south))
        {
            if (west && !east)
            {
                minX = 0.0F;
            }
            else if (!west && east)
            {
                maxX = 1.0F;
            }
        }
        else
        {
            minX = 0.0F;
            maxX = 1.0F;
        }

        if ((!north || !south) && (west || east || north || south))
        {
            if (north && !south)
            {
                minZ = 0.0F;
            }
            else if (!north && south)
            {
                maxZ = 1.0F;
            }
        }
        else
        {
            minZ = 0.0F;
            maxZ = 1.0F;
        }

        setBoundingBox(minX, 0.0F, minZ, maxX, 1.0F, maxZ);
    }
}
