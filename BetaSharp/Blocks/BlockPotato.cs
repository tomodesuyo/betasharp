using BetaSharp.Entities;
using BetaSharp.Items;

namespace BetaSharp.Blocks;

internal sealed class BlockPotato : BlockCrops
{
    public BlockPotato(int id, int textureId) : base(id, textureId)
    {
    }

    protected override int getSeedItemId()
    {
        return Item.Potato.id;
    }

    protected override int getCropItemId()
    {
        return Item.Potato.id;
    }

    public override int getTexture(int side, int meta)
    {
        if (meta < 7)
        {
            if (meta == 6)
            {
                meta = 5;
            }

            return textureId + (meta >> 1);
        }

        return textureId + 4;
    }

    public override void dropStacks(OnDropEvent @event)
    {
        base.dropStacks(@event);
        if (!@event.World.IsRemote && @event.Meta >= 7 && Random.Shared.Next(50) == 0)
        {
            dropStack(@event.World, @event.X, @event.Y, @event.Z, new ItemStack(Item.PoisonousPotato));
        }
    }
}
