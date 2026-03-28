using BetaSharp.Blocks;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class FenceGateRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int meta = ctx.BlockReader.GetBlockMeta(pos.x, pos.y, pos.z);
        int dir = meta & 3;
        bool open = (meta & 4) != 0;

        if (dir != 3 && dir != 1)
        {
            DrawPart(block, pos, ref ctx, 0.0F, 5.0F / 16.0F, 7.0F / 16.0F, 2.0F / 16.0F, 1.0F, 9.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 5.0F / 16.0F, 7.0F / 16.0F, 1.0F, 1.0F, 9.0F / 16.0F);
        }
        else
        {
            DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 5.0F / 16.0F, 0.0F, 9.0F / 16.0F, 1.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 5.0F / 16.0F, 14.0F / 16.0F, 9.0F / 16.0F, 1.0F, 1.0F);
        }

        if (!open)
        {
            if (dir != 3 && dir != 1)
            {
                DrawPart(block, pos, ref ctx, 6.0F / 16.0F, 6.0F / 16.0F, 7.0F / 16.0F, 0.5F, 15.0F / 16.0F, 9.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 0.5F, 6.0F / 16.0F, 7.0F / 16.0F, 10.0F / 16.0F, 15.0F / 16.0F, 9.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 10.0F / 16.0F, 6.0F / 16.0F, 7.0F / 16.0F, 14.0F / 16.0F, 9.0F / 16.0F, 9.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 10.0F / 16.0F, 12.0F / 16.0F, 7.0F / 16.0F, 14.0F / 16.0F, 15.0F / 16.0F, 9.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 2.0F / 16.0F, 6.0F / 16.0F, 7.0F / 16.0F, 6.0F / 16.0F, 9.0F / 16.0F, 9.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 2.0F / 16.0F, 12.0F / 16.0F, 7.0F / 16.0F, 6.0F / 16.0F, 15.0F / 16.0F, 9.0F / 16.0F);
            }
            else
            {
                DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 6.0F / 16.0F, 6.0F / 16.0F, 9.0F / 16.0F, 15.0F / 16.0F, 0.5F);
                DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 6.0F / 16.0F, 0.5F, 9.0F / 16.0F, 15.0F / 16.0F, 10.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 6.0F / 16.0F, 10.0F / 16.0F, 9.0F / 16.0F, 9.0F / 16.0F, 14.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 12.0F / 16.0F, 10.0F / 16.0F, 9.0F / 16.0F, 15.0F / 16.0F, 14.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 6.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 9.0F / 16.0F, 6.0F / 16.0F);
                DrawPart(block, pos, ref ctx, 7.0F / 16.0F, 12.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 15.0F / 16.0F, 6.0F / 16.0F);
            }
        }
        else if (dir == 3)
        {
            DrawPart(block, pos, ref ctx, 13.0F / 16.0F, 6.0F / 16.0F, 0.0F, 15.0F / 16.0F, 15.0F / 16.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 13.0F / 16.0F, 6.0F / 16.0F, 14.0F / 16.0F, 15.0F / 16.0F, 15.0F / 16.0F, 1.0F);
            DrawPart(block, pos, ref ctx, 9.0F / 16.0F, 6.0F / 16.0F, 0.0F, 13.0F / 16.0F, 9.0F / 16.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 9.0F / 16.0F, 6.0F / 16.0F, 14.0F / 16.0F, 13.0F / 16.0F, 9.0F / 16.0F, 1.0F);
            DrawPart(block, pos, ref ctx, 9.0F / 16.0F, 12.0F / 16.0F, 0.0F, 13.0F / 16.0F, 15.0F / 16.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 9.0F / 16.0F, 12.0F / 16.0F, 14.0F / 16.0F, 13.0F / 16.0F, 15.0F / 16.0F, 1.0F);
        }
        else if (dir == 1)
        {
            DrawPart(block, pos, ref ctx, 1.0F / 16.0F, 6.0F / 16.0F, 0.0F, 3.0F / 16.0F, 15.0F / 16.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 1.0F / 16.0F, 6.0F / 16.0F, 14.0F / 16.0F, 3.0F / 16.0F, 15.0F / 16.0F, 1.0F);
            DrawPart(block, pos, ref ctx, 3.0F / 16.0F, 6.0F / 16.0F, 0.0F, 7.0F / 16.0F, 9.0F / 16.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 3.0F / 16.0F, 6.0F / 16.0F, 14.0F / 16.0F, 7.0F / 16.0F, 9.0F / 16.0F, 1.0F);
            DrawPart(block, pos, ref ctx, 3.0F / 16.0F, 12.0F / 16.0F, 0.0F, 7.0F / 16.0F, 15.0F / 16.0F, 2.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 3.0F / 16.0F, 12.0F / 16.0F, 14.0F / 16.0F, 7.0F / 16.0F, 15.0F / 16.0F, 1.0F);
        }
        else if (dir == 0)
        {
            DrawPart(block, pos, ref ctx, 0.0F, 6.0F / 16.0F, 13.0F / 16.0F, 2.0F / 16.0F, 15.0F / 16.0F, 15.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 6.0F / 16.0F, 13.0F / 16.0F, 1.0F, 15.0F / 16.0F, 15.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 0.0F, 6.0F / 16.0F, 9.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 13.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 6.0F / 16.0F, 9.0F / 16.0F, 1.0F, 9.0F / 16.0F, 13.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 0.0F, 12.0F / 16.0F, 9.0F / 16.0F, 2.0F / 16.0F, 15.0F / 16.0F, 13.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 12.0F / 16.0F, 9.0F / 16.0F, 1.0F, 15.0F / 16.0F, 13.0F / 16.0F);
        }
        else if (dir == 2)
        {
            DrawPart(block, pos, ref ctx, 0.0F, 6.0F / 16.0F, 1.0F / 16.0F, 2.0F / 16.0F, 15.0F / 16.0F, 3.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 6.0F / 16.0F, 1.0F / 16.0F, 1.0F, 15.0F / 16.0F, 3.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 0.0F, 6.0F / 16.0F, 3.0F / 16.0F, 2.0F / 16.0F, 9.0F / 16.0F, 7.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 6.0F / 16.0F, 3.0F / 16.0F, 1.0F, 9.0F / 16.0F, 7.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 0.0F, 12.0F / 16.0F, 3.0F / 16.0F, 2.0F / 16.0F, 15.0F / 16.0F, 7.0F / 16.0F);
            DrawPart(block, pos, ref ctx, 14.0F / 16.0F, 12.0F / 16.0F, 3.0F / 16.0F, 1.0F, 15.0F / 16.0F, 7.0F / 16.0F);
        }

        return true;
    }

    private static void DrawPart(Block block, in BlockPos pos, ref BlockRenderContext ctx, float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        (ctx with { OverrideBounds = new Box(minX, minY, minZ, maxX, maxY, maxZ) }).DrawBlock(block, pos);
    }
}
