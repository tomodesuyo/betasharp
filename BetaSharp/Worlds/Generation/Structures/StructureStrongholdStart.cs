using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class StructureStrongholdStart : StructureStart
{
    private readonly ComponentStrongholdStairs2 _startPiece;

    public StructureStrongholdStart(IWorldContext world, JavaRandom random, int chunkX, int chunkZ)
    {
        StructureStrongholdPieces.PrepareStructurePieces();
        _startPiece = new ComponentStrongholdStairs2(0, random, (chunkX << 4) + 2, (chunkZ << 4) + 2);
        Components.Add(_startPiece);
        _startPiece.BuildComponent(_startPiece, Components, random);

        while (_startPiece.PendingComponents.Count > 0)
        {
            int index = random.NextInt(_startPiece.PendingComponents.Count);
            StructureComponent next = _startPiece.PendingComponents[index];
            _startPiece.PendingComponents.RemoveAt(index);
            next.BuildComponent(_startPiece, Components, random);
        }

        if (_startPiece.PortalRoom != null)
        {
            UpdateBoundingBox();
            MarkAvailableHeight(world, random, 10);
        }
        else
        {
            BoundingBox = StructureBoundingBox.CreateUnknownBox();
        }
    }

    public override bool IsSizeableStructure() => _startPiece.PortalRoom != null;
}
