namespace BetaSharp.Worlds.Generation.Biomes;

internal class BiomeGenDesert : Biome
{
    public BiomeGenDesert()
    {
        CreatureList.Clear();
        SetDecorator(treesPerChunk: -999, flowersPerChunk: 0, grassPerChunk: 0, deadBushPerChunk: 2, reedsPerChunk: 50, cactiPerChunk: 10);
    }
}
