using BetaSharp.Items;
using BetaSharp.Rules;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockNetherStalk : BlockPlant
{
    private const int MaxAge = 3;

    public BlockNetherStalk(int id) : base(id, 226)
    {
        setTickRandomly(true);
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 0.25F, 1.0F);
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
            if (meta < MaxAge)
            {
                @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta + 1);
            }
        }
    }

    public override int getTexture(int side, int meta)
    {
        return meta switch
        {
            <= 0 => textureId,
            1 => textureId + 1,
            _ => textureId + 2
        };
    }

    public override BlockRendererType getRenderType() => BlockRendererType.Crops;

    public override void dropStacks(OnDropEvent ctx)
    {
        if (ctx.World.IsRemote || !ctx.World.Rules.GetBool(DefaultRules.DoTileDrops))
        {
            return;
        }

        int dropCount = ctx.Meta >= MaxAge ? 2 + ctx.World.Random.NextInt(3) : 1;
        for (int i = 0; i < dropCount; ++i)
        {
            dropStack(ctx.World, ctx.X, ctx.Y, ctx.Z, new ItemStack(Item.NetherWart));
        }
    }

    public override int getDroppedItemId(int blockMeta) => -1;

    public override int getDroppedItemCount() => 0;

    public void ApplyBonemeal(IWorldContext world, int x, int y, int z)
    {
        int meta = world.Reader.GetBlockMeta(x, y, z);
        if (meta < MaxAge)
        {
            world.Writer.SetBlockMeta(x, y, z, meta + 1);
        }
    }
}
