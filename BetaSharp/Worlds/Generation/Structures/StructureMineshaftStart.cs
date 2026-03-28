using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class StructureMineshaftStart : StructureStart
{
    public StructureMineshaftStart(IWorldContext world, JavaRandom random, int chunkX, int chunkZ)
    {
        ComponentMineshaftRoom room = new(0, random, (chunkX << 4) + 2, (chunkZ << 4) + 2);
        Components.Add(room);
        room.BuildComponent(room, Components, random);
        UpdateBoundingBox();
        MarkAvailableHeight(world, random, 10);
    }
}
