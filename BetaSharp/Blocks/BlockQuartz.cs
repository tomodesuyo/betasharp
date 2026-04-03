using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal sealed class BlockQuartz : Block
{
    private const int NormalTexture = 236;
    private const int ChiseledTexture = 252;
    private const int PillarSideTexture = 236;
    private const int PillarEndTexture = 252;

    public BlockQuartz(int id) : base(id, Material.Stone)
    {
        textureId = NormalTexture;
    }

    public override int getTexture(int side, int meta)
    {
        int variant = meta & 3;
        if (variant == 1)
        {
            return ChiseledTexture;
        }

        if (variant != 2)
        {
            return NormalTexture;
        }

        int axis = meta & 12;
        bool isEndFace = axis switch
        {
            4 => side is 4 or 5,
            8 => side is 2 or 3,
            _ => side is 0 or 1
        };

        return isEndFace ? PillarEndTexture : PillarSideTexture;
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        int meta = @event.World.Reader.GetBlockMeta(@event.X, @event.Y, @event.Z) & 3;
        if (meta != 2)
        {
            return;
        }

        int axis = @event.Direction switch
        {
            4 or 5 => 4,
            2 or 3 => 8,
            _ => 0
        };
        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta | axis);
    }

    protected override int getDroppedItemMeta(int blockMeta)
    {
        return blockMeta & 3;
    }
}
