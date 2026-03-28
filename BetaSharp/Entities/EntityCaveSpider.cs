using BetaSharp.Worlds.Core.Systems;

namespace BetaSharp.Entities;

public sealed class EntityCaveSpider : EntitySpider
{
    public override EntityType Type => EntityRegistry.CaveSpider;

    public EntityCaveSpider(IWorldContext world) : base(world)
    {
        texture = "/mob/cavespider.png";
        setBoundingBoxSpacing(0.7F, 0.5F);
        health = 12;
        attackStrength = 2;
    }

    public override string getTexture() => "/mob/cavespider.png";
}
