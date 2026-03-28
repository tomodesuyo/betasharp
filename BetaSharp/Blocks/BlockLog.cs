using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockLog : Block
{
    public BlockLog(int id) : base(id, Material.Wood) => textureId = 20;

    public override int getDroppedItemCount() => 1;

    public override int getDroppedItemId(int blockMeta) => Log.id;

    public override void onAfterBreak(OnAfterBreakEvent @event) => base.onAfterBreak(@event);

    public override void onBreak(OnBreakEvent @event)
    {
        sbyte searchRadius = 4;
        int regionExtent = searchRadius + 1;
        if (@event.World.ChunkHost.IsRegionLoaded(@event.X - regionExtent, @event.Y - regionExtent, @event.Z - regionExtent, @event.X + regionExtent, @event.Y + regionExtent, @event.Z + regionExtent))
        {
            for (int offsetX = -searchRadius; offsetX <= searchRadius; ++offsetX)
            {
                for (int offsetY = -searchRadius; offsetY <= searchRadius; ++offsetY)
                {
                    for (int offsetZ = -searchRadius; offsetZ <= searchRadius; ++offsetZ)
                    {
                        int neighborBlockId = @event.World.Reader.GetBlockId(@event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ);
                        if (neighborBlockId == Leaves.id)
                        {
                            int leavesMeta = @event.World.Reader.GetBlockMeta(@event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ);
                            if ((leavesMeta & 8) == 0)
                            {
                                @event.World.Writer.SetBlockMetaWithoutNotifyingNeighbors(@event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ, leavesMeta | 8);
                            }
                        }
                    }
                }
            }
        }
    }

    public override int getTexture(int side, int meta)
    {
        int type = meta & 3;
        if (side == 1 || side == 0)
        {
            return 21;
        }

        return type switch
        {
            1 => 116,
            2 => 117,
            3 => 153,
            _ => 20
        };
    }

    protected override int getDroppedItemMeta(int blockMeta) => blockMeta & 3;
}
