using BetaSharp.Blocks.Materials;

namespace BetaSharp.Blocks;

internal sealed class BlockHay : Block
{
    private const int SideTexture = 136;
    private const int EndTexture = 137;

    public BlockHay(int id) : base(id, Material.SolidOrganic)
    {
        textureId = SideTexture;
    }

    public override int getTexture(int side, int meta)
    {
        int axis = meta & 12;
        bool isEndFace = axis switch
        {
            4 => side is 4 or 5,
            8 => side is 2 or 3,
            _ => side is 0 or 1,
        };

        return isEndFace ? EndTexture : SideTexture;
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        int meta = @event.Direction switch
        {
            4 or 5 => 4,
            2 or 3 => 8,
            _ => 0,
        };

        @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, meta);
    }
}
