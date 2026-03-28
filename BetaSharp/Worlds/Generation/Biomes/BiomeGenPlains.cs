namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeGenPlains : Biome
{
    public BiomeGenPlains()
    {
        SetDecorator(treesPerChunk: -999, flowersPerChunk: 4, grassPerChunk: 10);
    }
}
