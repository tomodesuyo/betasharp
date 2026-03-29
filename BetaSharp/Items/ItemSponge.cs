using BetaSharp.Blocks;

namespace BetaSharp.Items;

internal sealed class ItemSponge : ItemBlock
{
    public ItemSponge(int id) : base(id)
    {
        setMaxDamage(0);
        setHasSubtypes(true);
    }

    public override int getTextureId(int meta)
    {
        return Block.Sponge.getTexture(2, meta);
    }

    public override int getPlacementMetadata(int meta)
    {
        return meta & 1;
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        return base.getItemName() + ((itemStack.getDamage() & 1) == 1 ? ".wet" : ".dry");
    }
}
