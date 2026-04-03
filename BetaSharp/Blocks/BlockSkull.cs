using BetaSharp.Blocks.Entities;
using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Items;
using BetaSharp.Rules;
using BetaSharp.Stats;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockSkull : BlockWithEntity
{
    private const int FaceTexture = 225;
    private const int TopTexture = 240;
    private static readonly Dictionary<(IWorldContext World, int X, int Y, int Z), int> s_recentSkullDrops = [];

    public BlockSkull(int id) : base(id, FaceTexture, Material.Pumpkin)
    {
        setHardness(1.0F);
        setBoundingBox(0.25F, 0.0F, 0.25F, 0.75F, 0.5F, 0.75F);
    }

    public override BlockEntity getBlockEntity() => new BlockEntitySkull();

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Entity;

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        updateBoundingBox(world, entities, x, y, z);
        return base.getCollisionShape(world, entities, x, y, z);
    }

    public override void updateBoundingBox(IBlockReader world, EntityManager? entities, int x, int y, int z)
    {
        switch (world.GetBlockMeta(x, y, z))
        {
            case 2:
                setBoundingBox(0.25F, 0.25F, 0.5F, 0.75F, 0.75F, 1.0F);
                break;
            case 3:
                setBoundingBox(0.25F, 0.25F, 0.0F, 0.75F, 0.75F, 0.5F);
                break;
            case 4:
                setBoundingBox(0.5F, 0.25F, 0.25F, 1.0F, 0.75F, 0.75F);
                break;
            case 5:
                setBoundingBox(0.0F, 0.25F, 0.25F, 0.5F, 0.75F, 0.75F);
                break;
            default:
                setBoundingBox(0.25F, 0.0F, 0.25F, 0.75F, 0.5F, 0.75F);
                break;
        }
    }

    public override void setupRenderBoundingBox()
    {
        setBoundingBox(0.25F, 0.0F, 0.25F, 0.75F, 0.5F, 0.75F);
    }

    public override int getTexture(int side, int meta)
    {
        return side is 0 or 1 ? TopTexture : FaceTexture;
    }

    public override int getDroppedItemId(int blockMeta) => Item.Skull.id;

    public override void onPlaced(OnPlacedEvent @event)
    {
        base.onPlaced(@event);
        s_recentSkullDrops.Remove((@event.World, @event.X, @event.Y, @event.Z));

        int facing = @event.Direction is >= 2 and <= 5 ? @event.Direction : 1;
        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, facing);
    }

    public override bool canPlaceAt(CanPlaceAtContext evt)
    {
        if (!base.canPlaceAt(evt))
        {
            return false;
        }

        return evt.Direction switch
        {
            1 => evt.World.Reader.ShouldSuffocate(evt.X, evt.Y - 1, evt.Z),
            2 => evt.World.Reader.ShouldSuffocate(evt.X, evt.Y, evt.Z + 1),
            3 => evt.World.Reader.ShouldSuffocate(evt.X, evt.Y, evt.Z - 1),
            4 => evt.World.Reader.ShouldSuffocate(evt.X + 1, evt.Y, evt.Z),
            5 => evt.World.Reader.ShouldSuffocate(evt.X - 1, evt.Y, evt.Z),
            _ => true,
        };
    }

    public override void onBreak(OnBreakEvent ctx)
    {
        if (!ctx.World.IsRemote)
        {
            BlockEntitySkull? skull = ctx.World.Entities.GetBlockEntity<BlockEntitySkull>(ctx.X, ctx.Y, ctx.Z);
            s_recentSkullDrops[(ctx.World, ctx.X, ctx.Y, ctx.Z)] = skull?.GetSkullType() ?? BlockEntitySkull.Skeleton;
        }

        base.onBreak(ctx);
    }

    public override void onAfterBreak(OnAfterBreakEvent ctx)
    {
        ctx.Player.increaseStat(Stats.Stats.MineBlockStatArray[id], 1);
        if (!ctx.World.Rules.GetBool(DefaultRules.DoTileDrops))
        {
            s_recentSkullDrops.Remove((ctx.World, ctx.X, ctx.Y, ctx.Z));
            return;
        }

        int skullType = s_recentSkullDrops.Remove((ctx.World, ctx.X, ctx.Y, ctx.Z), out int cachedType)
            ? cachedType
            : BlockEntitySkull.Skeleton;

        dropStack(ctx.World, ctx.X, ctx.Y, ctx.Z, new ItemStack(Item.Skull, 1, skullType));
    }

    public static void CheckWitherSpawn(IWorldContext world, int x, int y, int z, BlockEntitySkull skull)
    {
        if (world.IsRemote || skull.GetSkullType() != BlockEntitySkull.WitherSkeleton || y < 2 || world.Difficulty == 0)
        {
            return;
        }

        bool xPattern =
            IsWitherSkull(world, x - 1, y, z) &&
            IsWitherSkull(world, x, y, z) &&
            IsWitherSkull(world, x + 1, y, z) &&
            IsSoulSand(world, x - 1, y - 1, z) &&
            IsSoulSand(world, x, y - 1, z) &&
            IsSoulSand(world, x + 1, y - 1, z) &&
            IsSoulSand(world, x, y - 2, z);

        bool zPattern =
            IsWitherSkull(world, x, y, z - 1) &&
            IsWitherSkull(world, x, y, z) &&
            IsWitherSkull(world, x, y, z + 1) &&
            IsSoulSand(world, x, y - 1, z - 1) &&
            IsSoulSand(world, x, y - 1, z) &&
            IsSoulSand(world, x, y - 1, z + 1) &&
            IsSoulSand(world, x, y - 2, z);

        if (!xPattern && !zPattern)
        {
            return;
        }

        ClearWitherPattern(world, x, y, z, xPattern);

        EntityWither wither = new(world);
        float yaw = xPattern ? 90.0F : 0.0F;
        wither.setPositionAndAnglesKeepPrevAngles(x + 0.5D, y - 1.45D, z + 0.5D, yaw, 0.0F);
        wither.bodyYaw = yaw;
        world.SpawnEntity(wither);

        for (int i = 0; i < 120; ++i)
        {
            world.Broadcaster.AddParticle(
                "smoke",
                x + world.Random.NextFloat(),
                y - 2 + world.Random.NextFloat() * 3.9D,
                z + world.Random.NextFloat(),
                0.0D,
                0.0D,
                0.0D);
        }
    }

    private static bool IsSoulSand(IWorldContext world, int x, int y, int z)
    {
        return world.Reader.GetBlockId(x, y, z) == Block.Soulsand.id;
    }

    private static bool IsWitherSkull(IWorldContext world, int x, int y, int z)
    {
        if (world.Reader.GetBlockId(x, y, z) != Block.Skull.id)
        {
            return false;
        }

        return world.Entities.GetBlockEntity<BlockEntitySkull>(x, y, z)?.GetSkullType() == BlockEntitySkull.WitherSkeleton;
    }

    private static void ClearWitherPattern(IWorldContext world, int x, int y, int z, bool xPattern)
    {
        world.Writer.SetBlockWithoutCallingOnPlaced(x, y, z, 0, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 1, z, 0, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 2, z, 0, 0);

        if (xPattern)
        {
            world.Writer.SetBlockWithoutCallingOnPlaced(x - 1, y, z, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x + 1, y, z, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x - 1, y - 1, z, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x + 1, y - 1, z, 0, 0);
        }
        else
        {
            world.Writer.SetBlockWithoutCallingOnPlaced(x, y, z - 1, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x, y, z + 1, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 1, z - 1, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 1, z + 1, 0, 0);
        }
    }
}
