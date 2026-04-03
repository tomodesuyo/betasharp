using BetaSharp.Blocks.Materials;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Blocks;

internal sealed class BlockSponge : Block
{
    public BlockSponge(int id) : base(id, Material.Sponge) => textureId = 48;

    public override int getTexture(int side, int meta)
    {
        return (meta & 3) switch
        {
            1 => 173,
            _ => 48
        };
    }

    protected override int getDroppedItemMeta(int blockMeta) => blockMeta & 1;

    public override void onPlaced(OnPlacedEvent @event)
    {
        TryAbsorb(@event.World, @event.X, @event.Y, @event.Z);
    }

    public override void neighborUpdate(OnTickEvent @event)
    {
        TryAbsorb(@event.World, @event.X, @event.Y, @event.Z);
    }

    private void TryAbsorb(IWorldContext world, int x, int y, int z)
    {
        if (world.IsRemote || world.Reader.GetBlockMeta(x, y, z) != 0)
        {
            return;
        }

        if (Absorb(world, x, y, z))
        {
            world.Writer.SetBlockMeta(x, y, z, 1);
        }
    }

    private static bool Absorb(IWorldContext world, int originX, int originY, int originZ)
    {
        Queue<(int X, int Y, int Z, int Depth)> queue = new();
        queue.Enqueue((originX, originY, originZ, 0));

        int absorbedCount = 0;
        while (queue.Count > 0)
        {
            var (x, y, z, depth) = queue.Dequeue();
            for (int side = 0; side < 6; ++side)
            {
                int neighborX = x;
                int neighborY = y;
                int neighborZ = z;

                switch (side)
                {
                    case 0:
                        --neighborY;
                        break;
                    case 1:
                        ++neighborY;
                        break;
                    case 2:
                        --neighborZ;
                        break;
                    case 3:
                        ++neighborZ;
                        break;
                    case 4:
                        --neighborX;
                        break;
                    default:
                        ++neighborX;
                        break;
                }

                if (world.Reader.GetMaterial(neighborX, neighborY, neighborZ) != Material.Water)
                {
                    continue;
                }

                if (!world.Writer.SetBlock(neighborX, neighborY, neighborZ, 0))
                {
                    continue;
                }

                ++absorbedCount;
                if (absorbedCount >= 64)
                {
                    return true;
                }

                if (depth < 6)
                {
                    queue.Enqueue((neighborX, neighborY, neighborZ, depth + 1));
                }
            }
        }

        return absorbedCount > 0;
    }
}
