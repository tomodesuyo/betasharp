using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class StructureStrongholdStart : StructureStart
{
    public StructureStrongholdStart(IWorldContext world, JavaRandom random, int chunkX, int chunkZ)
    {
        ComponentStrongholdPortalRoom? portalRoom = ComponentStrongholdPortalRoom.Create((chunkX << 4) + 2, 20, (chunkZ << 4) + 2, random.NextInt(4), 0, Components);
        if (portalRoom != null)
        {
            Components.Add(portalRoom);
            UpdateBoundingBox();
            MarkAvailableHeight(world, random, 10);
        }
        else
        {
            BoundingBox = StructureBoundingBox.CreateUnknownBox();
        }
    }

    public override bool IsSizeableStructure() => Components.Count > 0;
}
