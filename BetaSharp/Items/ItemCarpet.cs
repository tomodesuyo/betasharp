using BetaSharp.Blocks;

namespace BetaSharp.Items;

internal sealed class ItemCarpet : ItemBlock
{
    public ItemCarpet(int id) : base(id)
    {
        setMaxDamage(0);
        setHasSubtypes(true);
    }

    public override int getTextureId(int meta)
    {
        return Block.Carpet.getTexture(2, meta);
    }

    public override int getPlacementMetadata(int meta)
    {
        return meta;
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        return base.getItemName() + "." + ItemDye.DyeColorNames[BlockCloth.getBlockMeta(itemStack.getDamage())];
    }
}
