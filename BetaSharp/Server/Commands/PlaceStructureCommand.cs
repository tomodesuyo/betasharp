using BetaSharp.Entities;
using BetaSharp.Server.Command;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Generation.Structures;

namespace BetaSharp.Server.Commands;

public sealed class PlaceStructureCommand : ICommand
{
    public string Usage => "place structure <village|stronghold|mineshaft|fortress> [x] [z]";
    public string Description => "Generates a structure at your current chunk or the specified coordinates";
    public string[] Names => ["place"];

    public void Execute(ICommand.CommandContext c)
    {
        if (c.Args.Length < 2 || !c.Args[0].Equals("structure", StringComparison.OrdinalIgnoreCase))
        {
            c.Output.SendMessage("Usage: place structure <village|stronghold|mineshaft|fortress> [x] [z]");
            return;
        }

        string? structureId = LocateCommand.NormalizeStructureId(c.Args[1]);
        if (structureId == null)
        {
            c.Output.SendMessage($"Unknown structure: {c.Args[1]}");
            return;
        }

        ServerPlayerEntity? player = c.Server.playerManager.getPlayer(c.SenderName);
        if (player == null)
        {
            c.Output.SendMessage("Could not find your player.");
            return;
        }

        int x = MathHelper.Floor(player.x);
        int z = MathHelper.Floor(player.z);
        if (c.Args.Length >= 4)
        {
            if (!int.TryParse(c.Args[2], out x) || !int.TryParse(c.Args[3], out z))
            {
                c.Output.SendMessage("Coordinates must be integers.");
                return;
            }
        }
        else if (c.Args.Length == 3)
        {
            c.Output.SendMessage("Usage: place structure <village|stronghold|mineshaft|fortress> [x] [z]");
            return;
        }

        var world = c.Server.getWorld(player.dimensionId);
        int chunkX = x >> 4;
        int chunkZ = z >> 4;
        JavaRandom random = new(((long)chunkX * 341873128712L + (long)chunkZ * 132897987541L) ^ world.Seed);

        StructureStart start = structureId switch
        {
            "Village" => new StructureVillageStart(world, random, chunkX, chunkZ, 0),
            "Stronghold" => new StructureStrongholdStart(world, random, chunkX, chunkZ),
            "Mineshaft" => new StructureMineshaftStart(world, random, chunkX, chunkZ),
            "Fortress" => new StructureNetherFortressStart(world, random, chunkX, chunkZ),
            _ => throw new InvalidOperationException($"Unsupported structure id: {structureId}")
        };

        if (!start.IsSizeableStructure() || start.GetComponents().Count == 0)
        {
            c.Output.SendMessage($"Could not build a {structureId.ToLowerInvariant()} at chunk {chunkX}, {chunkZ}.");
            return;
        }

        StructureBoundingBox bounds = new(start.GetBoundingBox());
        start.GenerateStructure(world, random, bounds);
        c.Output.SendMessage(
            $"Placed {structureId.ToLowerInvariant()} at chunk {chunkX}, {chunkZ} (X={bounds.MinX}..{bounds.MaxX}, Y={bounds.MinY}..{bounds.MaxY}, Z={bounds.MinZ}..{bounds.MaxZ}).");
    }
}
