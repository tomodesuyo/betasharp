using BetaSharp.Blocks;
using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Items;

internal class ItemDoor : Item
{

    private readonly Material _doorMaterial;

    public ItemDoor(int id, Material material) : base(id)
    {
        _doorMaterial = material;
        maxCount = 64;
    }

    public override bool useOnBlock(ItemStack itemStack, EntityPlayer entityPlayer, IWorldContext world, int x, int y, int z, int side)
    {
        if (side != 1) return false;
        y++;

        int blockId = _doorMaterial == Material.Wood ? Block.Door.id : Block.IronDoor.id;
        if (!Block.Blocks[blockId].canPlaceAt(new CanPlaceAtContext(world, 0, x, y, z))) return false;

        int facing = MathHelper.Floor((entityPlayer.yaw + 180.0f) * 4.0f / 360.0f - 0.5f) & 3;
        PlaceDoorBlock(world, x, y, z, facing, Block.Blocks[blockId]);
        world.Broadcaster.NotifyNeighbors(x, y, z, blockId);
        world.Broadcaster.NotifyNeighbors(x, y + 1, z, blockId);
        itemStack.ConsumeItem(entityPlayer);
        return true;
    }

    public static void PlaceDoorBlock(IWorldContext world, int x, int y, int z, int facing, Block doorBlock)
    {
        int offsetX = 0;
        int offsetZ = 0;
        if (facing == 0) offsetZ = 1;
        if (facing == 1) offsetX = -1;
        if (facing == 2) offsetZ = -1;
        if (facing == 3) offsetX = 1;

        int leftSolid = (world.Reader.ShouldSuffocate(x - offsetX, y, z - offsetZ) ? 1 : 0)
                      + (world.Reader.ShouldSuffocate(x - offsetX, y + 1, z - offsetZ) ? 1 : 0);
        int rightSolid = (world.Reader.ShouldSuffocate(x + offsetX, y, z + offsetZ) ? 1 : 0)
                       + (world.Reader.ShouldSuffocate(x + offsetX, y + 1, z + offsetZ) ? 1 : 0);
        bool leftHasDoor = world.Reader.GetBlockId(x - offsetX, y, z - offsetZ) == doorBlock.id
                        || world.Reader.GetBlockId(x - offsetX, y + 1, z - offsetZ) == doorBlock.id;
        bool rightHasDoor = world.Reader.GetBlockId(x + offsetX, y, z + offsetZ) == doorBlock.id
                         || world.Reader.GetBlockId(x + offsetX, y + 1, z + offsetZ) == doorBlock.id;

        bool mirror = (leftHasDoor && !rightHasDoor) || rightSolid > leftSolid;
        world.Writer.SetBlock(x, y, z, doorBlock.id, facing);
        world.Writer.SetBlock(x, y + 1, z, doorBlock.id, 8 | (mirror ? 1 : 0));
    }
}
