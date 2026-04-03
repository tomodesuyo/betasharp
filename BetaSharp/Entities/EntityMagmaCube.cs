using BetaSharp.Items;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityMagmaCube : EntitySlime
{
    public override EntityType Type => EntityRegistry.MagmaCube;

    public EntityMagmaCube(IWorldContext world) : base(world)
    {
        texture = "/mob/lava.png";
        isImmuneToFire = true;
    }

    public override float getBrightnessAtEyes(float var1) => 1.0F;

    protected override void jump()
    {
        if (isTouchingLava())
        {
            velocityY = 0.22D + getSlimeSize() * 0.05D;
            return;
        }

        velocityY = 0.42F + getSlimeSize() * 0.1F;
    }

    protected override int getDropItemId() => getSlimeSize() > 1 ? Item.MagmaCream.id : 0;

    protected override void onLanding(float fallDistance)
    {
    }

    public override bool canSpawn()
    {
        return world.Difficulty > 0 &&
               world.Entities.CanSpawnEntity(boundingBox) &&
               world.Entities.GetEntityCollisionsScratch(this, boundingBox).Count == 0 &&
               !world.Reader.IsMaterialInBox(boundingBox, material => material.IsFluid);
    }

    protected override int getJumpDelay()
    {
        return base.getJumpDelay() * 4;
    }

    protected override string getParticleName()
    {
        return "flame";
    }

    protected override void alterSquishAmount()
    {
        squishAmount *= 0.9F;
    }

    protected override EntitySlime createInstance()
    {
        return new EntityMagmaCube(world);
    }

    protected override int getAttackStrength()
    {
        return base.getAttackStrength() + 2;
    }
}
