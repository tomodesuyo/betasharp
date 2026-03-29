namespace BetaSharp.Worlds.Generation.Structures;

internal class StructureStrongholdPieceWeight
{
    public Type PieceType { get; }
    public int PieceWeight { get; }
    public int InstancesSpawned { get; set; }
    public int InstancesLimit { get; }

    public StructureStrongholdPieceWeight(Type pieceType, int pieceWeight, int instancesLimit)
    {
        PieceType = pieceType;
        PieceWeight = pieceWeight;
        InstancesLimit = instancesLimit;
    }

    public virtual bool CanSpawnMoreStructuresOfType(int componentType)
    {
        return InstancesLimit == 0 || InstancesSpawned < InstancesLimit;
    }

    public bool CanSpawnMoreStructures()
    {
        return InstancesLimit == 0 || InstancesSpawned < InstancesLimit;
    }
}
