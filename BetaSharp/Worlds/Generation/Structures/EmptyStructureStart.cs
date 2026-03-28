using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

public sealed class EmptyStructureStart : StructureStart
{
    public EmptyStructureStart()
    {
        BoundingBox = StructureBoundingBox.CreateUnknownBox();
    }

    public override bool IsSizeableStructure() => false;
}
