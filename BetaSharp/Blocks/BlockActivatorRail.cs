using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockActivatorRail : BlockRail
{
    public BlockActivatorRail(int id, int textureId) : base(id, textureId, true)
    {
    }

    public override int getTexture(int side, int meta)
    {
        return (meta & 8) == 0 ? textureId - 16 : textureId;
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        base.onPlaced(@event);
        if (!@event.World.IsRemote)
        {
            UpdatePoweredState(@event.World, @event.X, @event.Y, @event.Z, @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z));
        }
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        base.neighborUpdate(@event);
        if (!@event.World.IsRemote && @event.World.Reader.GetBlockId(@event.X, @event.Y, @event.Z) == id)
        {
            UpdatePoweredState(@event.World, @event.X, @event.Y, @event.Z, @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z));
        }
    }

    private void UpdatePoweredState(IWorldContext world, int x, int y, int z, int meta)
    {
        bool shouldBePowered = world.Redstone.IsPowered(x, y, z) || world.Redstone.IsPowered(x, y + 1, z);
        bool isPowered = (meta & 8) != 0;
        if (shouldBePowered == isPowered)
        {
            return;
        }

        int shape = meta & 7;
        world.Writer.SetBlockMeta(x, y, z, shouldBePowered ? shape | 8 : shape);
        world.Broadcaster.SetBlocksDirty(x, y, z);
    }
}
