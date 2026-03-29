using BetaSharp.Items;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityWitherSkeleton : EntitySkeleton
{
    private static readonly ItemStack s_heldItem = new(Item.StoneSword, 1);

    public override EntityType Type => EntityRegistry.WitherSkeleton;

    public EntityWitherSkeleton(IWorldContext world) : base(world)
    {
        texture = "/mob/skeleton_wither.png";
        isImmuneToFire = true;
        attackStrength = 4;
        movementSpeed = 0.28F;
    }

    public override void tickMovement()
    {
        base.tickMovement();
        fireTicks = 0;
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (attackTime <= 0 && distance < 2.4F && entity.boundingBox.MaxY > boundingBox.MinY && entity.boundingBox.MinY < boundingBox.MaxY)
        {
            attackTime = 20;
            entity.damage(this, attackStrength);
        }
    }

    protected override int getDropItemId() => Item.Coal.id;

    protected override void dropFewItems()
    {
        int coalCount = random.NextInt(2);
        for (int i = 0; i < coalCount; ++i)
        {
            dropItem(Item.Coal.id, 1);
        }

        int boneCount = random.NextInt(3);
        for (int i = 0; i < boneCount; ++i)
        {
            dropItem(Item.Bone.id, 1);
        }
    }

    public override ItemStack getHeldItem() => s_heldItem;
}
