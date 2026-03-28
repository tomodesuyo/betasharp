using BetaSharp.Blocks.Materials;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockRedstoneLight : Block
{
    private readonly bool _powered;

    public BlockRedstoneLight(int id, bool powered) : base(id, 211, Material.Stone)
    {
        _powered = powered;
        if (powered)
        {
            setLuminance(1.0F);
            textureId++;
        }
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        UpdateState(@event.World, @event.X, @event.Y, @event.Z, scheduleTurnOff: true);
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        UpdateState(@event.World, @event.X, @event.Y, @event.Z, scheduleTurnOff: true);
    }

    public override void onTick(OnTickEvent @event)
    {
        if (_powered && !@event.World.Redstone.IsPowered(@event.X, @event.Y, @event.Z))
        {
            @event.World.Writer.SetBlock(@event.X, @event.Y, @event.Z, Block.RedstoneLamp.id);
        }
    }

    public override int getTickRate() => 4;

    public override int getDroppedItemId(int blockMeta)
    {
        return Block.RedstoneLamp.id;
    }

    private void UpdateState(IWorldContext world, int x, int y, int z, bool scheduleTurnOff)
    {
        if (world.IsRemote)
        {
            return;
        }

        bool isPowered = world.Redstone.IsPowered(x, y, z);
        if (_powered)
        {
            if (!isPowered && scheduleTurnOff)
            {
                world.TickScheduler.ScheduleBlockUpdate(x, y, z, id, getTickRate());
            }

            return;
        }

        if (isPowered)
        {
            world.Writer.SetBlock(x, y, z, Block.LitRedstoneLamp.id);
        }
    }
}
