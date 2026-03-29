using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

public abstract class StructurePieceBlockSelector
{
    public int SelectedBlockId { get; protected set; }
    public int SelectedBlockMeta { get; protected set; }

    public abstract void SelectBlocks(JavaRandom random, int x, int y, int z, bool wall);
}
