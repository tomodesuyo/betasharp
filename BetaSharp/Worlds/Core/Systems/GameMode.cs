namespace BetaSharp.Worlds.Core.Systems;

public static class GameMode
{
    public const int Survival = 0;
    public const int Creative = 1;
    public const int Adventure = 2;
    public const int Spectator = 3;

    public static bool IsCreative(int gameMode) => gameMode == Creative;

    public static bool IsSpectator(int gameMode) => gameMode == Spectator;

    public static bool IsAdventure(int gameMode) => gameMode is Adventure or Spectator;

    public static bool IsSurvivalOrAdventure(int gameMode) => gameMode is Survival or Adventure;

    public static string GetName(int gameMode) =>
        gameMode switch
        {
            Creative => "creative",
            Adventure => "adventure",
            Spectator => "spectator",
            _ => "survival"
        };

    public static int Parse(string nameOrId)
    {
        return nameOrId.ToLowerInvariant() switch
        {
            "0" or "s" or "survival" => Survival,
            "1" or "c" or "creative" => Creative,
            "2" or "a" or "adventure" => Adventure,
            "3" or "sp" or "spectator" => Spectator,
            _ => Survival
        };
    }
}
