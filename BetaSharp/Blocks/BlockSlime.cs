using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockSlime : Block
{
    public BlockSlime(int id, int textureId) : base(id, textureId, Material.Clay)
    {
        slipperiness = 0.8F;
    }

    public override bool isOpaque() => false;

    public override int getRenderLayer() => 1;

    public override void onEntityCollision(OnEntityCollisionEvent @event)
    {
        if (Math.Abs(@event.Entity.velocityY) < 0.1D && !@event.Entity.isSneaking())
        {
            double factor = 0.4D + Math.Abs(@event.Entity.velocityY) * 0.2D;
            @event.Entity.velocityX *= factor;
            @event.Entity.velocityZ *= factor;
        }
    }
}
