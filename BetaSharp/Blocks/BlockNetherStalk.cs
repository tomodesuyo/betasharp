using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockNetherStalk : BlockPlant
{
    public BlockNetherStalk(int id) : base(id, 226)
    {
        setTickRandomly(true);
        setBoundingBox(0.3125F, 0.0F, 0.3125F, 0.6875F, 0.875F, 0.6875F);
    }

    protected override bool canPlantOnTop(int id)
    {
        return id == Block.Soulsand.id;
    }

    public override bool canGrow(OnTickEvent ctx)
    {
        return canPlantOnTop(ctx.World.Reader.GetBlockId(ctx.X, ctx.Y - 1, ctx.Z));
    }

    public override void onTick(OnTickEvent @event)
    {
        if (Random.Shared.Next(10) == 0)
        {
            int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
            if (meta < 3)
            {
                @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta + 1);
            }
        }
    }

    public override int getTexture(int side, int meta)
    {
        return textureId + Math.Min(meta, 3);
    }
}
