using BetaSharp.Blocks.Materials;
using BetaSharp.Items;
using BetaSharp.Rules;
using BetaSharp.Worlds.Colors;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

public class BlockLeaves : BlockLeavesBase
{
    private readonly ThreadLocal<int[]?> s_decayRegion = new(() => null);
    private readonly int spriteIndex;

    public BlockLeaves(int id, int textureId) : base(id, textureId, Material.Leaves, false)
    {
        spriteIndex = textureId;
        setTickRandomly(true);
    }

    public override int getColor(int meta)
    {
        int variant = meta & 3;
        return variant == 1 ? FoliageColors.getSpruceColor() : variant == 2 ? FoliageColors.getBirchColor() : FoliageColors.getDefaultColor();
    }

    public override int getColorMultiplier(IBlockReader reader, int x, int y, int z)
    {
        int meta = reader.GetBlockMeta(x, y, z);
        int variant = meta & 3;
        if (variant == 1)
        {
            return FoliageColors.getSpruceColor();
        }

        if (variant == 2)
        {
            return FoliageColors.getBirchColor();
        }

        int red = 0;
        int green = 0;
        int blue = 0;

        for (int offsetZ = -1; offsetZ <= 1; ++offsetZ)
        {
            for (int offsetX = -1; offsetX <= 1; ++offsetX)
            {
                int color = reader.GetBiomeSource().GetBiome(x + offsetX, z + offsetZ).GetFoliageColorAtCoords(reader, x + offsetX, y, z + offsetZ);
                red += (color & 0xFF0000) >> 16;
                green += (color & 0x00FF00) >> 8;
                blue += color & 0x0000FF;
            }
        }

        return (red / 9 & 255) << 16 | (green / 9 & 255) << 8 | blue / 9 & 255;
    }

    public override void onBreak(OnBreakEvent @event)
    {
        sbyte searchRadius = 1;
        int loadCheckExtent = searchRadius + 1;
        if (@event.World.ChunkHost.IsRegionLoaded(@event.X - loadCheckExtent, @event.Y - loadCheckExtent, @event.Z - loadCheckExtent, @event.X + loadCheckExtent, @event.Y + loadCheckExtent, @event.Z + loadCheckExtent))
        {
            for (int offsetX = -searchRadius; offsetX <= searchRadius; ++offsetX)
            {
                for (int offsetY = -searchRadius; offsetY <= searchRadius; ++offsetY)
                {
                    for (int offsetZ = -searchRadius; offsetZ <= searchRadius; ++offsetZ)
                    {
                        int blockId = @event.World.Reader.GetBlockId(@event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ);
                        if (blockId == Leaves.id)
                        {
                            int leavesMeta = @event.World.Reader.GetBlockMeta(@event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ);
                            @event.World.Writer.SetBlockMetaWithoutNotifyingNeighbors(@event.X + offsetX, @event.Y + offsetY, @event.Z + offsetZ, leavesMeta | 8);
                        }
                    }
                }
            }
        }
    }

