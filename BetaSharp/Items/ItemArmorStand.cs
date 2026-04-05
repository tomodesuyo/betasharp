using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemArmorStand : Item
{
    public ItemArmorStand(int id) : base(id)
    {
        setMaxCount(16);
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

        if (!world.Reader.IsAir(x, y, z) && !world.Reader.GetMaterial(x, y, z).IsReplaceable)
        {
            return false;
        }

        if (!world.Reader.IsAir(x, y + 1, z) && !world.Reader.GetMaterial(x, y + 1, z).IsReplaceable)
        {
            return false;
        }

        if (!world.Reader.ShouldSuffocate(x, y - 1, z))
        {
            return false;
        }

        Box spawnBox = new(x + 0.25D, y, z + 0.25D, x + 0.75D, y + 1.975D, z + 0.75D);
        if (world.Entities.GetEntities(null, spawnBox).Count > 0)
        {
            return false;
        }

        if (!world.IsRemote)
        {
            EntityArmorStand armorStand = new(world);
            float wrappedYaw = entityPlayer.yaw - 180.0F;
            while (wrappedYaw < -180.0F)
            {
                wrappedYaw += 360.0F;
            }

            while (wrappedYaw >= 180.0F)
            {
                wrappedYaw -= 360.0F;
            }

            float snappedYaw = MathHelper.Floor((wrappedYaw + 22.5F) / 45.0F) * 45.0F;
            armorStand.setPositionAndAnglesKeepPrevAngles(x + 0.5D, y, z + 0.5D, snappedYaw, 0.0F);
            armorStand.bodyYaw = armorStand.yaw;
            armorStand.lastBodyYaw = armorStand.yaw;
            armorStand.prevYaw = armorStand.yaw;
            armorStand.prevPitch = 0.0F;
            armorStand.ResetPose();
            if (!world.SpawnEntity(armorStand))
            {
                return false;
            }
        }

        itemStack.ConsumeItem(entityPlayer);
        return true;
    }
}
