using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockBrewingStand : Block
{
    public BlockBrewingStand(int id) : base(id, 157, Material.Metal)
    {
        setBoundingBox(0.125F, 0.0F, 0.125F, 0.875F, 0.875F, 0.875F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.BrewingStand;

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        setBoundingBox(7.0F / 16.0F, 0.0F, 7.0F / 16.0F, 9.0F / 16.0F, 14.0F / 16.0F, 9.0F / 16.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 2.0F / 16.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        setupRenderBoundingBox();
    }

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 2.0F / 16.0F, 1.0F);

    public override bool onUse(OnUseEvent @event) => true;

    public override void randomDisplayTick(OnTickEvent @event)
    {
        double particleX = @event.X + 0.4F + Random.Shared.NextSingle() * 0.2F;
        double particleY = @event.Y + 0.7F + Random.Shared.NextSingle() * 0.3F;
        double particleZ = @event.Z + 0.4F + Random.Shared.NextSingle() * 0.2F;
        @event.World.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
    }

    public override int getDroppedItemId(int blockMeta) => Items.Item.BrewingStand.id;
}
