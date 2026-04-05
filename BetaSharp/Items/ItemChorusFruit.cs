using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemChorusFruit : ItemFood
{
    public ItemChorusFruit(int id) : base(id, 4, false)
    {
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        if (!entityPlayer.CanUseChorusFruit())
        {
            return itemStack;
        }

        ItemStack result = base.use(itemStack, world, entityPlayer);
        entityPlayer.StartChorusFruitCooldown();
        if (world.IsRemote)
        {
            return result;
        }

        double startX = entityPlayer.x;
        double startY = entityPlayer.y;
        double startZ = entityPlayer.z;

        for (int i = 0; i < 16; ++i)
        {
            double targetX = entityPlayer.x + (world.Random.NextDouble() - 0.5D) * 16.0D;
            double targetY = Math.Clamp(entityPlayer.y + world.Random.NextInt(16) - 8, 0.0D, 127.0D);
            double targetZ = entityPlayer.z + (world.Random.NextDouble() - 0.5D) * 16.0D;

            if (entityPlayer.vehicle != null)
            {
                entityPlayer.setVehicle(null);
            }

            if (TryTeleport(entityPlayer, world, targetX, targetY, targetZ))
            {
                world.Broadcaster.PlaySoundAtPos(startX, startY, startZ, "mob.endermen.portal", 1.0F, 1.0F);
                world.Broadcaster.PlaySoundAtEntity(entityPlayer, "mob.endermen.portal", 1.0F, 1.0F);
                break;
            }
        }

        return result;
    }

    private static bool TryTeleport(EntityPlayer player, IWorldContext world, double targetX, double targetY, double targetZ)
    {
        double previousX = player.x;
        double previousY = player.y;
        double previousZ = player.z;
        player.x = targetX;
        player.y = targetY;
        player.z = targetZ;

        int blockX = MathHelper.Floor(player.x);
        int blockY = MathHelper.Floor(player.y);
        int blockZ = MathHelper.Floor(player.z);
        bool foundGround = false;

        while (!foundGround && blockY > 0)
        {
            int belowId = world.Reader.GetBlockId(blockX, blockY - 1, blockZ);
            if (belowId > 0 && Block.Blocks[belowId].material.BlocksMovement)
            {
                foundGround = true;
            }
            else
            {
                --player.y;
                --blockY;
            }
        }

        if (!foundGround)
        {
            player.setPosition(previousX, previousY, previousZ);
            return false;
        }

        player.setPosition(player.x, player.y, player.z);
        if (world.Entities.GetEntityCollisionsScratch(player, player.boundingBox).Count > 0 || world.Reader.IsMaterialInBox(player.boundingBox, material => material.IsFluid))
        {
            player.setPosition(previousX, previousY, previousZ);
            return false;
        }

        for (int i = 0; i < 128; ++i)
        {
            double progress = i / 127.0D;
            float velocityX = (world.Random.NextFloat() - 0.5F) * 0.2F;
            float velocityY = (world.Random.NextFloat() - 0.5F) * 0.2F;
            float velocityZ = (world.Random.NextFloat() - 0.5F) * 0.2F;
            double particleX = previousX + (player.x - previousX) * progress + (world.Random.NextDouble() - 0.5D) * player.width * 2.0D;
            double particleY = previousY + (player.y - previousY) * progress + world.Random.NextDouble() * player.height;
            double particleZ = previousZ + (player.z - previousZ) * progress + (world.Random.NextDouble() - 0.5D) * player.width * 2.0D;
            world.Broadcaster.AddParticle("portal", particleX, particleY, particleZ, velocityX, velocityY, velocityZ);
        }

        return true;
    }
}
