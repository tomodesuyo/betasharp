using BetaSharp.Entities;

namespace BetaSharp.Potions;

public class Potion
{
    public static readonly Potion?[] PotionTypes = new Potion?[32];

    public static readonly Potion MoveSpeed = new Potion(1, false, 8171462).SetPotionName("potion.moveSpeed");
    public static readonly Potion MoveSlowdown = new Potion(2, true, 5926017).SetPotionName("potion.moveSlowdown");
    public static readonly Potion DigSpeed = new Potion(3, false, 14270531).SetPotionName("potion.digSpeed").SetEffectiveness(1.5D);
    public static readonly Potion DigSlowdown = new Potion(4, true, 4866583).SetPotionName("potion.digSlowDown");
    public static readonly Potion DamageBoost = new Potion(5, false, 9643043).SetPotionName("potion.damageBoost");
    public static readonly Potion Heal = new PotionHealth(6, false, 16262179).SetPotionName("potion.heal");
    public static readonly Potion Harm = new PotionHealth(7, true, 4393481).SetPotionName("potion.harm");
    public static readonly Potion Jump = new Potion(8, false, 7889559).SetPotionName("potion.jump");
    public static readonly Potion Confusion = new Potion(9, true, 5578058).SetPotionName("potion.confusion").SetEffectiveness(0.25D);
    public static readonly Potion Regeneration = new Potion(10, false, 13458603).SetPotionName("potion.regeneration").SetEffectiveness(0.25D);
    public static readonly Potion Resistance = new Potion(11, false, 10044730).SetPotionName("potion.resistance");
    public static readonly Potion FireResistance = new Potion(12, false, 14981690).SetPotionName("potion.fireResistance");
    public static readonly Potion WaterBreathing = new Potion(13, false, 3035801).SetPotionName("potion.waterBreathing");
    public static readonly Potion Invisibility = new Potion(14, false, 8356754).SetPotionName("potion.invisibility");
    public static readonly Potion Blindness = new Potion(15, true, 2039587).SetPotionName("potion.blindness").SetEffectiveness(0.25D);
    public static readonly Potion NightVision = new Potion(16, false, 2039713).SetPotionName("potion.nightVision");
    public static readonly Potion Hunger = new Potion(17, true, 5797459).SetPotionName("potion.hunger");
    public static readonly Potion Weakness = new Potion(18, true, 4738376).SetPotionName("potion.weakness");
    public static readonly Potion Poison = new Potion(19, true, 5149489).SetPotionName("potion.poison").SetEffectiveness(0.25D);

    public int Id { get; }
    public bool IsBadEffect { get; }
    public int LiquidColor { get; }
    public string Name { get; private set; } = string.Empty;
    public bool Unusable { get; private set; }
    public double Effectiveness { get; private set; }

    protected Potion(int id, bool isBadEffect, int liquidColor)
    {
        Id = id;
        IsBadEffect = isBadEffect;
        LiquidColor = liquidColor;
        Effectiveness = isBadEffect ? 0.5D : 1.0D;
        PotionTypes[id] = this;
    }

    public virtual void PerformEffect(EntityLiving entity, int amplifier)
    {
        if (Id == Regeneration.Id)
        {
            if (entity.health < entity.maxHealth)
            {
                entity.heal(1);
            }
        }
        else if (Id == Poison.Id)
        {
            if (entity.health > 1)
            {
                entity.damage(null, 1);
            }
        }
        else if ((Id != Heal.Id || entity.isUndead()) && (Id != Harm.Id || !entity.isUndead()))
        {
            if ((Id == Harm.Id && !entity.isUndead()) || (Id == Heal.Id && entity.isUndead()))
            {
                entity.damage(null, 6 << amplifier);
            }
        }
        else
        {
            entity.heal(6 << amplifier);
        }
    }

    public virtual void AffectEntity(EntityLiving? source, EntityLiving target, int amplifier, double distanceScale)
    {
        if ((Id != Heal.Id || target.isUndead()) && (Id != Harm.Id || !target.isUndead()))
        {
            if ((Id == Harm.Id && !target.isUndead()) || (Id == Heal.Id && target.isUndead()))
            {
                int amount = (int)(distanceScale * (6 << amplifier) + 0.5D);
                target.damage(source, amount);
            }
        }
        else
        {
            int amount = (int)(distanceScale * (6 << amplifier) + 0.5D);
            target.heal(amount);
        }
    }

    public virtual bool IsInstant() => false;

    public virtual bool IsReady(int duration, int amplifier)
    {
        if (Id == Regeneration.Id || Id == Poison.Id)
        {
            int interval = 25 >> amplifier;
            return interval <= 0 || duration % interval == 0;
        }

        return Id == Hunger.Id;
    }

    public Potion SetPotionName(string name)
    {
        Name = name;
        return this;
    }

    public Potion SetEffectiveness(double effectiveness)
    {
        Effectiveness = effectiveness;
        return this;
    }

    public Potion SetPotionUnusable()
    {
        Unusable = true;
        return this;
    }

    public static string GetDurationString(PotionEffect effect)
    {
        int totalSeconds = effect.Duration / 20;
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return seconds < 10 ? $"{minutes}:0{seconds}" : $"{minutes}:{seconds}";
    }
}
