using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockCarpet : Block
{
    private const float Height = 1.0F / 16.0F;

    public BlockCarpet(int id) : base(id, Material.Wool)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, Height, 1.0F);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override int getTexture(int side, int meta)
    {
        return Block.Wool.getTexture(side, meta);
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta;
    }

    public override void setupRenderBoundingBox()
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, Height, 1.0F);
    }

    public override void updateBoundingBox(IBlockReader blockReader, EntityManager? entities, int x, int y, int z)
    {
        setBoundingBox(0.0F, 0.0F, 0.0F, 1.0F, Height, 1.0F);
    }

    public override bool canPlaceAt(CanPlaceAtContext evt)
    {
        return base.canPlaceAt(evt) && CanStay(evt.World.Reader, evt.X, evt.Y, evt.Z);
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        if (!CanStay(@event.World.Reader, @event.X, @event.Y, @event.Z))
        {
            dropStacks(new OnDropEvent(@event.World, @event.X, @event.Y, @event.Z, @event.Meta));
            @event.World.Writer.SetBlock(@event.X, @event.Y, @event.Z, 0);
        }
    }

    public override bool isSideVisible(IBlockReader world, int x, int y, int z, int side)
    {
        return side == 1 || base.isSideVisible(world, x, y, z, side);
    }

    private static bool CanStay(IBlockReader world, int x, int y, int z)
    {
        return !world.IsAir(x, y - 1, z);
    }
}
