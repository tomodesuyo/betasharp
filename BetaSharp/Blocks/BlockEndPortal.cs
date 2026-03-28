using BetaSharp.Blocks.Materials;
using BetaSharp.Blocks.Entities;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockEndPortal : BlockWithEntity
{
    public BlockEndPortal(int id, Material material) : base(id, 171, material)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F / 16.0F, 1.0F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Entity;

    public override bool isSideVisible(IBlockReader iBlockReader, int x, int y, int z, int side)
    {
        return side == 1;
    }

    public override BlockEntity getBlockEntity() => new BlockEntityEndPortal();

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z) => null;

    public override int getDroppedItemCount() => 0;

    public override int getRenderLayer() => 1;

    public override void updateBoundingBox(IBlockReader blockReader, EntityManager? entities, int x, int y, int z) => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F / 16.0F, 1.0F);

    public override void onEntityCollision(OnEntityCollisionEvent @event)
    {
        if (@event.Entity.vehicle == null && @event.Entity.passenger == null)
        {
            if (@event.Entity is EntityPlayer player)
            {
                player.QueueDimensionChange(player.dimensionId == 1 ? 0 : 1);
            }
            else
            {
                @event.Entity.tickPortalCooldown();
            }
        }
    }

    public override void randomDisplayTick(OnTickEvent @event)
    {
        double particleX = @event.X + Random.Shared.NextSingle();
        double particleY = @event.Y + 0.8F;
        double particleZ = @event.Z + Random.Shared.NextSingle();
        @event.World.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
    }
}
