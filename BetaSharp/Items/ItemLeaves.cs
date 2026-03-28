using BetaSharp.Blocks;
using BetaSharp.Worlds.Colors;

namespace BetaSharp.Items;

internal class ItemLeaves : ItemBlock
{

    public ItemLeaves(int id) : base(id)
    {
        setMaxDamage(0);
        setHasSubtypes(true);
    }

    public override int getPlacementMetadata(int meta)
    {
        return meta | 4;
    }

    public override int getTextureId(int meta)
    {
        return Block.Leaves.getTexture(0, meta);
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        return getItemName();
    }

    public override int getColorMultiplier(int leafType)
    {
        int variant = leafType & 3;
        return variant == 1 ? FoliageColors.getSpruceColor() : variant == 2 ? FoliageColors.getBirchColor() : FoliageColors.getDefaultColor();
    }
}
