using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockEndPortal : Block
{
    public BlockEndPortal(int id, Material material) : base(id, material)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F / 16.0F, 1.0F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z) => null;

    public override int getDroppedItemCount() => 0;

    public override int getRenderLayer() => 1;

    public override void updateBoundingBox(IBlockReader blockReader, EntityManager? entities, int x, int y, int z) => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F / 16.0F, 1.0F);

    public override void randomDisplayTick(OnTickEvent @event)
    {
        double particleX = @event.X + Random.Shared.NextSingle();
        double particleY = @event.Y + 0.8F;
        double particleZ = @event.Z + Random.Shared.NextSingle();
        @event.World.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
    }
}
