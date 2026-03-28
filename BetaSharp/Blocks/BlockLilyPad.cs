using BetaSharp.Blocks.Materials;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal class BlockLilyPad : Block
{
    public BlockLilyPad(int id, int textureId) : base(id, textureId, Material.Plant)
    {
        float inset = 0.0625F;
        setBoundingBox(inset, 0.0F, inset, 1.0F - inset, 0.015625F, 1.0F - inset);
    }

    public override bool isOpaque() => false;

    public override bool isFullCube() => false;

    public override Box? getCollisionShape(IBlockReader world, EntityManager entities, int x, int y, int z) => null;

    public override bool canPlaceAt(CanPlaceAtContext context)
    {
        int belowId = context.World.Reader.GetBlockId(context.X, context.Y - 1, context.Z);
        return context.Y > 0
               && context.World.Reader.IsAir(context.X, context.Y, context.Z)
               && (belowId == Block.Water.id || belowId == Block.FlowingWater.id)
               && context.World.Reader.GetBlockMeta(context.X, context.Y - 1, context.Z) == 0;
    }
}
