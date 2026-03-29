using BetaSharp.Entities;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal sealed class ItemSeedFood : ItemFood
{
    private readonly int _cropId;
    private readonly int _soilId;

    public ItemSeedFood(int id, int healAmount, int cropId, int soilId) : base(id, healAmount, false)
    {
        maxCount = 64;
        _cropId = cropId;
        _soilId = soilId;
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        if (side != 1)
        {
            return false;
        }

        if (world.Reader.GetBlockId(x, y, z) == _soilId && world.Reader.IsAir(x, y + 1, z))
        {
            world.Writer.SetBlock(x, y + 1, z, _cropId);
            itemStack.ConsumeItem(entityPlayer);
            return true;
        }

        return false;
    }
}
