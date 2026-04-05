using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal class ItemBoat : Item
{

    public ItemBoat(int id) : base(id)
    {
        maxCount = 1;
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        float partialTick = 1.0F;
        float pitch = entityPlayer.prevPitch + (entityPlayer.pitch - entityPlayer.prevPitch) * partialTick;
        float yaw = entityPlayer.prevYaw + (entityPlayer.yaw - entityPlayer.prevYaw) * partialTick;
        double x = entityPlayer.prevX + (entityPlayer.x - entityPlayer.prevX) * (double)partialTick;
        double y = entityPlayer.prevY + (entityPlayer.y - entityPlayer.prevY) * (double)partialTick + 1.62D - (double)entityPlayer.standingEyeHeight;
        double z = entityPlayer.prevZ + (entityPlayer.z - entityPlayer.prevZ) * (double)partialTick;
        Vec3D rayStart = new Vec3D(x, y, z);
        float cosYaw = MathHelper.Cos(-yaw * ((float)Math.PI / 180.0F) - (float)Math.PI);
        float sinYaw = MathHelper.Sin(-yaw * ((float)Math.PI / 180.0F) - (float)Math.PI);
        float cosPitch = -MathHelper.Cos(-pitch * ((float)Math.PI / 180.0F));
        float sinPitch = MathHelper.Sin(-pitch * ((float)Math.PI / 180.0F));
        float dirX = sinYaw * cosPitch;
        float dirZ = cosYaw * cosPitch;
        double rayLength = 5.0D;
        Vec3D rayEnd = rayStart + new Vec3D((double)dirX * rayLength, (double)sinPitch * rayLength, (double)dirZ * rayLength);
        HitResult hitResult = world.Reader.Raycast(rayStart, rayEnd, true);
        if (hitResult.Type == HitResultType.MISS)
        {
            return itemStack;
        }

        Vec3D look = entityPlayer.getLook(1.0F);
        List<Entity> entities = world.Entities.GetEntities(entityPlayer, entityPlayer.boundingBox.Stretch(look.x * rayLength, look.y * rayLength, look.z * rayLength).Expand(1.0D, 1.0D, 1.0D));
        for (int i = 0; i < entities.Count; ++i)
        {
            Entity entity = entities[i];
            if (!entity.isCollidable())
            {
                continue;
            }

            Box expandedBox = entity.boundingBox.Expand(entity.getTargetingMargin(), entity.getTargetingMargin(), entity.getTargetingMargin());
            if (expandedBox.Contains(rayStart))
            {
                return itemStack;
            }
        }

        if (hitResult.Type != HitResultType.TILE)
        {
            return itemStack;
        }

        int hitX = hitResult.BlockX;
        int hitY = hitResult.BlockY;
        int hitZ = hitResult.BlockZ;
        int hitBlockId = world.Reader.GetBlockId(hitX, hitY, hitZ);
        bool hitWater = hitBlockId == Block.Water.id || hitBlockId == Block.FlowingWater.id;
        double spawnY = hitWater ? hitResult.Pos.y - 0.12D : hitResult.Pos.y;
        EntityBoat boat = new(world, hitResult.Pos.x, spawnY, hitResult.Pos.z)
        {
            yaw = entityPlayer.yaw
        };

        if (world.Entities.GetEntityCollisionsScratch(boat, boat.boundingBox.Contract(0.1D, 0.0D, 0.1D)).Count > 0)
        {
            return itemStack;
        }

        if (!world.IsRemote && world.SpawnEntity(boat))
        {
            itemStack.ConsumeItem(entityPlayer);
        }

        return itemStack;
    }
}
