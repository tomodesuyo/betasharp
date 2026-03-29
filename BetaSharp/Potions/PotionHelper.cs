namespace BetaSharp.Potions;

public static class PotionHelper
{
    public const string SugarEffect = "-0+1-2-3&4-4+13";
    public const string GhastTearEffect = "+0-1-2-3&4-4+13";
    public const string SpiderEyeEffect = "-0-1+2-3&4-4+13";
    public const string FermentedSpiderEyeEffect = "-0+3-4+13";
    public const string SpeckledMelonEffect = "+0-1+2-3&4-4+13";
    public const string BlazePowderEffect = "+0-1-2+3&4-4+13";
    public const string MagmaCreamEffect = "+0+1-2-3&4-4+13";
    public const string RedstoneEffect = "-5+6-7";
    public const string GlowstoneEffect = "+5-6-7";
    public const string GunpowderEffect = "+14&13-13";

    private static readonly Dictionary<int, string> s_potionRequirements = new();
    private static readonly Dictionary<int, string> s_potionAmplifiers = new();
    private static readonly Dictionary<int, int> s_colorCache = new();

    static PotionHelper()
    {
        s_potionRequirements[Potion.Regeneration.Id] = "0 & !1 & !2 & !3 & 0+6";
        s_potionRequirements[Potion.MoveSpeed.Id] = "!0 & 1 & !2 & !3 & 1+6";
        s_potionRequirements[Potion.FireResistance.Id] = "0 & 1 & !2 & !3 & 0+6";
        s_potionRequirements[Potion.Heal.Id] = "0 & !1 & 2 & !3";
        s_potionRequirements[Potion.Poison.Id] = "!0 & !1 & 2 & !3 & 2+6";
        s_potionRequirements[Potion.Weakness.Id] = "!0 & !1 & !2 & 3 & 3+6";
        s_potionRequirements[Potion.Harm.Id] = "!0 & !1 & 2 & 3";
        s_potionRequirements[Potion.MoveSlowdown.Id] = "!0 & 1 & !2 & 3 & 3+6";
        s_potionRequirements[Potion.DamageBoost.Id] = "0 & !1 & !2 & 3 & 3+6";
        s_potionRequirements[Potion.NightVision.Id] = "!0 & 1 & 2 & !3 & 2+6";
        s_potionRequirements[Potion.Invisibility.Id] = "!0 & 1 & 2 & 3 & 2+6";
        s_potionRequirements[Potion.WaterBreathing.Id] = "0 & !1 & 2 & 3 & 2+6";
        s_potionRequirements[Potion.Jump.Id] = "0 & 1 & !2 & 3 & 3+6";

        s_potionAmplifiers[Potion.MoveSpeed.Id] = "5";
        s_potionAmplifiers[Potion.DigSpeed.Id] = "5";
        s_potionAmplifiers[Potion.DamageBoost.Id] = "5";
        s_potionAmplifiers[Potion.Regeneration.Id] = "5";
        s_potionAmplifiers[Potion.Harm.Id] = "5";
        s_potionAmplifiers[Potion.Heal.Id] = "5";
        s_potionAmplifiers[Potion.Resistance.Id] = "5";
        s_potionAmplifiers[Potion.Poison.Id] = "5";
        s_potionAmplifiers[Potion.Jump.Id] = "5";
    }

    public static bool CheckFlag(int value, int bit)
    {
        return (value & 1 << bit) != 0;
    }

    public static int ApplyIngredient(int potionData, string effect)
    {
        bool hasNumber = false;
        bool invert = false;
        bool remove = false;
        bool check = false;
        int bit = 0;

        for (int i = 0; i < effect.Length; ++i)
        {
            char c = effect[i];
            if (c >= '0' && c <= '9')
            {
                bit *= 10;
                bit += c - '0';
                hasNumber = true;
            }
            else if (c == '!')
            {
                if (hasNumber)
                {
                    potionData = ApplyBitOperation(potionData, bit, remove, invert, check);
                    hasNumber = false;
                    bit = 0;
                    remove = invert = check = false;
                }

                invert = true;
            }
            else if (c == '-')
            {
                if (hasNumber)
                {
                    potionData = ApplyBitOperation(potionData, bit, remove, invert, check);
                    hasNumber = false;
                    bit = 0;
                    invert = check = false;
                }

                remove = true;
            }
            else if (c == '+')
            {
                if (hasNumber)
                {
                    potionData = ApplyBitOperation(potionData, bit, remove, invert, check);
                    hasNumber = false;
                    bit = 0;
                    remove = invert = check = false;
                }
            }
            else if (c == '&')
            {
                if (hasNumber)
                {
                    potionData = ApplyBitOperation(potionData, bit, remove, invert, check);
                    hasNumber = false;
                    bit = 0;
                    remove = invert = false;
                }

                check = true;
            }
        }

        if (hasNumber)
        {
            potionData = ApplyBitOperation(potionData, bit, remove, invert, check);
        }

        return potionData & short.MaxValue;
    }

