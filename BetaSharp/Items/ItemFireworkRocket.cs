using BetaSharp.Entities;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemFireworkRocket : Item
{
    public ItemFireworkRocket(int id) : base(id)
    {
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        if (entityPlayer.IsGlidingWithElytra())
        {
            return Launch(itemStack, entityPlayer, world, entityPlayer.x, entityPlayer.y + 0.5D, entityPlayer.z, boostGlidingPlayer: true);
        }

        switch (side)
        {
            case 0:
                --y;
                break;
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

        return Launch(itemStack, entityPlayer, world, x + 0.5D, y + 0.15D, z + 0.5D, boostGlidingPlayer: false);
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        bool boostGlidingPlayer = entityPlayer.IsGlidingWithElytra();
        if (!Launch(itemStack, entityPlayer, world, entityPlayer.x, entityPlayer.y + 0.5D, entityPlayer.z, boostGlidingPlayer))
        {
            return itemStack;
        }

        return itemStack;
    }

    private bool Launch(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, double x, double y, double z, bool boostGlidingPlayer)
    {
        if (!world.IsRemote)
        {
            EntityFireworkRocket rocket = boostGlidingPlayer ? new EntityFireworkRocket(world, entityPlayer) : new EntityFireworkRocket(world, x, y, z);

            if (!world.SpawnEntity(rocket))
            {
                return false;
            }
        }

        if (!entityPlayer.capabilities.IsCreativeMode)
        {
            itemStack.count--;
        }

        return true;
    }
}
