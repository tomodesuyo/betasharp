using BetaSharp.Blocks;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class WallRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        BlockWall wall = (BlockWall)block;
        bool north = wall.canConnectWallTo(ctx.BlockReader, pos.x, pos.y, pos.z - 1);
        bool south = wall.canConnectWallTo(ctx.BlockReader, pos.x, pos.y, pos.z + 1);
        bool west = wall.canConnectWallTo(ctx.BlockReader, pos.x - 1, pos.y, pos.z);
        bool east = wall.canConnectWallTo(ctx.BlockReader, pos.x + 1, pos.y, pos.z);

        float centerMinX = 0.25F;
        float centerMaxX = 0.75F;
        float centerMinZ = 0.25F;
        float centerMaxZ = 0.75F;
        float centerMaxY = 1.0F;

        if (north && south && !west && !east)
        {
            centerMinX = 0.3125F;
            centerMaxX = 0.6875F;
            centerMaxY = 0.8125F;
        }
        else if (!north && !south && west && east)
        {
            centerMinZ = 0.3125F;
            centerMaxZ = 0.6875F;
            centerMaxY = 0.8125F;
        }

        (ctx with { OverrideBounds = new Box(centerMinX, 0.0F, centerMinZ, centerMaxX, centerMaxY, centerMaxZ) }).DrawBlock(block, pos);

        if (north)
        {
            (ctx with { OverrideBounds = new Box(0.3125F, 0.0F, 0.0F, 0.6875F, 0.8125F, 0.5F) }).DrawBlock(block, pos);
        }

        if (south)
        {
            (ctx with { OverrideBounds = new Box(0.3125F, 0.0F, 0.5F, 0.6875F, 0.8125F, 1.0F) }).DrawBlock(block, pos);
        }

        if (west)
        {
            (ctx with { OverrideBounds = new Box(0.0F, 0.0F, 0.3125F, 0.5F, 0.8125F, 0.6875F) }).DrawBlock(block, pos);
        }

        if (east)
        {
            (ctx with { OverrideBounds = new Box(0.5F, 0.0F, 0.3125F, 1.0F, 0.8125F, 0.6875F) }).DrawBlock(block, pos);
        }

        return true;
    }
}
