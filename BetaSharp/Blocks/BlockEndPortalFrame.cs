using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockEndPortalFrame : Block
{
    public BlockEndPortalFrame(int id) : base(id, 159, Material.Stone)
    {
    }

    public override int getTexture(int side, int meta) => side == 1 ? textureId - 1 : side == 0 ? textureId + 16 : textureId;

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.EndPortalFrame;

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 13.0F / 16.0F, 1.0F);

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 13.0F / 16.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        if (HasEye(world.GetBlockMeta(x, y, z)))
        {
            setBoundingBox(5.0F / 16.0F, 13.0F / 16.0F, 5.0F / 16.0F, 11.0F / 16.0F, 1.0F, 11.0F / 16.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }

        setupRenderBoundingBox();
    }

    public override int getDroppedItemId(int blockMeta) => 0;

    public override void onPlaced(OnPlacedEvent @event)
    {
        int meta = 0;
        if (@event.Placer != null)
        {
            meta = ((MathHelper.Floor(@event.Placer.yaw * 4.0F / 360.0F + 0.5D) & 3) + 2) % 4;
        }

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta);
    }

    public static bool HasEye(int meta) => (meta & 4) != 0;
}
