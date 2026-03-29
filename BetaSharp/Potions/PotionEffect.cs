using BetaSharp.Entities;

namespace BetaSharp.Potions;

public sealed class PotionEffect : IEquatable<PotionEffect>
{
    public int PotionId { get; private set; }
    public int Duration { get; private set; }
    public int Amplifier { get; private set; }

    public PotionEffect(int potionId, int duration, int amplifier)
    {
        PotionId = potionId;
        Duration = duration;
        Amplifier = amplifier;
    }

    public PotionEffect(PotionEffect other)
    {
        PotionId = other.PotionId;
        Duration = other.Duration;
        Amplifier = other.Amplifier;
    }

    public void Combine(PotionEffect other)
    {
        if (PotionId != other.PotionId)
        {
            throw new ArgumentException("Potion effects must match", nameof(other));
        }

        if (other.Amplifier > Amplifier)
        {
            Amplifier = other.Amplifier;
            Duration = other.Duration;
        }
        else if (other.Amplifier == Amplifier && Duration < other.Duration)
        {
            Duration = other.Duration;
        }
    }

    public bool OnUpdate(EntityLiving entity)
    {
        if (Duration > 0)
        {
            if (Potion.PotionTypes[PotionId]!.IsReady(Duration, Amplifier))
            {
                PerformEffect(entity);
            }

            --Duration;
        }

        return Duration > 0;
    }

    public void PerformEffect(EntityLiving entity)
    {
        if (Duration > 0)
        {
            Potion.PotionTypes[PotionId]!.PerformEffect(entity, Amplifier);
        }
    }

    public bool Equals(PotionEffect? other)
    {
        return other != null && PotionId == other.PotionId && Duration == other.Duration && Amplifier == other.Amplifier;
    }

    public override bool Equals(object? obj)
    {
        return obj is PotionEffect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PotionId, Duration, Amplifier);
    }
}
