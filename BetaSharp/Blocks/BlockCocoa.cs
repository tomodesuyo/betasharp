using BetaSharp.Blocks.Materials;
using BetaSharp.Items;
using BetaSharp.Rules;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockCocoa : Block
{
    private static readonly int[] s_offsetX = [0, -1, 0, 1];
    private static readonly int[] s_offsetZ = [1, 0, -1, 0];

    public BlockCocoa(int id) : base(id, 168, Material.Plant)
    {
        setTickRandomly(true);
        setupRenderBoundingBox();
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.Cocoa;

    public override int getTexture(int side, int meta) => textureId + 2 - GetAge(meta);

    public override bool canPlaceAt(CanPlaceAtContext context)
    {
        return base.canPlaceAt(context) && FindSupportedDirection(context.World.Reader, context.X, context.Y, context.Z) >= 0;
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        int age = GetAge(@event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z));
        int direction = GetPlacementDirection(@event.Direction);
        if (direction < 0 || !IsSupportedByLog(@event.World.Reader, @event.X, @event.Y, @event.Z, direction))
        {
            direction = FindSupportedDirection(@event.World.Reader, @event.X, @event.Y, @event.Z);
        }

        if (direction < 0)
        {
            @event.World.Writer.SetBlock(@event.X, @event.Y, @event.Z, 0);
            return;
        }

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, MakeMeta(direction, age));
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        if (!CanStay(@event.World.Reader, @event.X, @event.Y, @event.Z, @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z)))
        {
            BreakIfInvalid(@event.World, @event.X, @event.Y, @event.Z);
        }
    }

    public override void onTick(OnTickEvent @event)
    {
        if (@event.World.IsRemote)
        {
            return;
        }

        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (!CanStay(@event.World.Reader, @event.X, @event.Y, @event.Z, meta))
        {
            BreakIfInvalid(@event.World, @event.X, @event.Y, @event.Z);
            return;
        }

        int age = GetAge(meta);
        if (age < 2 && @event.World.Random.NextInt(5) == 0)
        {
            @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, MakeMeta(GetDirection(meta), age + 1));
        }
    }

    public void ApplyBonemeal(IWorldContext world, int x, int y, int z)
    {
        int meta = world.Reader.GetBlockMeta(x, y, z);
        int age = GetAge(meta);
        if (age < 2)
        {
            world.Writer.SetBlockMeta(x, y, z, MakeMeta(GetDirection(meta), 2));
        }
    }

    public override void updateBoundingBox(IBlockReader blockReader, EntityManager? entities, int x, int y, int z)
    {
        int meta = blockReader.GetBlockMeta(x, y, z);
        SetBounds(GetAge(meta), GetDirection(meta));
    }

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        updateBoundingBox(world, entities, x, y, z);
        return BoundingBox.Offset(x, y, z);
    }

    public override Box getBoundingBox(IBlockReader world, EntityManager entities, int x, int y, int z)
    {
        updateBoundingBox(world, entities, x, y, z);
        return BoundingBox.Offset(x, y, z);
    }

    public override void setupRenderBoundingBox()
    {
        SetBounds(2, 2);
    }

    public override int getDroppedItemCount() => 0;

    public override void dropStacks(OnDropEvent ctx)
    {
        if (ctx.World.IsRemote || !ctx.World.Rules.GetBool(DefaultRules.DoTileDrops))
        {
            return;
        }

        int count = GetAge(ctx.Meta) >= 2 ? 3 : 1;
        if (Random.Shared.NextSingle() <= ctx.Luck)
        {
            dropStack(ctx.World, ctx.X, ctx.Y, ctx.Z, new ItemStack(Item.Dye, count, 3));
        }
    }

    private void BreakIfInvalid(IWorldContext world, int x, int y, int z)
    {
        int meta = world.Reader.GetBlockMeta(x, y, z);
        dropStacks(new OnDropEvent(world, x, y, z, meta));
        world.Writer.SetBlock(x, y, z, 0);
    }

    private static int GetAge(int meta) => (meta & 12) >> 2;

    private static int GetDirection(int meta) => meta & 3;

    private static int MakeMeta(int direction, int age) => direction | age << 2;

    private static int GetPlacementDirection(int side)
    {
        return side switch
        {
            2 => 0,
            3 => 2,
            4 => 3,
            5 => 1,
            _ => -1,
        };
    }

    private static int FindSupportedDirection(IBlockReader world, int x, int y, int z)
    {
        for (int direction = 0; direction < 4; ++direction)
        {
            if (IsSupportedByLog(world, x, y, z, direction))
            {
                return direction;
            }
        }

        return -1;
    }

    private static bool CanStay(IBlockReader world, int x, int y, int z, int meta)
    {
        return IsSupportedByLog(world, x, y, z, GetDirection(meta));
    }

    private static bool IsSupportedByLog(IBlockReader world, int x, int y, int z, int direction)
    {
        int supportX = x + s_offsetX[direction];
        int supportZ = z + s_offsetZ[direction];
        return world.GetBlockId(supportX, y, supportZ) == Block.Log.id && (world.GetBlockMeta(supportX, y, supportZ) & 3) == 3;
    }

    private void SetBounds(int age, int direction)
    {
        int width = 4 + age * 2;
        int height = 5 + age * 2;
        float halfWidth = width / 2.0F;

        switch (direction)
        {
            case 0:
                setBoundingBox((8.0F - halfWidth) / 16.0F, (12.0F - height) / 16.0F, (15.0F - width) / 16.0F, (8.0F + halfWidth) / 16.0F, 0.75F, 15.0F / 16.0F);
                break;
            case 1:
                setBoundingBox(1.0F / 16.0F, (12.0F - height) / 16.0F, (8.0F - halfWidth) / 16.0F, (1.0F + width) / 16.0F, 0.75F, (8.0F + halfWidth) / 16.0F);
                break;
            case 3:
                setBoundingBox((15.0F - width) / 16.0F, (12.0F - height) / 16.0F, (8.0F - halfWidth) / 16.0F, 15.0F / 16.0F, 0.75F, (8.0F + halfWidth) / 16.0F);
                break;
            default:
                setBoundingBox((8.0F - halfWidth) / 16.0F, (12.0F - height) / 16.0F, 1.0F / 16.0F, (8.0F + halfWidth) / 16.0F, 0.75F, (1.0F + width) / 16.0F);
                break;
        }
    }
}
