using BetaSharp.Blocks.Materials;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Colors;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockVine : Block
{
    public BlockVine(int id) : base(id, 143, Material.Plant)
    {
        setTickRandomly(true);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z) => null;

    public override BlockRendererType getRenderType() => BlockRendererType.Vine;

    public override int getColor(int meta) => FoliageColors.getDefaultColor();

    public override int getColorMultiplier(IBlockReader reader, int x, int y, int z)
    {
        return reader.GetBiomeSource().GetBiome(x, z).GetFoliageColorAtCoords(reader, x, y, z);
    }

    public override void setupRenderBoundingBox() => setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F);

    public override void updateBoundingBox(IBlockReader world, EntityManager? entities, int x, int y, int z)
    {
        int meta = world.GetBlockMeta(x, y, z);
        float minX = 1.0F;
        float maxX = 1.0F;
        float minY = 1.0F;
        float maxY = 1.0F;
        float minZ = 1.0F;
        float maxZ = 1.0F;
        bool attached = meta > 0;

        if ((meta & 2) != 0)
        {
            minX = MathF.Max(minX, 1.0F / 16.0F);
            maxX = 0.0F;
            minY = 0.0F;
            maxY = 1.0F;
            minZ = 0.0F;
            maxZ = 1.0F;
            attached = true;
        }

        if ((meta & 8) != 0)
        {
            minX = 1.0F;
            maxX = MathF.Min(maxX, 15.0F / 16.0F);
            minY = 0.0F;
            maxY = 1.0F;
            minZ = 0.0F;
            maxZ = 1.0F;
            attached = true;
        }

        if ((meta & 4) != 0)
        {
            minZ = MathF.Max(minZ, 1.0F / 16.0F);
            maxZ = 0.0F;
            minX = 0.0F;
            maxX = 1.0F;
            minY = 0.0F;
            maxY = 1.0F;
            attached = true;
        }

        if ((meta & 1) != 0)
        {
            minZ = 1.0F;
            maxZ = MathF.Min(maxZ, 15.0F / 16.0F);
            minX = 0.0F;
            maxX = 1.0F;
            minY = 0.0F;
            maxY = 1.0F;
            attached = true;
        }

        if (!attached && CanBePlacedOn(world, x, y + 1, z))
        {
            minY = MathF.Min(minY, 15.0F / 16.0F);
            maxY = 1.0F;
            minX = 0.0F;
            maxX = 1.0F;
            minZ = 0.0F;
            maxZ = 1.0F;
        }

        setBoundingBox(minX, minY, minZ, maxX, maxY, maxZ);
    }

    public override bool canPlaceAt(CanPlaceAtContext context)
    {
        return CanBePlacedOn(context.World.Reader, context.X - 1, context.Y, context.Z)
               || CanBePlacedOn(context.World.Reader, context.X + 1, context.Y, context.Z)
               || CanBePlacedOn(context.World.Reader, context.X, context.Y, context.Z - 1)
               || CanBePlacedOn(context.World.Reader, context.X, context.Y, context.Z + 1)
               || context.World.Reader.GetBlockId(context.X, context.Y + 1, context.Z) == id;
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (@event.Direction == 1 && CanBePlacedOn(@event.World.Reader, @event.X, @event.Y + 1, @event.Z))
        {
            meta = 0;
        }
        else if (@event.Direction == 2 && CanBePlacedOn(@event.World.Reader, @event.X, @event.Y, @event.Z + 1))
        {
            meta = 1;
        }
        else if (@event.Direction == 3 && CanBePlacedOn(@event.World.Reader, @event.X, @event.Y, @event.Z - 1))
        {
            meta = 4;
        }
        else if (@event.Direction == 4 && CanBePlacedOn(@event.World.Reader, @event.X + 1, @event.Y, @event.Z))
        {
            meta = 8;
        }
        else if (@event.Direction == 5 && CanBePlacedOn(@event.World.Reader, @event.X - 1, @event.Y, @event.Z))
        {
            meta = 2;
        }

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta);
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        if (!@event.World.IsRemote && !CanStay(@event.World, @event.X, @event.Y, @event.Z))
        {
            dropStacks(new OnDropEvent(@event.World, @event.X, @event.Y, @event.Z, @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z)));
            @event.World.Writer.SetBlock(@event.X, @event.Y, @event.Z, 0);
        }
    }

    public override void onAfterBreak(OnAfterBreakEvent @event)
    {
        if (!@event.World.IsRemote && @event.Player.getHand() != null && @event.Player.getHand().itemId == Item.Shears.id)
        {
            dropStack(@event.World, @event.X, @event.Y, @event.Z, new ItemStack(id, 1, 0));
        }
        else
        {
            base.onAfterBreak(@event);
        }
    }

    public override int getDroppedItemCount() => 0;

    public override void onTick(OnTickEvent @event)
    {
        if (@event.World.IsRemote || Random.Shared.Next(4) != 0)
        {
            return;
        }

        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        if (@event.Y > 0 && @event.World.Reader.IsAir(@event.X, @event.Y - 1, @event.Z))
        {
            @event.World.Writer.SetBlockWithoutCallingOnPlaced(@event.X, @event.Y - 1, @event.Z, id, meta);
        }
    }

    private static bool CanBePlacedOn(IBlockReader world, int x, int y, int z)
    {
        int blockId = world.GetBlockId(x, y, z);
        return blockId > 0 && world.ShouldSuffocate(x, y, z);
    }

    private bool CanStay(IWorldContext world, int x, int y, int z)
    {
        int oldMeta = world.Reader.GetBlockMeta(x, y, z);
        int newMeta = oldMeta;

        if (oldMeta > 0)
        {
            for (int bit = 0; bit <= 3; ++bit)
            {
                int mask = 1 << bit;
                if ((oldMeta & mask) != 0)
                {
                    int dx = bit == 0 ? 0 : bit == 1 ? -1 : bit == 2 ? 0 : 1;
                    int dz = bit == 0 ? 1 : bit == 1 ? 0 : bit == 2 ? -1 : 0;
                    bool hasWall = CanBePlacedOn(world.Reader, x + dx, y, z + dz);
                    bool hasVineAbove = world.Reader.GetBlockId(x, y + 1, z) == id && (world.Reader.GetBlockMeta(x, y + 1, z) & mask) != 0;
                    if (!hasWall && !hasVineAbove)
                    {
                        newMeta &= ~mask;
                    }
                }
            }
        }

        if (newMeta == 0 && !CanBePlacedOn(world.Reader, x, y + 1, z))
        {
            return false;
        }

        if (newMeta != oldMeta)
        {
            world.Writer.SetBlockMeta(x, y, z, newMeta);
        }

        return true;
    }
}
