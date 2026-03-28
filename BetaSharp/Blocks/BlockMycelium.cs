using BetaSharp.Blocks.Materials;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockMycelium : Block
{
    public BlockMycelium(int id) : base(id, Material.SolidOrganic)
    {
        textureId = 78;
        setTickRandomly(true);
    }

    public override int getTexture(int side)
    {
        return side switch
        {
            1 => textureId,
            0 => Dirt.textureId,
            _ => 77
        };
    }

    public override int getTextureId(IBlockReader iBlockReader, int x, int y, int z, int side)
    {
        if (side == 1)
        {
            return textureId;
        }

        if (side == 0)
        {
            return Dirt.textureId;
        }

        return iBlockReader.GetMaterial(x, y + 1, z) == Material.SnowLayer || iBlockReader.GetMaterial(x, y + 1, z) == Material.SnowBlock ? 68 : 77;
    }

    public override void onTick(OnTickEvent ctx)
    {
        if (ctx.World.Lighting.GetLightLevel(ctx.X, ctx.Y + 1, ctx.Z) < 4 && BlockLightOpacity[ctx.World.Reader.GetBlockId(ctx.X, ctx.Y + 1, ctx.Z)] > 2)
        {
            if (Random.Shared.Next(4) == 0)
            {
                ctx.World.Writer.SetBlock(ctx.X, ctx.Y, ctx.Z, Dirt.id);
            }

            return;
        }

        if (ctx.World.Lighting.GetLightLevel(ctx.X, ctx.Y + 1, ctx.Z) < 9)
        {
            return;
        }

        int spreadX = ctx.X + Random.Shared.Next(3) - 1;
        int spreadY = ctx.Y + Random.Shared.Next(5) - 3;
        int spreadZ = ctx.Z + Random.Shared.Next(3) - 1;
        int blockAboveId = ctx.World.Reader.GetBlockId(spreadX, spreadY + 1, spreadZ);
        if (ctx.World.Reader.GetBlockId(spreadX, spreadY, spreadZ) == Dirt.id && ctx.World.Lighting.GetLightLevel(spreadX, spreadY + 1, spreadZ) >= 4 && BlockLightOpacity[blockAboveId] <= 2)
        {
            ctx.World.Writer.SetBlock(spreadX, spreadY, spreadZ, id);
        }
    }
}
