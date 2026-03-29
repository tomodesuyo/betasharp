using BetaSharp.Blocks;
using BetaSharp.Util.Hit;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntitySmallFireball : EntityFireball
{
    public override EntityType Type => EntityRegistry.SmallFireball;

    public EntitySmallFireball(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(5.0F / 16.0F, 5.0F / 16.0F);
    }

    public EntitySmallFireball(IWorldContext world, EntityLiving owner, double x, double y, double z) : base(world, owner, x, y, z)
    {
        setBoundingBoxSpacing(5.0F / 16.0F, 5.0F / 16.0F);
    }

    public EntitySmallFireball(IWorldContext world, double x, double y, double z, double velocityX, double velocityY, double velocityZ) : base(world, x, y, z, velocityX, velocityY, velocityZ)
    {
        setBoundingBoxSpacing(5.0F / 16.0F, 5.0F / 16.0F);
    }

    public override bool isCollidable()
    {
        return false;
    }

    public override bool damage(Entity entity, int amount)
    {
        return false;
    }

    protected override void OnImpact(HitResult hitResult)
    {
        if (!world.IsRemote)
        {
            if (hitResult.Entity != null)
            {
                Entity attacker = owner is null ? this : owner;
                if (hitResult.Entity.damage(attacker, 5))
                {
                    hitResult.Entity.fireTicks = 100;
                }
            }
            else
            {
                int x = hitResult.BlockX;
                int y = hitResult.BlockY;
                int z = hitResult.BlockZ;

                switch (hitResult.Side)
                {
                    case 0:
                        --y;
                        break;
                    case 1:
                        ++y;
                        break;
                    case 2:
                        --z;
                        break;
                    case 3:
                        ++z;
                        break;
                    case 4:
                        --x;
                        break;
                    case 5:
                        ++x;
                        break;
                }

                if (world.Reader.IsAir(x, y, z))
                {
                    world.Writer.SetBlock(x, y, z, Block.Fire.id, 0, doUpdate: false);
                }
            }
        }

        markDead();
    }
}
