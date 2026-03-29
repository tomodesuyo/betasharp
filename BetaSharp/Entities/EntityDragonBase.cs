using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public abstract class EntityDragonBase : EntityLiving, SpawnableEntity
{
    protected EntityDragonBase(IWorldContext world) : base(world)
    {
    }

    public virtual bool DamageFromPart(EntityDragonPart part, Entity? source, int amount) => damage(source!, amount);

    protected bool SuperDamage(Entity? source, int amount) => base.damage(source!, amount);
}
