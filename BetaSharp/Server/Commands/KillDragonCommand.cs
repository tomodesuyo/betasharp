using BetaSharp.Entities;
using BetaSharp.Server.Command;
using BetaSharp.Util.Maths;

namespace BetaSharp.Server.Commands;

public sealed class KillDragonCommand : ICommand
{
    public string Usage => "killdragon";
    public string Description => "Triggers the Ender Dragon death sequence for testing";
    public string[] Names => ["killdragon", "testkilldragon"];

    public void Execute(ICommand.CommandContext c)
    {
        ServerPlayerEntity? player = c.Server.playerManager.getPlayer(c.SenderName);
        List<EntityDragon> dragons = [];

        if (player != null)
        {
            dragons.AddRange(c.Server.getWorld(player.dimensionId).Entities.CollectEntitiesOfType<EntityDragon>(new Box(-30000000, 0.0D, -30000000, 30000000, 256.0D, 30000000)));
        }

        if (dragons.Count == 0)
        {
            for (int i = 0; i < c.Server.worlds.Length; ++i)
            {
                dragons.AddRange(c.Server.worlds[i].Entities.CollectEntitiesOfType<EntityDragon>(new Box(-30000000, 0.0D, -30000000, 30000000, 256.0D, 30000000)));
            }
        }

        int triggered = 0;
        for (int i = 0; i < dragons.Count; ++i)
        {
            EntityDragon dragon = dragons[i];
            if (dragon.dead || dragon.DeathTicks > 0)
            {
                continue;
            }

            dragon.ForceKillForTesting();
            ++triggered;
        }

        c.Output.SendMessage(triggered > 0
            ? $"Killed {triggered} dragon(s) and created the exit portal immediately."
            : "No living Ender Dragon was found.");
    }
}
