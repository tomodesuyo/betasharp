using BetaSharp.Blocks.Materials;
using BetaSharp.Items;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Colors;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockVine : Block
{
    private static readonly int[] s_offsetX = [0, -1, 0, 1];
    private static readonly int[] s_offsetZ = [1, 0, -1, 0];
    private static readonly int[] s_vineGrowth = [-1, -1, 2, 0, 1, 3];

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
        float minY = 1.0F;
        float minZ = 1.0F;
        float maxX = 0.0F;
        float maxY = 0.0F;
        float maxZ = 0.0F;
        bool attached = meta > 0;

        if ((meta & 2) != 0)
        {
            maxX = MathF.Max(maxX, 1.0F / 16.0F);
            minX = 0.0F;
            minY = 0.0F;
            maxY = 1.0F;
            minZ = 0.0F;
            maxZ = 1.0F;
            attached = true;
        }

        if ((meta & 8) != 0)
        {
            minX = MathF.Min(minX, 15.0F / 16.0F);
            maxX = 1.0F;
            minY = 0.0F;
            maxY = 1.0F;
            minZ = 0.0F;
            maxZ = 1.0F;
            attached = true;
        }

        if ((meta & 4) != 0)
        {
            maxZ = MathF.Max(maxZ, 1.0F / 16.0F);
            minZ = 0.0F;
            minX = 0.0F;
            maxX = 1.0F;
            minY = 0.0F;
            maxY = 1.0F;
            attached = true;
        }

        if ((meta & 1) != 0)
        {
            minZ = MathF.Min(minZ, 15.0F / 16.0F);
            maxZ = 1.0F;
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

        const int radius = 4;
        int vineBudget = 5;
        bool crowded = false;

        for (int checkX = @event.X - radius; checkX <= @event.X + radius && !crowded; ++checkX)
        {
            for (int checkZ = @event.Z - radius; checkZ <= @event.Z + radius && !crowded; ++checkZ)
            {
                for (int checkY = @event.Y - 1; checkY <= @event.Y + 1; ++checkY)
                {
                    if (@event.World.Reader.GetBlockId(checkX, checkY, checkZ) == id && --vineBudget <= 0)
                    {
                        crowded = true;
                        break;
                    }
                }
            }
        }

        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
        int growthDirection = Random.Shared.Next(6);
        int side = s_vineGrowth[growthDirection];

        if (growthDirection == 1 && @event.Y < 255 && @event.World.Reader.IsAir(@event.X, @event.Y + 1, @event.Z))
        {
            if (crowded)
            {
                return;
            }

            int attachMeta = Random.Shared.Next(16) & meta;
            if (attachMeta > 0)
            {
                for (int i = 0; i <= 3; ++i)
                {
                    if (!CanBePlacedOn(@event.World.Reader, @event.X + s_offsetX[i], @event.Y + 1, @event.Z + s_offsetZ[i]))
                    {
                        attachMeta &= ~(1 << i);
                    }
                }

                if (attachMeta > 0)
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(@event.X, @event.Y + 1, @event.Z, id, attachMeta);
                }
            }

            return;
        }

        if (growthDirection >= 2 && growthDirection <= 5 && (meta & 1 << side) == 0)
        {
            if (crowded)
            {
                return;
            }

            int sideX = @event.X + s_offsetX[side];
            int sideZ = @event.Z + s_offsetZ[side];
            int sideBlockId = @event.World.Reader.GetBlockId(sideX, @event.Y, sideZ);
            if (sideBlockId != 0 && Block.Blocks[sideBlockId] != null)
            {
                if (Block.Blocks[sideBlockId].material.BlocksMovement && Block.Blocks[sideBlockId].isFullCube())
                {
                    @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta | 1 << side);
                }
            }
            else
            {
                int clockwise = side + 1 & 3;
                int counterClockwise = side + 3 & 3;

                if ((meta & 1 << clockwise) != 0 && CanBePlacedOn(@event.World.Reader, sideX + s_offsetX[clockwise], @event.Y, sideZ + s_offsetZ[clockwise]))
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(sideX, @event.Y, sideZ, id, 1 << clockwise);
                }
                else if ((meta & 1 << counterClockwise) != 0 && CanBePlacedOn(@event.World.Reader, sideX + s_offsetX[counterClockwise], @event.Y, sideZ + s_offsetZ[counterClockwise]))
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(sideX, @event.Y, sideZ, id, 1 << counterClockwise);
                }
                else if ((meta & 1 << clockwise) != 0 &&
                         @event.World.Reader.IsAir(sideX + s_offsetX[clockwise], @event.Y, sideZ + s_offsetZ[clockwise]) &&
                         CanBePlacedOn(@event.World.Reader, @event.X + s_offsetX[clockwise], @event.Y, @event.Z + s_offsetZ[clockwise]))
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(sideX + s_offsetX[clockwise], @event.Y, sideZ + s_offsetZ[clockwise], id, 1 << (side + 2 & 3));
                }
                else if ((meta & 1 << counterClockwise) != 0 &&
                         @event.World.Reader.IsAir(sideX + s_offsetX[counterClockwise], @event.Y, sideZ + s_offsetZ[counterClockwise]) &&
                         CanBePlacedOn(@event.World.Reader, @event.X + s_offsetX[counterClockwise], @event.Y, @event.Z + s_offsetZ[counterClockwise]))
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(sideX + s_offsetX[counterClockwise], @event.Y, sideZ + s_offsetZ[counterClockwise], id, 1 << (side + 2 & 3));
                }
                else if (CanBePlacedOn(@event.World.Reader, sideX, @event.Y + 1, sideZ))
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(sideX, @event.Y, sideZ, id, 0);
                }
            }
        }
        else if (@event.Y > 1)
        {
            int belowBlockId = @event.World.Reader.GetBlockId(@event.X, @event.Y - 1, @event.Z);
            if (belowBlockId == 0)
            {
                int belowMeta = Random.Shared.Next(16) & meta;
                if (belowMeta > 0)
                {
                    @event.World.Writer.SetBlockWithoutCallingOnPlaced(@event.X, @event.Y - 1, @event.Z, id, belowMeta);
                }
            }
            else if (belowBlockId == id)
            {
                int growMeta = Random.Shared.Next(16) & meta;
                int belowMeta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y - 1, @event.Z);
                if (belowMeta != (belowMeta | growMeta))
                {
                    @event.World.Writer.SetBlockMeta(@event.X, @event.Y - 1, @event.Z, belowMeta | growMeta);
                }
            }
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