    public static List<PotionEffect>? GetPotionEffects(int potionData, bool includeUnusable)
    {
        List<PotionEffect>? effects = null;

        for (int i = 0; i < Potion.PotionTypes.Length; ++i)
        {
            Potion? potion = Potion.PotionTypes[i];
            if (potion == null || (potion.Unusable && !includeUnusable))
            {
                continue;
            }

            if (!s_potionRequirements.TryGetValue(potion.Id, out string? requirement))
            {
                continue;
            }

            int durationScale = EvaluateRequirement(requirement, 0, requirement.Length, potionData);
            if (durationScale <= 0)
            {
                continue;
            }

            int amplifier = 0;
            if (s_potionAmplifiers.TryGetValue(potion.Id, out string? amplifierRule))
            {
                amplifier = EvaluateRequirement(amplifierRule, 0, amplifierRule.Length, potionData);
                if (amplifier < 0)
                {
                    amplifier = 0;
                }
            }

            int duration;
            if (potion.IsInstant())
            {
                duration = 1;
            }
            else
            {
                duration = 1200 * (durationScale * 3 + (durationScale - 1) * 2);
                duration >>= amplifier;
                duration = (int)Math.Round(duration * potion.Effectiveness);
                if ((potionData & 16384) != 0)
                {
                    duration = (int)Math.Round(duration * 0.75D + 0.5D);
                }
            }

            effects ??= [];
            effects.Add(new PotionEffect(potion.Id, duration, amplifier));
        }

        return effects;
    }

    public static int GetColor(int potionData, bool includeUnusable)
    {
        if (includeUnusable)
        {
            return GetLiquidColor(GetPotionEffects(potionData, true));
        }

        if (!s_colorCache.TryGetValue(potionData, out int color))
        {
            color = GetLiquidColor(GetPotionEffects(potionData, false));
            s_colorCache[potionData] = color;
        }

        return color;
    }

    public static int GetLiquidColor(IEnumerable<PotionEffect>? effects)
    {
        const int defaultColor = 3694022;
        if (effects == null)
        {
            return defaultColor;
        }

        float red = 0.0F;
        float green = 0.0F;
        float blue = 0.0F;
        float total = 0.0F;

        foreach (PotionEffect effect in effects)
        {
            Potion potion = Potion.PotionTypes[effect.PotionId]!;
            int color = potion.LiquidColor;
            for (int i = 0; i <= effect.Amplifier; ++i)
            {
                red += (color >> 16 & 255) / 255.0F;
                green += (color >> 8 & 255) / 255.0F;
                blue += (color & 255) / 255.0F;
                total += 1.0F;
            }
        }

        if (total <= 0.0F)
        {
            return defaultColor;
        }

        int r = (int)(red / total * 255.0F);
        int g = (int)(green / total * 255.0F);
        int b = (int)(blue / total * 255.0F);
        return r << 16 | g << 8 | b;
    }

    private static int ApplyBitOperation(int potionData, int bit, bool remove, bool invert, bool check)
    {
        if (check)
        {
            return !CheckFlag(potionData, bit) ? 0 : potionData;
        }

        if (remove)
        {
            return potionData & ~(1 << bit);
        }

        if (invert)
        {
            return CheckFlag(potionData, bit) ? potionData & ~(1 << bit) : potionData | 1 << bit;
        }

        return potionData | 1 << bit;
    }

