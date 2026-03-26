using BetaSharp.Entities;
using BetaSharp.Network.Packets.S2CPlay;
using BetaSharp.Server.Command;
using BetaSharp.Server.Internal;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Server.Commands;

public class GameModeCommand : ICommand
{
    public string Usage => "gamemode <survival|creative|spectator|0|1|3> [player]";
    public string Description => "Change a player's game mode";
    public string[] Names => ["gamemode", "gm"];

    public void Execute(ICommand.CommandContext c)
    {
        if (c.Args.Length is < 1 or > 2)
        {
            c.Output.SendMessage("Usage: gamemode <survival|creative|spectator|0|1|3> [player]");
            return;
        }

        int gameMode = c.Args[0].ToLowerInvariant() switch
        {
            "0" or "s" or "survival" => GameMode.Survival,
            "1" or "c" or "creative" => GameMode.Creative,
            "3" or "sp" or "spectator" => GameMode.Spectator,
            _ => -1
        };
        if (gameMode < 0)
        {
            c.Output.SendMessage("Unknown game mode. Use survival, creative, spectator, 0, 1, or 3.");
            return;
        }

        string modeName = GameMode.GetName(gameMode);

        ServerPlayerEntity? target = c.Args.Length == 2
            ? c.Server.playerManager.getPlayer(c.Args[1])
            : c.Server.playerManager.getPlayer(c.SenderName);

        if (target == null)
        {
            c.Output.SendMessage(c.Args.Length == 2
                ? "Can't find user " + c.Args[1] + "."
                : "Could not find your player.");
            return;
        }

        target.capabilities.SetGameMode(gameMode);
        if (GameMode.IsSpectator(gameMode))
        {
            target.ResetMotionForSpectatorTransition();
        }

        target.networkHandler.sendPacket(PlayerCapabilitiesS2CPacket.Get(target.capabilities));

        if (c.Server is InternalServer internalServer)
        {
            for (int i = 0; i < c.Server.worlds.Length; ++i)
            {
                if (c.Server.worlds[i] != null)
                {
                    c.Server.worlds[i].Properties.GameType = gameMode;
                }
            }

            if (internalServer.config is InternalServerConfiguration configuration)
            {
                configuration.SetGameMode(gameMode);
            }
        }

        if (!target.name.Equals(c.SenderName, StringComparison.OrdinalIgnoreCase))
        {
            c.LogOp("Set " + target.name + "'s game mode to " + modeName + ".");
            c.Output.SendMessage("Set " + target.name + "'s game mode to " + modeName + ".");
        }
        else
        {
            c.LogOp("Set own game mode to " + modeName + ".");
            c.Output.SendMessage("Your game mode has been updated to " + modeName + ".");
        }
    }
}
