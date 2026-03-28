using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockFenceGate : Block
{
    public BlockFenceGate(int id, int textureId) : base(id, textureId, Material.Wood)
    {
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.FenceGate;

    public override bool canPlaceAt(CanPlaceAtContext context) => context.World.Reader.GetMaterial(context.X, context.Y - 1, context.Z).IsSolid && base.canPlaceAt(context);

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        int meta = world.GetBlockMeta(x, y, z);
        if ((meta & 4) != 0)
        {
            return null;
        }

        return (meta & 1) == 0
            ? new Box(x, y, z + 0.375F, x + 1.0F, y + 1.5F, z + 0.625F)
            : new Box(x + 0.375F, y, z, x + 0.625F, y + 1.5F, z + 1.0F);
    }

    public override void updateBoundingBox(IBlockReader world, EntityManager? entities, int x, int y, int z)
    {
        int meta = world.GetBlockMeta(x, y, z) & 3;
        if (meta != 2 && meta != 0)
        {
            setBoundingBox(6.0F / 16.0F, 0.0F, 0.0F, 10.0F / 16.0F, 1.0F, 1.0F);
        }
        else
        {
            setBoundingBox(0.0F, 0.0F, 6.0F / 16.0F, 1.0F, 1.0F, 10.0F / 16.0F);
        }
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        int meta = 0;
        if (@event.Placer != null)
        {
            meta = MathHelper.Floor(@event.Placer.yaw * 4.0F / 360.0F + 0.5D) & 3;
        }

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta);
    }

    public override bool onUse(OnUseEvent @event)
    {
        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (IsOpen(meta))
        {
            @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta & ~4);
        }
        else
        {
            int playerDir = MathHelper.Floor(@event.Player.yaw * 4.0F / 360.0F + 0.5D) & 3;
            int gateDir = meta & 3;
            if (gateDir == (playerDir + 2) % 4)
            {
                meta = playerDir;
            }

            @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta | 4);
        }

        @event.World.Broadcaster.WorldEvent(1003, @event.X, @event.Y, @event.Z, 0);
        return true;
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        if (!@event.World.IsRemote)
        {
            if (!@event.World.Reader.GetMaterial(@event.X, @event.Y - 1, @event.Z).IsSolid)
            {
                dropStacks(new OnDropEvent(@event.World, @event.X, @event.Y, @event.Z, @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z)));
                @event.World.Writer.SetBlock(@event.X, @event.Y, @event.Z, 0);
                return;
            }

            int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
            bool isPowered = @event.World.Redstone.IsPowered(@event.X, @event.Y, @event.Z);
            if (isPowered && !IsOpen(meta))
            {
                @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta | 4);
                @event.World.Broadcaster.WorldEvent(1003, @event.X, @event.Y, @event.Z, 0);
            }
            else if (!isPowered && IsOpen(meta))
            {
                @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta & ~4);
                @event.World.Broadcaster.WorldEvent(1003, @event.X, @event.Y, @event.Z, 0);
            }
        }
    }

    public static bool IsOpen(int meta) => (meta & 4) != 0;
}
