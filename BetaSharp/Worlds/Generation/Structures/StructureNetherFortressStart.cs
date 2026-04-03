using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class StructureNetherFortressStart : StructureStart
{
    public StructureNetherFortressStart(IWorldContext world, JavaRandom random, int chunkX, int chunkZ)
    {
        StructureNetherFortressPieces.Start start = new(random, (chunkX << 4) + 2, (chunkZ << 4) + 2);
        Components.Add(start);
        start.BuildComponent(start, Components, random);

        while (start.PendingComponents.Count > 0)
        {
            int index = random.NextInt(start.PendingComponents.Count);
            StructureComponent component = start.PendingComponents[index];
            start.PendingComponents.RemoveAt(index);
            component.BuildComponent(start, Components, random);
        }

        UpdateBoundingBox();
        SetRandomHeight(random, 48, 70);
    }
}
