using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;

namespace BetaSharp.Blocks;

internal class BlockSilverfish : Block
{
    public BlockSilverfish(int id) : base(id, 1, Material.Stone)
    {
    }

    public override int getDroppedItemId(int blockMeta)
    {
        return 0;
    }

    public override int getTexture(int side, int meta)
    {
        return meta switch
        {
            1 => Block.Cobblestone.getTexture(side),
            2 => Block.StoneBrick.getTexture(side, 0),
            _ => Block.Stone.getTexture(side, 0),
        };
    }

    public override int getDroppedItemCount()
    {
        return 0;
    }

    public override void onAfterBreak(OnAfterBreakEvent @event)
    {
        if (!@event.World.IsRemote)
        {
            EntitySilverfish silverfish = new(@event.World);
            silverfish.setPositionAndAngles(@event.X + 0.5D, @event.Y, @event.Z + 0.5D, 0.0F, 0.0F);
            @event.World.SpawnEntity(silverfish);
        }

        base.onAfterBreak(@event);
    }
}
