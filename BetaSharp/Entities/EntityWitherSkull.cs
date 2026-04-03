using BetaSharp.Potions;
using BetaSharp.Util;
using BetaSharp.Util.Hit;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityWitherSkull : EntityFireball
{
    private readonly SyncedProperty<bool> _invulnerable;

    public override EntityType Type => EntityRegistry.WitherSkull;

    public EntityWitherSkull(IWorldContext world) : base(world)
    {
        setBoundingBoxSpacing(5.0F / 16.0F, 5.0F / 16.0F);
        _invulnerable = DataSynchronizer.MakeProperty(16, false);
    }

    public EntityWitherSkull(IWorldContext world, EntityLiving owner, double x, double y, double z) : base(world, owner, x, y, z)
    {
        setBoundingBoxSpacing(5.0F / 16.0F, 5.0F / 16.0F);
        _invulnerable = DataSynchronizer.MakeProperty(16, false);
    }

    public EntityWitherSkull(IWorldContext world, double x, double y, double z, double velocityX, double velocityY, double velocityZ) : base(world, x, y, z, velocityX, velocityY, velocityZ)
    {
        setBoundingBoxSpacing(5.0F / 16.0F, 5.0F / 16.0F);
        _invulnerable = DataSynchronizer.MakeProperty(16, false);
    }

    public bool IsInvulnerable() => _invulnerable.Value;

    public void SetInvulnerable(bool invulnerable)
    {
        _invulnerable.Value = invulnerable;
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
                int damage = owner is null ? 5 : 8;
                if (hitResult.Entity.damage(attacker, damage) && owner != null && !hitResult.Entity.isAlive())
                {
                    owner.heal(5);
                }

                if (hitResult.Entity is EntityLiving living)
                {
                    int durationSeconds = world.Difficulty switch
                    {
                        >= 3 => 40,
                        2 => 10,
                        _ => 0
                    };

                    if (durationSeconds > 0)
                    {
                        living.addPotionEffect(new PotionEffect(Potion.Wither.Id, durationSeconds * 20, 1));
                    }
                }
            }

            world.CreateExplosion(this, x, y, z, 1.0F, false);
        }

        markDead();
    }
}