    private static int EvaluateRequirement(string expression, int start, int end, int flags)
    {
        if (start >= expression.Length || end < 0 || start >= end)
        {
            return 0;
        }

        int orIndex = expression.IndexOf('|', start);
        if (orIndex >= 0 && orIndex < end)
        {
            int left = EvaluateRequirement(expression, start, orIndex, flags);
            return left > 0 ? left : Math.Max(0, EvaluateRequirement(expression, orIndex + 1, end, flags));
        }

        int andIndex = expression.IndexOf('&', start);
        if (andIndex >= 0 && andIndex < end)
        {
            int left = EvaluateRequirement(expression, start, andIndex, flags);
            if (left <= 0)
            {
                return 0;
            }

            int right = EvaluateRequirement(expression, andIndex + 1, end, flags);
            return right <= 0 ? 0 : Math.Max(left, right);
        }

        bool multiplier = false;
        bool multiplierValueSet = false;
        bool negate = false;
        bool inverted = false;
        sbyte compareMode = -1;
        int flagIndex = 0;
        int multiplierValue = 0;
        int sum = 0;
        bool hasNumber = false;

        for (int i = start; i < end; ++i)
        {
            char c = expression[i];
            if (c >= '0' && c <= '9')
            {
                if (multiplier)
                {
                    multiplierValue = c - '0';
                    multiplierValueSet = true;
                }
                else
                {
                    flagIndex *= 10;
                    flagIndex += c - '0';
                    hasNumber = true;
                }
            }
            else if (c == '*')
            {
                multiplier = true;
            }
            else if (c == '!')
            {
                if (hasNumber)
                {
                    sum += EvaluateFlag(inverted, multiplierValueSet, negate, compareMode, flagIndex, multiplierValue, flags);
                    multiplier = multiplierValueSet = negate = inverted = hasNumber = false;
                    multiplierValue = 0;
                    flagIndex = 0;
                    compareMode = -1;
                }

                inverted = true;
            }
            else if (c == '-')
            {
                if (hasNumber)
                {
                    sum += EvaluateFlag(inverted, multiplierValueSet, negate, compareMode, flagIndex, multiplierValue, flags);
                    multiplier = multiplierValueSet = inverted = hasNumber = false;
                    multiplierValue = 0;
                    flagIndex = 0;
                    compareMode = -1;
                }

                negate = true;
            }
            else if (c == '+')
            {
                if (hasNumber)
                {
                    sum += EvaluateFlag(inverted, multiplierValueSet, negate, compareMode, flagIndex, multiplierValue, flags);
                    multiplier = multiplierValueSet = negate = inverted = hasNumber = false;
                    multiplierValue = 0;
                    flagIndex = 0;
                    compareMode = -1;
                }
            }
            else if (c == '=' || c == '<' || c == '>')
            {
                if (hasNumber)
                {
                    sum += EvaluateFlag(inverted, multiplierValueSet, negate, compareMode, flagIndex, multiplierValue, flags);
                    multiplier = multiplierValueSet = negate = inverted = hasNumber = false;
                    multiplierValue = 0;
                    flagIndex = 0;
                }

                compareMode = c == '=' ? (sbyte)0 : c == '>' ? (sbyte)1 : (sbyte)2;
            }
        }

        if (hasNumber)
        {
            sum += EvaluateFlag(inverted, multiplierValueSet, negate, compareMode, flagIndex, multiplierValue, flags);
        }

        return sum;
    }

    private static int EvaluateFlag(bool inverted, bool hasMultiplier, bool negate, int compareMode, int flagIndex, int multiplierValue, int flags)
    {
        int value;
        if (inverted)
        {
            value = CheckFlag(flags, flagIndex) ? 0 : 1;
        }
        else if (compareMode != -1)
        {
            int count = CountSetFlags(flags);
            value = compareMode switch
            {
                0 when count == flagIndex => 1,
                1 when count > flagIndex => 1,
                2 when count < flagIndex => 1,
                _ => 0
            };
        }
        else
        {
            value = CheckFlag(flags, flagIndex) ? 1 : 0;
        }

        if (hasMultiplier)
        {
            value *= multiplierValue;
        }

        if (negate)
        {
            value *= -1;
        }

        return value;
    }

    private static int CountSetFlags(int value)
    {
        int count = 0;
        while (value > 0)
        {
            value &= value - 1;
            ++count;
        }

        return count;
    }
}
