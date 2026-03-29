using BetaSharp.Blocks;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Blocks.Renderers;

public sealed class DragonEggRenderer : IBlockRenderer
{
    public bool Draw(Block block, in BlockPos pos, ref BlockRenderContext ctx)
    {
        int currentY = 0;

        for (int layer = 0; layer < 8; ++layer)
        {
            int radius = 0;
            int height = 1;

            if (layer == 0)
            {
                radius = 2;
            }
            else if (layer == 1)
            {
                radius = 3;
            }
            else if (layer == 2)
            {
                radius = 4;
            }
            else if (layer == 3)
            {
                radius = 5;
                height = 2;
            }
            else if (layer == 4)
            {
                radius = 6;
                height = 3;
            }
            else if (layer == 5)
            {
                radius = 7;
                height = 5;
            }
            else if (layer == 6)
            {
                radius = 6;
                height = 2;
            }
            else if (layer == 7)
            {
                radius = 3;
            }

            float minY = 1.0F - (currentY + height) / 16.0F;
            float maxY = 1.0F - currentY / 16.0F;
            currentY += height;

            var eggCtx = ctx with
            {
                OverrideBounds = new Box(
                    0.5F - radius / 16.0F,
                    minY,
                    0.5F - radius / 16.0F,
                    0.5F + radius / 16.0F,
                    maxY,
                    0.5F + radius / 16.0F)
            };
            eggCtx.DrawBlock(block, pos);
        }

        return true;
    }
}
