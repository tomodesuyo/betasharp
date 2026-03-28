namespace BetaSharp.Worlds.Generation.Structures;

public sealed class StructureVillagePieceWeight(Type pieceType, int weight, int limit)
{
    public Type PieceType { get; } = pieceType;
    public int VillagePieceWeight { get; } = weight;
    public int VillagePiecesLimit { get; } = limit;
    public int VillagePiecesSpawned { get; set; }

    public bool CanSpawnMoreVillagePieces() => VillagePiecesLimit == 0 || VillagePiecesSpawned < VillagePiecesLimit;

    public bool CanSpawnMoreVillagePiecesOfType(int componentType) =>
        CanSpawnMoreVillagePieces() && (VillagePiecesLimit == 0 || componentType + 1 < VillagePiecesLimit);
}
