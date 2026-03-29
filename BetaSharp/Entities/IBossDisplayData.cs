namespace BetaSharp.Entities;

public interface IBossDisplayData
{
    int BossHealth { get; }
    int MaxBossHealth { get; }
    string BossName { get; }
}
