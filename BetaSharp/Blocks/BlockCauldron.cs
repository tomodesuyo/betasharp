using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockCauldron : Block
{
    public BlockCauldron(int id) : base(id, 154, Material.Metal)
    {
    }

    public override int getTexture(int side, int meta) => side == 1 ? 138 : side == 0 ? 155 : 154;

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 5.0F / 16.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        float thickness = 2.0F / 16.0F;
        setBoundingBox(0.0F, 0.0F, 0.0F, thickness, 1.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, thickness);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(1.0F - thickness, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(0.0F, 0.0F, 1.0F - thickness, 1.0F, 1.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setupRenderBoundingBox();
    }

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Cauldron;

    public override int getDroppedItemId(int blockMeta) => Items.Item.Cauldron.id;
}
