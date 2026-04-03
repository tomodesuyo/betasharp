using BetaSharp.Blocks;

namespace BetaSharp.Items;

internal sealed class ItemWall : ItemBlock
{
    private static readonly string[] s_names =
    [
        "normal",
        "mossy"
    ];

    public ItemWall(int id) : base(id)
    {
        setMaxDamage(0);
        setHasSubtypes(true);
    }

    public override int getTextureId(int meta)
    {
        return Block.Wall.getTexture(2, meta);
    }

    public override int getPlacementMetadata(int meta)
    {
        return meta & 1;
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        int meta = itemStack.getDamage() & 1;
        return Block.Wall.getBlockName() + "." + s_names[meta];
    }
}
