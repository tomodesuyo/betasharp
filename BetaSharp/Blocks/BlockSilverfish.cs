using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;

namespace BetaSharp.Blocks;

internal class BlockSilverfish : Block
{
    public BlockSilverfish(int id) : base(id, 1, Material.Stone)
    {
    }

    public static bool CanContainSilverfish(int blockId, int blockMeta)
    {
        return blockId == Block.Stone.id
               || blockId == Block.Cobblestone.id
               || blockId == Block.StoneBrick.id && (blockMeta & 3) <= 3;
    }

    public static int GetMonsterEggMetaForModel(int blockId, int blockMeta)
    {
        if (blockId == Block.Cobblestone.id)
        {
            return 1;
        }

        if (blockId == Block.StoneBrick.id)
        {
            return (blockMeta & 3) switch
            {
                1 => 3,
                2 => 4,
                3 => 5,
                _ => 2,
            };
        }

        return 0;
    }

    public static int GetModelBlockId(int meta)
    {
        return meta switch
        {
            1 => Block.Cobblestone.id,
            2 or 3 or 4 or 5 => Block.StoneBrick.id,
            _ => Block.Stone.id,
        };
    }

    public static int GetModelBlockMeta(int meta)
    {
        return meta switch
        {
            3 => 1,
            4 => 2,
            5 => 3,
            _ => 0,
        };
    }

    public override int getDroppedItemId(int blockMeta)
    {
        return 0;
    }

    public override int getTexture(int side, int meta)
    {
        int modelBlockId = GetModelBlockId(meta);
        int modelMeta = GetModelBlockMeta(meta);
        return Block.Blocks[modelBlockId].getTexture(side, modelMeta);
    }

    public override int getDroppedItemCount()
    {
        return 0;
    }

    public override void onAfterBreak(OnAfterBreakEvent @event)
    {
        if (!@event.World.IsRemote)
        {
            EntitySilverfish silverfish = new(@event.World);
            silverfish.setPositionAndAngles(@event.X + 0.5D, @event.Y, @event.Z + 0.5D, 0.0F, 0.0F);
            @event.World.SpawnEntity(silverfish);
        }

        base.onAfterBreak(@event);
    }
}
