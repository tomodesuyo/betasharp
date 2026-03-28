using BetaSharp.Blocks.Materials;
using BetaSharp.Worlds.Core.Systems;

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
        int capTexture = textureId - 16 - _capType;
        return meta == 10 && side > 1
            ? textureId - 1
            : meta >= 1 && meta <= 9 && side == 1
                ? capTexture
                : meta >= 1 && meta <= 3 && side == 2
                    ? capTexture
                    : meta >= 7 && meta <= 9 && side == 3
                        ? capTexture
                        : (meta == 1 || meta == 4 || meta == 7) && side == 4
                            ? capTexture
                            : (meta == 3 || meta == 6 || meta == 9) && side == 5
                                ? capTexture
                                : meta == 14
                                    ? capTexture
                                    : meta == 15
                                        ? textureId - 1
                                        : textureId;
    }

    public override int getDroppedItemCount()
    {
        int count = Random.Shared.Next(10) - 7;
        return count < 0 ? 0 : count;
    }

    public override int getDroppedItemId(int blockMeta) => Block.BrownMushroom.id + _capType;
}
