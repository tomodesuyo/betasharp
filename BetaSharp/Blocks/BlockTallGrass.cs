using BetaSharp.Items;
using BetaSharp.Worlds.ClientData.Colors;
using BetaSharp.Worlds.Colors;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

public class BlockTallGrass : BlockPlant
{
    public BlockTallGrass(int i, int j) : base(i, j)
    {
        float halfSize = 0.4F;
        setBoundingBox(0.5F - halfSize, 0.0F, 0.5F - halfSize, 0.5F + halfSize, 0.8F, 0.5F + halfSize);
    }

    public override int getTexture(int side, int meta) => meta == 1 ? textureId : meta == 2 ? textureId + 16 + 1 : meta == 0 ? textureId + 16 : textureId;

    public override int getColor(int meta) => meta == 0 ? 0xFFFFFF : FoliageColors.getDefaultColor();

    public override int getColorMultiplier(IBlockReader iBlockReader, int x, int y, int z)
    {
        int meta = iBlockReader.GetBlockMeta(x, y, z);
        if (meta == 0) return 0xFFFFFF;

        return iBlockReader.GetBiomeSource().GetBiome(x, z).GetGrassColorAtCoords(iBlockReader, x, y, z);
    }

    public override int getColorMultiplier(IBlockReader iBlockReader, int x, int y, int z, int knownMeta)
    {
        if (knownMeta == 0) return 0xFFFFFF;
        return iBlockReader.GetBiomeSource().GetBiome(x, z).GetGrassColorAtCoords(iBlockReader, x, y, z);
    }

    public override int getDroppedItemId(int blockMeta) => Random.Shared.Next(8) == 0 ? Item.Seeds.id : -1;
}