    public override void onTick(OnTickEvent @event)
    {
        if (!@event.World.IsRemote)
        {
            int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z);
            if ((meta & 8) != 0 && (meta & 4) == 0)
            {
                sbyte decayRadius = 4;
                int loadCheckExtent = decayRadius + 1;
                sbyte regionSize = 32;
                int planeSize = regionSize * regionSize;
                int centerOffset = regionSize / 2;
                if (s_decayRegion.Value == null)
                {
                    s_decayRegion.Value = new int[regionSize * regionSize * regionSize];
                }

                int[] decayRegion = s_decayRegion.Value;

                int distanceToLog;
                if (@event.World.ChunkHost.IsRegionLoaded(@event.X - loadCheckExtent, @event.Y - loadCheckExtent, @event.Z - loadCheckExtent, @event.X + loadCheckExtent, @event.Y + loadCheckExtent, @event.Z + loadCheckExtent))
                {
                    distanceToLog = -decayRadius;

                    while (distanceToLog <= decayRadius)
                    {
                        int dx;
                        int dy;

                        for (dx = -decayRadius; dx <= decayRadius; ++dx)
                        {
                            for (dy = -decayRadius; dy <= decayRadius; ++dy)
                            {
                                int blockId = @event.World.Reader.GetBlockId(@event.X + distanceToLog, @event.Y + dx, @event.Z + dy);
                                if (blockId == Log.id)
                                {
                                    decayRegion[(distanceToLog + centerOffset) * planeSize + (dx + centerOffset) * regionSize + dy + centerOffset] = 0;
                                }
                                else if (blockId == Leaves.id)
                                {
                                    decayRegion[(distanceToLog + centerOffset) * planeSize + (dx + centerOffset) * regionSize + dy + centerOffset] = -2;
                                }
                                else
                                {
                                    decayRegion[(distanceToLog + centerOffset) * planeSize + (dx + centerOffset) * regionSize + dy + centerOffset] = -1;
                                }
                            }
                        }

                        ++distanceToLog;
                    }

                    for (distanceToLog = 1; distanceToLog <= 4; ++distanceToLog)
                    {
                        int dx;
                        int dy;
                        int dz;

                        for (dx = -decayRadius; dx <= decayRadius; ++dx)
                        {
                            for (dy = -decayRadius; dy <= decayRadius; ++dy)
                            {
                                for (dz = -decayRadius; dz <= decayRadius; ++dz)
                                {
                                    if (decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset] == distanceToLog - 1)
                                    {
                                        if (decayRegion[(dx + centerOffset - 1) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset] == -2)
                                        {
                                            decayRegion[(dx + centerOffset - 1) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset] = distanceToLog;
                                        }

                                        if (decayRegion[(dx + centerOffset + 1) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset] == -2)
                                        {
                                            decayRegion[(dx + centerOffset + 1) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset] = distanceToLog;
                                        }

                                        if (decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset - 1) * regionSize + dz + centerOffset] == -2)
                                        {
                                            decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset - 1) * regionSize + dz + centerOffset] = distanceToLog;
                                        }

                                        if (decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset + 1) * regionSize + dz + centerOffset] == -2)
                                        {
                                            decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset + 1) * regionSize + dz + centerOffset] = distanceToLog;
                                        }

                                        if (decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset) * regionSize + (dz + centerOffset - 1)] == -2)
                                        {
                                            decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset) * regionSize + (dz + centerOffset - 1)] = distanceToLog;
                                        }

                                        if (decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset + 1] == -2)
                                        {
                                            decayRegion[(dx + centerOffset) * planeSize + (dy + centerOffset) * regionSize + dz + centerOffset + 1] = distanceToLog;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                distanceToLog = decayRegion[centerOffset * planeSize + centerOffset * regionSize + centerOffset];
                if (distanceToLog >= 0)
                {
                    @event.World.Writer.SetBlockMetaWithoutNotifyingNeighbors(@event.X, @event.Y, @event.Z, meta & -9);
                }
                else
                {
                    breakLeaves(@event.World, @event.X, @event.Y, @event.Z);
                }
            }
        }
    }

    private void breakLeaves(IWorldContext level, int x, int y, int z)
    {
        dropStacks(new OnDropEvent(level, x, y, z, level.Reader.GetBlockMeta(x, y, z)));
        level.Writer.SetBlock(x, y, z, 0);
    }

    public override int getDroppedItemId(int blockMeta) => Sapling.id;

    public override void dropStacks(OnDropEvent ctx)
    {
        if (!ctx.World.IsRemote && ctx.World.Rules.GetBool(DefaultRules.DoTileDrops))
        {
            int saplingChance = (ctx.Meta & 3) == 3 ? 40 : 20;
            if (Random.Shared.Next(saplingChance) == 0 && Random.Shared.NextSingle() <= ctx.Luck)
            {
                dropStack(ctx.World, ctx.X, ctx.Y, ctx.Z, new ItemStack(getDroppedItemId(ctx.Meta), 1, getDroppedItemMeta(ctx.Meta)));
            }

            if ((ctx.Meta & 3) == 0 && Random.Shared.Next(200) == 0 && Random.Shared.NextSingle() <= ctx.Luck)
            {
                dropStack(ctx.World, ctx.X, ctx.Y, ctx.Z, new ItemStack(Item.Apple.id, 1, 0));
            }
        }
    }

    public override void onAfterBreak(OnAfterBreakEvent ctx)
    {
        if (!ctx.World.IsRemote && ctx.Player.getHand() != null && ctx.Player.getHand().itemId == Item.Shears.id)
        {
            ctx.Player.increaseStat(Stats.Stats.MineBlockStatArray[id], 1);
            dropStack(ctx.World, ctx.X, ctx.Y, ctx.Z, new ItemStack(Leaves.id, 1, ctx.Meta & 3));
        }
        else
        {
            base.onAfterBreak(ctx);
        }
    }

    protected override int getDroppedItemMeta(int blockMeta) => blockMeta & 3;

    public override bool isOpaque() => !graphicsLevel;

    public override int getTexture(int side, int meta)
    {
        return (meta & 3) switch
        {
            1 => textureId + 80,
            3 => textureId + 144,
            _ => textureId
        };
    }

    public void setGraphicsLevel(bool bl)
    {
        graphicsLevel = bl;
        textureId = spriteIndex + (bl ? 0 : 1);
    }

    public override void onSteppedOn(OnEntityStepEvent ctx) => base.onSteppedOn(ctx);
}
