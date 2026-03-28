using BetaSharp.Blocks.Materials;
using BetaSharp.Items;

namespace BetaSharp.Blocks;

internal class BlockMelon : Block
{
    public BlockMelon(int id) : base(id, 136, Material.Pumpkin)
    {
    }

    public override int getTexture(int side)
    {
        return side is 0 or 1 ? 137 : textureId;
    }

    public override int getTexture(int side, int meta)
    {
        return getTexture(side);
    }

    public override int getDroppedItemId(int blockMeta)
    {
        return Item.Melon.id;
    }

    public override int getDroppedItemCount()
    {
        return 3 + Random.Shared.Next(5);
    }
}
