using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockStem : BlockPlant
{
    private readonly Block _fruitBlock;

    public BlockStem(int id, Block fruitBlock) : base(id, 111)
    {
        _fruitBlock = fruitBlock;
        setTickRandomly(true);
        setBoundingBox(0.375F, 0.0F, 0.375F, 0.625F, 0.25F, 0.625F);
    }

    protected override bool canPlantOnTop(int id)
    {
        return id == Farmland.id;
    }

    public override int getTexture(int side, int meta)
    {
        return textureId + Math.Min(meta, 7);
    }

    public override BlockRendererType getRenderType()
    {
        return BlockRendererType.Crops;
    }

    public override void onTick(OnTickEvent @event)
    {
        base.onTick(@event);
        if (@event.World.Lighting.GetBrightness(LightType.Block, @event.X, @event.Y + 1, @event.Z) < 9)
        {
            return;
        }

        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (meta < 7)
        {
            if (Random.Shared.Next(25) == 0)
            {
                @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta + 1);
            }

            return;
        }

        for (int side = 2; side <= 5; ++side)
        {
            int x = @event.X;
            int z = @event.Z;
            switch (side)
            {
                case 2: --z; break;
                case 3: ++z; break;
                case 4: --x; break;
                case 5: ++x; break;
            }

            if (@event.World.Reader.GetBlockId(x, @event.Y, z) == _fruitBlock.id)
            {
                return;
            }
        }

        int growX = @event.X + Random.Shared.Next(3) - 1;
        int growZ = @event.Z + Random.Shared.Next(3) - 1;
        int soilId = @event.World.Reader.GetBlockId(growX, @event.Y - 1, growZ);
        if (@event.World.Reader.IsAir(growX, @event.Y, growZ) && soilId == Farmland.id)
        {
            @event.World.Writer.SetBlock(growX, @event.Y, growZ, _fruitBlock.id);
        }
    }
}
