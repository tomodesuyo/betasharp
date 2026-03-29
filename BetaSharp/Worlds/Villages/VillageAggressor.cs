using BetaSharp.Entities;

namespace BetaSharp.Worlds.Villages;

internal sealed class VillageAggressor(EntityLiving aggressor, int aggressionTime)
{
    public EntityLiving Aggressor { get; } = aggressor;
    public int AggressionTime { get; set; } = aggressionTime;
}
