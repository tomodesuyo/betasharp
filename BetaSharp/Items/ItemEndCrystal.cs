using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemEndCrystal : Item
{
    public ItemEndCrystal(int id) : base(id)
    {
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        if (side != 1)
        {
            return false;
        }

        int baseBlockId = world.Reader.GetBlockId(x, y, z);
        if (baseBlockId != Block.Obsidian.id && baseBlockId != Block.Bedrock.id)
        {
            return false;
        }

        ++y;
        if (!world.Reader.IsAir(x, y, z) || !world.Reader.IsAir(x, y + 1, z))
        {
            return false;
        }

        if (!world.IsRemote)
        {
            EntityEnderCrystal crystal = new(world);
            crystal.setPositionAndAnglesKeepPrevAngles(x + 0.5D, y, z + 0.5D, entityPlayer.yaw, 0.0F);
            if (!world.SpawnEntity(crystal))
            {
                return false;
            }

            world.Broadcaster.PlaySoundAtPos(x + 0.5D, y + 0.5D, z + 0.5D, "random.glass", 1.0F, 1.0F);
        }

        itemStack.ConsumeItem(entityPlayer);
        return true;
    }
}
