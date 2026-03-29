using BetaSharp.Blocks.Materials;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Blocks;

internal class BlockPumpkin : Block
{
    private readonly bool lit;

    public BlockPumpkin(int id, int textureId, bool lit) : base(id, Material.Pumpkin)
    {
        this.textureId = textureId;
        setTickRandomly(true);
        this.lit = lit;
    }

    public override int getTexture(int side, int meta)
    {
        if (side == 1)
        {
            return textureId;
        }

        if (side == 0)
        {
            return textureId;
        }

        int faceTexture = textureId + 1 + 16;
        if (lit)
        {
            ++faceTexture;
        }

        return meta == 2 && side == 2 ? faceTexture :
            meta == 3 && side == 5 ? faceTexture :
            meta == 0 && side == 3 ? faceTexture :
            meta == 1 && side == 4 ? faceTexture : textureId + 16;
    }

    public override int getTexture(int side) => side == 1 ? textureId : side == 0 ? textureId : side == 3 ? textureId + 1 + 16 : textureId + 16;

    public override bool canPlaceAt(CanPlaceAtContext evt)
    {
        int blockId = evt.World.Reader.GetBlockId(evt.X, evt.Y, evt.Z);
        return (blockId == 0 || Blocks[blockId].material.IsReplaceable) && evt.World.Reader.ShouldSuffocate(evt.X, evt.Y - 1, evt.Z);
    }

    public override void onPlaced(OnPlacedEvent @event)
    {
        if (@event.Placer != null)
        {
            int direction = MathHelper.Floor(@event.Placer.yaw * 4.0F / 360.0F + 2.5D) & 3;
            @event.World.Writer.SetBlockMeta(@event.X, @event.Y, @event.Z, direction);
        }

        TrySpawnIronGolem(@event.World, @event.X, @event.Y, @event.Z);
    }

    private static void TrySpawnIronGolem(Worlds.Core.Systems.IWorldContext world, int x, int y, int z)
    {
        if (world.IsRemote)
        {
            return;
        }

        if (world.Reader.GetBlockId(x, y - 1, z) != Block.IronBlock.id || world.Reader.GetBlockId(x, y - 2, z) != Block.IronBlock.id)
        {
            return;
        }

        bool hasXArms = world.Reader.GetBlockId(x - 1, y - 1, z) == Block.IronBlock.id && world.Reader.GetBlockId(x + 1, y - 1, z) == Block.IronBlock.id;
        bool hasZArms = world.Reader.GetBlockId(x, y - 1, z - 1) == Block.IronBlock.id && world.Reader.GetBlockId(x, y - 1, z + 1) == Block.IronBlock.id;
        if (!hasXArms && !hasZArms)
        {
            return;
        }

        world.Writer.SetBlockWithoutCallingOnPlaced(x, y, z, 0, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 1, z, 0, 0);
        world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 2, z, 0, 0);
        if (hasXArms)
        {
            world.Writer.SetBlockWithoutCallingOnPlaced(x - 1, y - 1, z, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x + 1, y - 1, z, 0, 0);
        }
        else
        {
            world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 1, z - 1, 0, 0);
            world.Writer.SetBlockWithoutCallingOnPlaced(x, y - 1, z + 1, 0, 0);
        }

        EntityIronGolem golem = new(world);
        golem.SetPlayerCreated(true);
        golem.setPositionAndAnglesKeepPrevAngles(x + 0.5D, y - 1.95D, z + 0.5D, 0.0F, 0.0F);
        world.SpawnEntity(golem);

        for (int i = 0; i < 120; ++i)
        {
            world.Broadcaster.AddParticle("snowballpoof", x + world.Random.NextFloat(), y - 2 + world.Random.NextFloat() * 3.9D, z + world.Random.NextFloat(), 0.0D, 0.0D, 0.0D);
        }
    }
}
