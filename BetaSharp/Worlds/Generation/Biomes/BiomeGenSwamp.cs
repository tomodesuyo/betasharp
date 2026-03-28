using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Generators.Features;

namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeGenSwamp : Biome
{
    public BiomeGenSwamp()
    {
        SetDecorator(treesPerChunk: 2, flowersPerChunk: -999, grassPerChunk: 1, deadBushPerChunk: 1, mushroomsPerChunk: 8, reedsPerChunk: 10, clayPerChunk: 1, waterLiliesPerChunk: 4);
        SetWaterColor(14745518);
    }

    public override Feature GetRandomWorldGenForTrees(JavaRandom rand) => new SwampTreeFeature();

    public override int GetGrassColorAtCoords(IBlockReader reader, int x, int y, int z)
    {
        return ((base.GetGrassColorAtCoords(reader, x, y, z) & 0xFEFEFE) + 0x4E0E4E) / 2;
    }

    public override int GetFoliageColorAtCoords(IBlockReader reader, int x, int y, int z)
    {
        return ((base.GetFoliageColorAtCoords(reader, x, y, z) & 0xFEFEFE) + 0x4E0E4E) / 2;
    }
}
