using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal class BlockMushroomCap : Block
{
    private readonly int _capType;

    public BlockMushroomCap(int id, Material material, int textureId, int capType) : base(id, textureId, material)
    {
        _capType = capType;
    }

    public override int getTexture(int side, int meta)
    {
        return _capType == 0 ? textureId : textureId + 1;
    }
}
