using BetaSharp.Blocks;
using BetaSharp.Entities;
using BetaSharp.Util.Hit;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemEnderEye : Item
{
    public ItemEnderEye(int id) : base(id)
    {
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        int blockId = world.Reader.GetBlockId(x, y, z);
        int meta = world.Reader.GetBlockMeta(x, y, z);
        if (blockId != Block.EndPortalFrame.id || BlockEndPortalFrame.HasEye(meta))
        {
            return false;
        }

        world.Writer.SetBlockMeta(x, y, z, meta | 4);
        itemStack.ConsumeItem(entityPlayer);

        for (int i = 0; i < 16; ++i)
        {
            double particleX = x + (5.0F + itemRand.NextFloat() * 6.0F) / 16.0F;
            double particleY = y + 13.0F / 16.0F;
            double particleZ = z + (5.0F + itemRand.NextFloat() * 6.0F) / 16.0F;
            world.Broadcaster.AddParticle("smoke", particleX, particleY, particleZ, 0.0D, 0.0D, 0.0D);
        }

        BlockEndPortalFrame.TryActivatePortal(world, x, y, z);
        return true;
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        HitResult hitResult = entityPlayer.rayTrace(5.0D, 1.0F);
        if (hitResult.Type == HitResultType.TILE && world.Reader.GetBlockId(hitResult.BlockX, hitResult.BlockY, hitResult.BlockZ) == Block.EndPortalFrame.id)
        {
            return itemStack;
        }

        if (!world.IsRemote)
        {
            Vec3i? pos = world.ChunkHost.ChunkSource.FindNearestStructure("Stronghold", MathHelper.Floor(entityPlayer.x), MathHelper.Floor(entityPlayer.y), MathHelper.Floor(entityPlayer.z));
            if (pos != null)
            {
                EntityEnderEye eye = new(world, entityPlayer.x, entityPlayer.y + 1.62D - entityPlayer.standingEyeHeight, entityPlayer.z);
                eye.SetTarget(pos.Value.X, pos.Value.Y, pos.Value.Z);
                world.SpawnEntity(eye);
                world.Broadcaster.PlaySoundAtEntity(entityPlayer, "random.bow", 0.5F, 0.4F / (itemRand.NextFloat() * 0.4F + 0.8F));
                world.Broadcaster.WorldEvent(1002, MathHelper.Floor(entityPlayer.x), MathHelper.Floor(entityPlayer.y), MathHelper.Floor(entityPlayer.z), 0);
                itemStack.ConsumeItem(entityPlayer);
            }
        }

        return itemStack;
    }
}
