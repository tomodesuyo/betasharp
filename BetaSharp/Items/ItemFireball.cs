using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemFireball : Item
{
    public ItemFireball(int id) : base(id)
    {
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int meta)
    {
        switch (meta)
        {
            case 0: --y; break;
            case 1: ++y; break;
            case 2: --z; break;
            case 3: ++z; break;
            case 4: --x; break;
            case 5: ++x; break;
        }

        if (!world.CanInteract(entityPlayer, x, y, z))
        {
            return false;
        }

        if (world.Reader.GetBlockId(x, y, z) == 0)
        {
            world.Broadcaster.PlaySoundAtPos(x + 0.5F, y + 0.5F, z + 0.5F, "fire.ignite", 1.0F, itemRand.NextFloat() * 0.4F + 0.8F);
            world.Writer.SetBlock(x, y, z, Block.Fire.id);
        }

        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            itemStack.ConsumeItem(entityPlayer);
        }

        return true;
    }
}
