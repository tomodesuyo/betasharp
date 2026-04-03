using System;
using BetaSharp.Entities;
using BetaSharp.Server.Command;
using BetaSharp.Util.Maths;

namespace BetaSharp.Server.Commands;

public class LocateCommand : ICommand
{
    public string Usage => "locate structure <village|stronghold|mineshaft|fortress>";
    public string Description => "Finds the nearest generated structure";
    public string[] Names => ["locate"];
    public byte PermissionLevel => 0;

    public void Execute(ICommand.CommandContext c)
    {
        if (c.Args.Length == 0)
        {
            c.Output.SendMessage("Usage: locate structure <village|stronghold|mineshaft|fortress>");
            return;
        }

        string structureArg;
        if (c.Args.Length >= 2 && c.Args[0].Equals("structure", StringComparison.OrdinalIgnoreCase))
        {
            structureArg = c.Args[1];
        }
        else if (c.Args.Length == 1 && c.Args[0].Equals("structure", StringComparison.OrdinalIgnoreCase))
        {
            c.Output.SendMessage("Usage: locate structure <village|stronghold|mineshaft|fortress>");
            return;
        }
        else
        {
            structureArg = c.Args[0];
        }

        string? structureId = NormalizeStructureId(structureArg);
        if (structureId == null)
        {
            c.Output.SendMessage($"Unknown structure: {structureArg}");
            return;
        }

        ServerPlayerEntity? player = c.Server.playerManager.getPlayer(c.SenderName);
        if (player == null)
        {
            c.Output.SendMessage("Could not find your player.");
            return;
        }

        Vec3i? pos = c.Server.getWorld(player.dimensionId).ChunkCache.FindNearestStructure(
            structureId,
            MathHelper.Floor(player.x),
            MathHelper.Floor(player.y),
            MathHelper.Floor(player.z));

        if (pos == null)
        {
            c.Output.SendMessage($"Could not find a nearby {structureId.ToLowerInvariant()}.");
            return;
        }

        c.Output.SendMessage($"Nearest {structureId.ToLowerInvariant()}: X={pos.Value.X}, Y={pos.Value.Y}, Z={pos.Value.Z}");
    }

    internal static string? NormalizeStructureId(string arg)
    {
        return arg.ToLowerInvariant() switch
        {
            "village" => "Village",
            "stronghold" => "Stronghold",
            "mineshaft" => "Mineshaft",
            "shaft" => "Mineshaft",
            "fortress" => "Fortress",
            "netherfortress" => "Fortress",
            "nether_fortress" => "Fortress",
            _ => null,
        };
    }
}
