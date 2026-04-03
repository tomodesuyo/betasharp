using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemSkull : Item
{
    private static readonly string[] s_skullTypes =
    [
        "skeleton",
        "wither",
        "zombie",
        "char",
        "creeper"
    ];

    public ItemSkull(int id) : base(id)
    {
        setMaxCount(64);
        setHasSubtypes(true);
    }

    public override int getTextureId(int damage)
    {
        int skullType = NormalizeSkullType(damage);
        return skullType + 14 * 16;
    }

    public override string getItemNameIS(ItemStack itemStack)
    {
        return $"{base.getItemName()}.{s_skullTypes[NormalizeSkullType(itemStack.getDamage())]}";
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        if (side == 0)
        {
            return false;
        }

        int blockId = world.Reader.GetBlockId(x, y, z);
        bool replaceable = blockId != 0 && Block.Blocks[blockId].material.IsReplaceable;
        if (!replaceable)
        {
            switch (side)
            {
                case 1:
                    ++y;
                    break;
                case 2:
                    --z;
                    break;
                case 3:
                    ++z;
                    break;
                case 4:
                    --x;
                    break;
                case 5:
                    ++x;
                    break;
            }
        }

        if (itemStack.count == 0 || !Block.Skull.canPlaceAt(new CanPlaceAtContext(world, side, x, y, z)))
        {
            return false;
        }

        if (!world.Writer.SetBlockWithoutCallingOnPlaced(x, y, z, Block.Skull.id, 0))
        {
            return false;
        }

        Block.Skull.onPlaced(new OnPlacedEvent(world, entityPlayer, side, side, x, y, z));
        if (world.Entities.GetOrCreateBlockEntity<BlockEntitySkull>(x, y, z) is not { } skull)
        {
            return false;
        }

        skull.SetSkullType(NormalizeSkullType(itemStack.getDamage()));
        if (side == 1)
        {
            skull.SetSkullRotation(MathHelper.Floor(entityPlayer.yaw * 16.0F / 360.0F + 0.5D) & 15);
        }

        skull.markDirty();
        BlockSkull.CheckWitherSpawn(world, x, y, z, skull);

        world.Broadcaster.PlaySoundAtPos(
            x + 0.5F,
            y + 0.5F,
            z + 0.5F,
            Block.Skull.soundGroup.StepSound,
            (Block.Skull.soundGroup.Volume + 1.0F) / 2.0F,
            Block.Skull.soundGroup.Pitch * 0.8F);
        itemStack.ConsumeItem(entityPlayer);
        return true;
    }

    private static int NormalizeSkullType(int damage)
    {
        return damage < 0 || damage >= s_skullTypes.Length ? BlockEntitySkull.Skeleton : damage;
    }
}
