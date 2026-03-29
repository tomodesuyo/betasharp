using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class ComponentStrongholdStairs2 : ComponentStrongholdStairs
{
    public StructureStrongholdPieceWeight? CurrentPieceWeight { get; set; }
    public ComponentStrongholdPortalRoom? PortalRoom { get; set; }
    public List<StructureComponent> PendingComponents { get; } = [];

    public ComponentStrongholdStairs2(int componentType, JavaRandom random, int x, int z) : base(componentType, random, x, z)
    {
    }
}
