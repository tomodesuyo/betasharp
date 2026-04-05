using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemLead : Item
{
    public ItemLead(int id) : base(id)
    {
    }

    public override void useOnEntity(ItemStack itemStack, EntityLiving entityLiving, EntityPlayer entityPlayer)
    {
        if (entityLiving is not EntityAnimal animal)
        {
            return;
        }

        bool alreadyLeashed = animal.IsLeashed;
        if (animal.IsLeashedTo(entityPlayer))
        {
            return;
        }

        animal.SetLeashedTo(entityPlayer);
        if (!alreadyLeashed)
        {
            itemStack.ConsumeItem(entityPlayer);
        }
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        int blockId = world.Reader.GetBlockId(x, y, z);
        if (blockId != Block.Fence.id && blockId != Block.NetherFence.id)
        {
            return false;
        }

        bool attachedAny = false;
        Box searchBox = new(x - 7.0D, y - 7.0D, z - 7.0D, x + 8.0D, y + 8.0D, z + 8.0D);
        List<Entity> nearbyEntities = world.Entities.GetEntities(null, searchBox);
        for (int i = 0; i < nearbyEntities.Count; ++i)
        {
            if (nearbyEntities[i] is EntityAnimal animal && animal.IsLeashedTo(entityPlayer))
            {
                animal.AttachLeashToFence(x, y, z);
                attachedAny = true;
            }
        }

        return attachedAny;
    }
}
