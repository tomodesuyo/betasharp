using BetaSharp.Items;

namespace BetaSharp.Blocks;

internal sealed class BlockCarrot : BlockCrops
{
    public BlockCarrot(int id, int textureId) : base(id, textureId)
    {
    }

    protected override int getSeedItemId()
    {
        return Item.Carrot.id;
    }

    protected override int getCropItemId()
    {
        return Item.Carrot.id;
    }

    public override int getTexture(int side, int meta)
    {
        if (meta < 7)
        {
            if (meta == 6)
            {
                meta = 5;
            }

            return textureId + (meta >> 1);
        }

        return textureId + 3;
    }
}
