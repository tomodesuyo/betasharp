using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockEndPortalFrame : Block
{
    public BlockEndPortalFrame(int id) : base(id, 159, Material.Stone)
    {
    }

    public override int getTexture(int side, int meta) => side == 1 ? textureId - 1 : side == 0 ? textureId + 16 : textureId;

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.EndPortalFrame;

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 13.0F / 16.0F, 1.0F);

    public override void addIntersectingBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z, Box box, List<Box> boxes)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 13.0F / 16.0F, 1.0F);
        base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        if (HasEye(world.GetBlockMeta(x, y, z)))
        {
            setBoundingBox(5.0F / 16.0F, 13.0F / 16.0F, 5.0F / 16.0F, 11.0F / 16.0F, 1.0F, 11.0F / 16.0F);
            base.addIntersectingBoundingBox(world, entities, x, y, z, box, boxes);
        }

        setupRenderBoundingBox();
    }

    public override int getDroppedItemId(int blockMeta) => 0;

    public override void onPlaced(OnPlacedEvent @event)
    {
        int meta = 0;
        if (@event.Placer != null)
        {
            meta = ((MathHelper.Floor(@event.Placer.yaw * 4.0F / 360.0F + 0.5D) & 3) + 2) % 4;
        }

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta);
    }

    public static bool HasEye(int meta) => (meta & 4) != 0;

    public static bool TryActivatePortal(IWorldContext world, int x, int y, int z)
    {
        int meta = world.Reader.GetBlockMeta(x, y, z);
        int facing = meta & 3;
        int[] offsetX = [0, -1, 0, 1];
        int[] offsetZ = [1, 0, -1, 0];
        int[] enderEyeMetaToDirection = [1, 2, 3, 0];
        int parallelDirection = enderEyeMetaToDirection[facing];
        int start = 0;
        int end = 0;
        bool foundAny = false;
        bool valid = true;

        for (int i = -2; i <= 2; ++i)
        {
            int frameX = x + offsetX[parallelDirection] * i;
            int frameZ = z + offsetZ[parallelDirection] * i;
            if (world.Reader.GetBlockId(frameX, y, frameZ) == Block.EndPortalFrame.id)
            {
                int frameMeta = world.Reader.GetBlockMeta(frameX, y, frameZ);
                if (!HasEye(frameMeta))
                {
                    valid = false;
                    break;
                }

                if (!foundAny)
                {
                    start = i;
                    end = i;
                    foundAny = true;
                }
                else
                {
                    end = i;
                }
            }
        }

        if (!valid || !foundAny || end != start + 2)
        {
            return false;
        }

        for (int i = start; i <= end; ++i)
        {
            int frameX = x + offsetX[parallelDirection] * i + offsetX[facing] * 4;
            int frameZ = z + offsetZ[parallelDirection] * i + offsetZ[facing] * 4;
            int frameId = world.Reader.GetBlockId(frameX, y, frameZ);
            int frameMeta = world.Reader.GetBlockMeta(frameX, y, frameZ);
            if (frameId != Block.EndPortalFrame.id || !HasEye(frameMeta))
            {
                return false;
            }
        }

        for (int i = start - 1; i <= end + 1; i += 4)
        {
            for (int step = 1; step <= 3; ++step)
            {
                int frameX = x + offsetX[parallelDirection] * i + offsetX[facing] * step;
                int frameZ = z + offsetZ[parallelDirection] * i + offsetZ[facing] * step;
                int frameId = world.Reader.GetBlockId(frameX, y, frameZ);
                int frameMeta = world.Reader.GetBlockMeta(frameX, y, frameZ);
                if (frameId != Block.EndPortalFrame.id || !HasEye(frameMeta))
                {
                    return false;
                }
            }
        }

        for (int i = start; i <= end; ++i)
        {
            for (int step = 1; step <= 3; ++step)
            {
                int portalX = x + offsetX[parallelDirection] * i + offsetX[facing] * step;
                int portalZ = z + offsetZ[parallelDirection] * i + offsetZ[facing] * step;
                world.Writer.SetBlock(portalX, y, portalZ, Block.EndPortal.id, 0, doUpdate: false);
            }
        }

        return true;
    }
}
