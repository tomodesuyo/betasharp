using BetaSharp.Blocks;
using BetaSharp.NBT;
using BetaSharp.Util.Maths;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityEnderCrystal : EntityLiving, SpawnableEntity
{
    public override EntityType Type => EntityRegistry.EnderCrystal;
    public int InnerRotation { get; private set; }

    public EntityEnderCrystal(IWorldContext world) : base(world)
    {
        preventEntitySpawning = true;
        setBoundingBoxSpacing(2.0F, 2.0F);
        standingEyeHeight = height / 2.0F;
        health = 5;
        maxHealth = 5;
        InnerRotation = random.NextInt(100000);
    }

    protected override bool canDespawn() => false;

    public override bool canSpawn() => true;

    public override float getShadowRadius() => 0.0F;

    public override bool damage(Entity entity, int amount)
    {
        if (dead || world.IsRemote)
        {
            return false;
        }

        health = 0;
        markDead();
        world.CreateExplosion(null, x, y, z, 6.0F);
        return true;
    }

    public override void tickMovement()
    {
        ++InnerRotation;

        int blockX = MathHelper.Floor(x);
        int blockY = MathHelper.Floor(y);
        int blockZ = MathHelper.Floor(z);
        if (world.Reader.GetBlockId(blockX, blockY, blockZ) != Block.Fire.id)
        {
            world.Writer.SetBlockWithoutNotifyingNeighbors(blockX, blockY, blockZ, Block.Fire.id, 0, false);
        }
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
        base.writeNbt(nbt);
        nbt.SetInteger("InnerRotation", InnerRotation);
    }

    public override void readNbt(NBTTagCompound nbt)
    {
        base.readNbt(nbt);
        InnerRotation = nbt.GetInteger("InnerRotation");
    }
}
