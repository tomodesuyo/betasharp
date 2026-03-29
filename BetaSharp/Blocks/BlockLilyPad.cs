using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockLilyPad : Block
{
    private const int LilyPadColor = 2129968;

    public BlockLilyPad(int id, int textureId) : base(id, textureId, Material.Plant)
    {
        float halfWidth = 0.5F;
        float height = 0.015625F;
        setBoundingBox(0.5F - halfWidth, 0.0F, 0.5F - halfWidth, 0.5F + halfWidth, height, 0.5F + halfWidth);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override BlockRendererType getRenderType() => BlockRendererType.LilyPad;

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z)
        => new(x + BoundingBox.MinX, y + BoundingBox.MinY, z + BoundingBox.MinZ, x + BoundingBox.MaxX, y + BoundingBox.MaxY, z + BoundingBox.MaxZ);

    public override int getColor(int meta) => LilyPadColor;

    public override int getColorMultiplier(IBlockReader reader, int x, int y, int z) => LilyPadColor;

    public override bool canPlaceAt(CanPlaceAtContext context)
    {
        return CanStay(context.World, context.X, context.Y, context.Z);
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        if (!CanStay(@event.World, @event.X, @event.Y, @event.Z))
        {
            dropStacks(new OnDropEvent(@event.World, @event.X, @event.Y, @event.Z, @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z)));
            @event.World.Writer.SetBlock(@event.X, @event.Y, @event.Z, 0);
        }
    }

    private static bool CanStay(IWorldContext world, int x, int y, int z)
    {
        return y >= 0 && y < 256 &&
               world.Reader.GetMaterial(x, y - 1, z) == Material.Water &&
               world.Reader.GetBlockMeta(x, y - 1, z) == 0;
    }
}
