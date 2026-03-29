using BetaSharp.NBT;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityDragonPart : Entity
{
    public override EntityType? Type => null;
    public EntityDragonBase Owner { get; }
    public string Name { get; }

    public EntityDragonPart(EntityDragonBase owner, string name, float width, float height) : base(owner.world)
    {
        Owner = owner;
        Name = name;
        setBoundingBoxSpacing(width, height);
    }

    public override bool isCollidable() => true;

    public override bool damage(Entity entity, int amount)
    {
        return Owner.DamageFromPart(this, entity, amount);
    }

    public override void writeNbt(NBTTagCompound nbt)
    {
    }

    public override void readNbt(NBTTagCompound nbt)
    {
    }
}
