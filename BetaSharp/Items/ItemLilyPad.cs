using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Util.Hit;
using BetaSharp.Items;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemLilyPad : Item
{
    public ItemLilyPad(int id) : base(id)
    {
        setTextureId(Block.LilyPad.getTexture(2));
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int meta)
    {
        return false;
    }

    public override ItemStack use(ItemStack itemStack, IWorldContext world, EntityPlayer entityPlayer)
    {
        HitResult hitResult = entityPlayer.rayTrace(5.0D, 1.0F);
        if (hitResult.Type != HitResultType.TILE)
        {
            return itemStack;
        }

        int x = hitResult.BlockX;
        int y = hitResult.BlockY;
        int z = hitResult.BlockZ;
        if (!world.CanInteract(entityPlayer, x, y, z))
        {
            return itemStack;
        }

        if (world.Reader.GetMaterial(x, y, z) == Material.Water && world.Reader.GetBlockMeta(x, y, z) == 0 && world.Reader.IsAir(x, y + 1, z) && world.Writer.SetBlock(x, y + 1, z, Block.LilyPad.id))
        {
            if (!entityPlayer.capabilities.IsCreativeMode)
            {
                --itemStack.count;
            }
        }

        return itemStack;
    }

    public override int getColorMultiplier(int damage)
    {
        return Block.LilyPad.getColor(damage);
    }
}
