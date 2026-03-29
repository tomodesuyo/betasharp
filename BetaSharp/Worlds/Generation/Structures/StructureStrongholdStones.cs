using BetaSharp.Blocks;
using BetaSharp.Util.Maths;

namespace BetaSharp.Worlds.Generation.Structures;

internal sealed class StructureStrongholdStones : StructurePieceBlockSelector
{
    public override void SelectBlocks(JavaRandom random, int x, int y, int z, bool wall)
    {
        if (!wall)
        {
            SelectedBlockId = 0;
            SelectedBlockMeta = 0;
            return;
        }

        SelectedBlockId = Block.StoneBrick.id;
        float chance = random.NextFloat();
        if (chance < 0.2F)
        {
            SelectedBlockMeta = 2;
        }
        else if (chance < 0.5F)
        {
            SelectedBlockMeta = 1;
        }
        else if (chance < 0.55F)
        {
            SelectedBlockId = Block.Silverfish.id;
            SelectedBlockMeta = 2;
        }
        else
        {
            SelectedBlockMeta = 0;
        }
    }
}
