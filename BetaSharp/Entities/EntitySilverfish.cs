using BetaSharp.Blocks;
using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntitySilverfish : EntityMonster
{
    public override EntityType Type => EntityRegistry.Silverfish;

    public EntitySilverfish(IWorldContext world) : base(world)
    {
        texture = "/mob/silverfish.png";
        setBoundingBoxSpacing(0.3F, 0.7F);
        movementSpeed = 0.6F;
        attackStrength = 1;
        health = 8;
    }

    protected override Entity? findPlayerToAttack()
    {
        EntityPlayer? player = world.Entities.GetClosestPlayerTarget(x, y, z, 8.0D);
        return player != null && canSee(player) ? player : null;
    }

    protected override void attackEntity(Entity entity, float distance)
    {
        if (attackTime <= 0 && distance < 1.2F && entity.boundingBox.MaxY > boundingBox.MinY && entity.boundingBox.MinY < boundingBox.MaxY)
        {
            attackTime = 20;
            entity.damage(this, attackStrength);
        }
    }

    protected override float getBlockPathWeight(int x, int y, int z)
    {
        return world.Reader.GetBlockId(x, y - 1, z) == Block.Stone.id ? 10.0F : base.getBlockPathWeight(x, y, z);
    }

    public override bool canSpawn()
    {
        return base.canSpawn() && world.Entities.GetClosestPlayer(x, y, z, 5.0D) == null;
    }
}
