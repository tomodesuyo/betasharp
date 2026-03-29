using BetaSharp.Entities;
using BetaSharp.Potions;
using BetaSharp.Server.Command;

namespace BetaSharp.Server.Commands;

public sealed class EffectCommand : ICommand
{
    public string Usage => "effect <player> <effect|clear> [seconds] [amplifier]";
    public string Description => "Applies or clears potion effects";
    public string[] Names => ["effect"];

    public void Execute(ICommand.CommandContext c)
    {
        if (c.Args.Length < 2)
        {
            c.Output.SendMessage($"Usage: {Usage}");
            return;
        }

        ServerPlayerEntity? player = c.Server.playerManager.getPlayer(c.Args[0]);
        if (player == null)
        {
            c.Output.SendMessage("Player not found.");
            return;
        }

        if (c.Args[1].Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            if (!player.getActivePotionEffects().Any())
            {
                c.Output.SendMessage($"{player.name} has no active effects.");
                return;
            }

            player.clearPotionEffects();
            c.Output.SendMessage($"Cleared all effects from {player.name}.");
            return;
        }

        Potion? potion = ResolvePotion(c.Args[1]);
        if (potion == null)
        {
            c.Output.SendMessage($"Unknown effect: {c.Args[1]}");
            return;
        }

        int durationSeconds = potion.IsInstant() ? 1 : 30;
        if (c.Args.Length >= 3 && !int.TryParse(c.Args[2], out durationSeconds))
        {
            c.Output.SendMessage("Duration must be an integer number of seconds.");
            return;
        }

        int amplifier = 0;
        if (c.Args.Length >= 4 && !int.TryParse(c.Args[3], out amplifier))
        {
            c.Output.SendMessage("Amplifier must be an integer.");
            return;
        }

        if (durationSeconds <= 0)
        {
            if (player.removePotionEffect(potion))
            {
                c.Output.SendMessage($"Removed {GetPotionDisplayName(potion)} from {player.name}.");
            }
            else
            {
                c.Output.SendMessage($"{player.name} does not have {GetPotionDisplayName(potion)}.");
            }

            return;
        }

        int duration = potion.IsInstant() ? Math.Max(1, durationSeconds) : durationSeconds * 20;
        player.addPotionEffect(new PotionEffect(potion.Id, duration, Math.Max(0, amplifier)));
        c.Output.SendMessage($"Applied {GetPotionDisplayName(potion)} {(amplifier + 1)} to {player.name} for {durationSeconds}s.");
    }

    private static Potion? ResolvePotion(string arg)
    {
        if (int.TryParse(arg, out int id) && id >= 0 && id < Potion.PotionTypes.Length)
        {
            return Potion.PotionTypes[id];
        }

        string normalized = arg.Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .ToLowerInvariant();

        return normalized switch
        {
            "speed" or "swiftness" or "movespeed" => Potion.MoveSpeed,
            "slowness" or "moveslowdown" => Potion.MoveSlowdown,
            "haste" or "digspeed" => Potion.DigSpeed,
            "miningfatigue" or "digslowdown" => Potion.DigSlowdown,
            "strength" or "damageboost" => Potion.DamageBoost,
            "instanthealth" or "heal" => Potion.Heal,
            "instantdamage" or "harm" => Potion.Harm,
            "jump" or "jumpboost" => Potion.Jump,
            "nausea" or "confusion" => Potion.Confusion,
            "regeneration" => Potion.Regeneration,
            "resistance" => Potion.Resistance,
            "fireresistance" => Potion.FireResistance,
            "waterbreathing" => Potion.WaterBreathing,
            "invisibility" => Potion.Invisibility,
            "blindness" => Potion.Blindness,
            "nightvision" => Potion.NightVision,
            "hunger" => Potion.Hunger,
            "weakness" => Potion.Weakness,
            "poison" => Potion.Poison,
            _ => null
        };
    }

    private static string GetPotionDisplayName(Potion potion)
    {
        return TranslationStorage.Instance.TranslateKey(potion.Name + ".name");
    }
}
