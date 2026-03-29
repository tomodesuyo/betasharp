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

    public override bool canSpawn()
    {
        return world.Difficulty > 0 &&
               world.Entities.CanSpawnEntity(boundingBox) &&
               world.Entities.GetEntityCollisionsScratch(this, boundingBox).Count == 0 &&
               !world.Reader.IsMaterialInBox(boundingBox, material => material.IsFluid);
    }

    public override void markDead()
    {
        int size = getSlimeSize();
        if (!world.IsRemote && size > 1 && health == 0)
        {
            for (int i = 0; i < 4; ++i)
            {
                float offsetX = ((i % 2) - 0.5F) * size / 4.0F;
                float offsetY = ((i / 2) - 0.5F) * size / 4.0F;
                EntityMagmaCube magmaCube = new(world);
                magmaCube.setSlimeSize(size / 2);
                magmaCube.setPositionAndAnglesKeepPrevAngles(x + offsetX, y + 0.5D, z + offsetY, random.NextFloat() * 360.0F, 0.0F);
                world.SpawnEntity(magmaCube);
            }
        }

        base.markDead();
    }
}
