namespace BetaSharp.Worlds.Core.Systems;

public class WorldSettings
{
    public WorldSettings(long seed, WorldType terrainType, string generatorOptions, int gameType)
    {
        Seed = seed;
        TerrainType = terrainType;
        GeneratorOptions = generatorOptions;
        GameType = gameType;
    }

    public WorldSettings(long seed, WorldType terrainType, string generatorOptions = "", bool isCreativeMode = false)
        : this(seed, terrainType, generatorOptions, isCreativeMode ? GameMode.Creative : GameMode.Survival)
    {
    }

    public long Seed { get; }
    public WorldType TerrainType { get; }
    public string GeneratorOptions { get; }
    public int GameType { get; }
    public bool IsCreativeMode => GameMode.IsCreative(GameType);
    public bool IsSpectatorMode => GameMode.IsSpectator(GameType);
}
