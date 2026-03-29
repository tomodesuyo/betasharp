using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemGlassBottle : Item
{
    public ItemGlassBottle(int id) : base(id)
    {
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        float partialTick = 1.0F;
        float pitch = entityPlayer.prevPitch + (entityPlayer.pitch - entityPlayer.prevPitch) * partialTick;
        float yaw = entityPlayer.prevYaw + (entityPlayer.yaw - entityPlayer.prevYaw) * partialTick;
        double x = entityPlayer.prevX + (entityPlayer.x - entityPlayer.prevX) * partialTick;
        double y = entityPlayer.prevY + (entityPlayer.y - entityPlayer.prevY) * partialTick + 1.62D - entityPlayer.standingEyeHeight;
        double z = entityPlayer.prevZ + (entityPlayer.z - entityPlayer.prevZ) * partialTick;
        Vec3D rayStart = new(x, y, z);
        float cosYaw = MathHelper.Cos(-yaw * ((float)Math.PI / 180.0F) - (float)Math.PI);
        float sinYaw = MathHelper.Sin(-yaw * ((float)Math.PI / 180.0F) - (float)Math.PI);
        float cosPitch = -MathHelper.Cos(-pitch * ((float)Math.PI / 180.0F));
        float sinPitch = MathHelper.Sin(-pitch * ((float)Math.PI / 180.0F));
        float dirX = sinYaw * cosPitch;
        float dirZ = cosYaw * cosPitch;
        double reachDistance = 5.0D;
        Vec3D rayEnd = rayStart + new Vec3D(dirX * reachDistance, sinPitch * reachDistance, dirZ * reachDistance);
        HitResult hitResult = world.Reader.Raycast(rayStart, rayEnd, true);
        if (hitResult.Type == HitResultType.TILE)
        {
            int hitX = hitResult.BlockX;
            int hitY = hitResult.BlockY;
            int hitZ = hitResult.BlockZ;
            if (world.CanInteract(entityPlayer, hitX, hitY, hitZ) && world.Reader.GetMaterial(hitX, hitY, hitZ) == Material.Water)
            {
                return FillBottle(itemStack, entityPlayer);
            }
        }

        if (!TryFindWaterAlongRay(world, entityPlayer, rayStart, rayEnd))
        {
            return itemStack;
        }

        return FillBottle(itemStack, entityPlayer);
    }

    private static ItemStack FillBottle(ItemStack itemStack, EntityPlayer entityPlayer)
    {
        ItemStack waterBottle = new(Item.Potion, 1, 0);
        if (entityPlayer.capabilities.IsCreativeMode)
        {
            if (!entityPlayer.inventory.addItemStackToInventory(waterBottle.copy()))
            {
                entityPlayer.dropItem(waterBottle.copy());
            }

            return itemStack;
        }

        --itemStack.count;
        if (itemStack.count <= 0)
        {
            return waterBottle;
        }

        if (!entityPlayer.inventory.addItemStackToInventory(waterBottle.copy()))
        {
            entityPlayer.dropItem(waterBottle.copy());
        }

        return itemStack;
    }

    private static bool TryFindWaterAlongRay(IWorldContext world, EntityPlayer entityPlayer, Vec3D rayStart, Vec3D rayEnd)
    {
        const int steps = 32;
        Vec3D rayDelta = (rayEnd - rayStart) / steps;
        Vec3D sample = rayStart;

        for (int i = 0; i <= steps; ++i)
        {
            int x = MathHelper.Floor(sample.x);
            int y = MathHelper.Floor(sample.y);
            int z = MathHelper.Floor(sample.z);
            if (world.CanInteract(entityPlayer, x, y, z) && world.Reader.GetMaterial(x, y, z) == Material.Water)
            {
                return true;
            }

            sample += rayDelta;
        }

        return false;
    }
}
