namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class StructureStrongholdPieceWeight2 : StructureStrongholdPieceWeight
{
    public StructureStrongholdPieceWeight2(Type pieceType, int pieceWeight, int instancesLimit) : base(pieceType, pieceWeight, instancesLimit)
    {
    }

    public override bool CanSpawnMoreStructuresOfType(int componentType) => base.CanSpawnMoreStructuresOfType(componentType) && componentType > 4;
}
