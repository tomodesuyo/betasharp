using BetaSharp.Worlds.Core.Systems;
using BetaSharp.Worlds.Generation.Generators.Features;
using BetaSharp.Util.Maths;
using BetaSharp.Entities;

namespace BetaSharp.Worlds.Generation.Biomes;

internal sealed class EndBiomeDecorator : BiomeDecorator
{
    private readonly EndSpikeFeature _spikeFeature = new();

    public override void Decorate(IWorldContext world, JavaRandom random, Biome biome, int blockX, int blockZ)
    {
        if (random.NextInt(5) == 0)
        {
            int x = blockX + random.NextInt(16) + 8;
            int z = blockZ + random.NextInt(16) + 8;
            int y = world.Reader.GetTopSolidBlockY(x, z);
            if (y > 0)
            {
                _spikeFeature.Generate(world, random, x, y, z);
            }
        }

        if (blockX == 0 && blockZ == 0 && !world.Properties.HasSpawnedEnderDragon)
        {
            List<EntityDragon> dragons = world.Entities.CollectEntitiesOfType<EntityDragon>(new Box(-192.0D, 0.0D, -192.0D, 192.0D, 256.0D, 192.0D));
            if (dragons.Count == 0)
            {
                EntityDragon dragon = new(world);
                dragon.setPositionAndAngles(0.0D, 128.0D, 0.0D, random.NextFloat() * 360.0F, 0.0F);
                if (world.SpawnEntity(dragon))
                {
                    world.Properties.HasSpawnedEnderDragon = true;
                }
            }
        }
    }
}
