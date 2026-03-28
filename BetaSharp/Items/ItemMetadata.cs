using BetaSharp.Blocks;

namespace BetaSharp.Items;

internal class ItemMetadata : ItemBlock
{
    private readonly Block _block;

    public ItemMetadata(int id, Block block) : base(id)
    {
        _block = block;
        setMaxDamage(0);
        setHasSubtypes(true);
    }

    public override int getTextureId(int meta)
    {
        return _block.getTexture(2, meta);
    }

    public override int getPlacementMetadata(int meta)
    {
        return meta;
    }
}
