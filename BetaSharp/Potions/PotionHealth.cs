namespace BetaSharp.Potions;

public sealed class PotionHealth : Potion
{
    public PotionHealth(int id, bool isBadEffect, int liquidColor) : base(id, isBadEffect, liquidColor)
    {
    }

    public override bool IsInstant() => true;

    public override bool IsReady(int duration, int amplifier) => duration >= 1;
}
