using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockRedstoneBlock : BlockOreStorage
{
    public BlockRedstoneBlock(int id, int textureId) : base(id, textureId)
    {
    }

    public override bool canEmitRedstonePower() => true;

    public override bool isPoweringSide(IBlockReader iBlockReader, int x, int y, int z, int side) => true;

    public override bool isStrongPoweringSide(IBlockReader world, int x, int y, int z, int side) => true;
}
